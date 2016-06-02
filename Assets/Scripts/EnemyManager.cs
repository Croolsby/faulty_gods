using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour, OnFireEventHandler {
    public PlayerController player;
    public GameObject enemyPrefab;
    public GameObject deathPiecePrefab;
    public float maxSpeed;
    public float repulsion;
    public float forceMagnitude;
    public float torqueMagnitude;

    private List<EnemyController> enemies;
    private float accumalatedTime;

    // Use this for initialization
    void Start() {
        enemies = new List<EnemyController>();
        accumalatedTime = 0;
        InputController.AddOnFireEventHandler(this);
    }

    // Update is called once per frame
    void Update() {
        // spawning on a timer
        accumalatedTime += Time.deltaTime;
        if (accumalatedTime >= 0.1f) {
            accumalatedTime -= 0.1f;
            Spawn();
        }
    }

    /*
     * called by InputController
     */
    public void OnFireEvent() {
        CategorizeAndFlag();
        KillFlagged();
    }

    public void Spawn() {
        if (enemies.Count < 1) {
            GameObject enemy = Instantiate<GameObject>(enemyPrefab);
            enemies.Add(enemy.GetComponent<EnemyController>());
            //print("enemies.Count: " + enemies.Count);
        }
    }

    /*
     * Finds the exactly where the fault line intersects the enemy,
     * detroys the enemy,
     * then creates two new objects representing the two halves.
     */
    public void KillFlagged() {
        List<EnemyController> newList = new List<EnemyController>();
        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i].KillFlag != FaultLine.None) {
                // goal: find the two pairs of vertices (aka the edges) that cross the fault line
                // idea: traverse vertices in order, take note when quadrant switches
                EnemyController enemy = enemies[i];
                Vector3[] edge1 = new Vector3[2];
                Vector3[] edge2 = new Vector3[2];
                int nEdgesFound = 0;
                int nPointsInQuad1 = 0;
                int nPointsInQuad2 = 0;
                Quadrant quad1 = Quadrant.None;
                Quadrant quad2 = Quadrant.None;
                int startIndexOfPiece1 = -1;
                for (int j = 0; j < enemy.nVertices; j++) {
                    if (nEdgesFound == 0) {
                        nPointsInQuad1++;
                    } else if (nEdgesFound == 1) {
                        nPointsInQuad2++;
                    } else if (nEdgesFound == 2) {
                        nPointsInQuad1++;
                    } else {
                        Debug.LogError("More than two edges found.");
                    }
                    if (enemy.Quadrants[j] != enemy.Quadrants[(j + 1) % enemy.nVertices]) {
                        if (nEdgesFound == 0) {
                            edge1[0] = enemy.Vertices[j];
                            edge1[1] = enemy.Vertices[(j + 1) % enemy.nVertices];
                            nEdgesFound++;
                            quad1 = enemy.Quadrants[j];
                            quad2 = enemy.Quadrants[(j + 1) % enemy.nVertices];
                        } else if (nEdgesFound == 1) {
                            edge2[0] = enemy.Vertices[j];
                            edge2[1] = enemy.Vertices[(j + 1) % enemy.nVertices];
                            nEdgesFound++;
                            startIndexOfPiece1 = (j + 1) % enemy.nVertices;
                        }
                    }
                }
                //print("nPointsInQuad1: " + nPointsInQuad1);
                //print("nPointsInQuad2: " + nPointsInQuad2);

                // get the line that intersects the edges
                // origin
                Vector3 faultO = player.transform.position;
                // direction
                Vector3 faultD = new Vector3();
                if (enemies[i].KillFlag == FaultLine.Front) {
                    faultD = player.transform.rotation * player.Forward;
                } else if (enemies[i].KillFlag == FaultLine.Right) {
                    Vector3 front = player.transform.rotation * player.Forward;
                    faultD = new Vector3(front.z, 0, -front.x);
                } else if (enemies[i].KillFlag == FaultLine.Left) {
                    Vector3 front = player.transform.rotation * player.Forward;
                    faultD = -(new Vector3(front.z, 0, -front.x));
                } else if (enemies[i].KillFlag == FaultLine.Back) {
                    faultD = -(player.transform.rotation * player.Forward);
                }

                // find the intersection
                // using equation for line intersection:
                //      edgeO + s * edgeD = faultO + t * faultD
                // also need to define edge relative to world
                Vector3 edge1O = edge1[0] + enemy.transform.position;
                Vector3 edge1D = edge1[1] - edge1[0];
                Vector3 edge2O = edge2[0] + enemy.transform.position;
                Vector3 edge2D = edge2[1] - edge2[0];
                float s1 = (faultD.x*edge1O.z - faultD.z*edge1O.x + faultD.z*faultO.x - faultD.x*faultO.z) 
                    / (faultD.z*edge1D.x - faultD.x*edge1D.z);
                float s2 = (faultD.x*edge2O.z - faultD.z*edge2O.x + faultD.z*faultO.x - faultD.x*faultO.z) 
                    / (faultD.z*edge2D.x - faultD.x*edge2D.z);

                // use intersections to create halves (each half is an array of vertices)
                // also need to define edge intersection relative to enemy center
                Vector3 edge1I = edge1O + s1 * edge1D - enemy.transform.position;
                Vector3 edge2I = edge2O + s2 * edge2D - enemy.transform.position;
                Vector3[] vertices1 = new Vector3[nPointsInQuad1 + 2];
                Vector3[] vertices2 = new Vector3[nPointsInQuad2 + 2];

                vertices1[0] = edge2I;
                vertices2[0] = edge1I;
                int vertices1Counter = 1;
                int vertices2Counter = 1;
                int originalCounter = startIndexOfPiece1;
                for (int j = 0; j < enemy.nVertices; j++) {
                    if (enemy.Quadrants[originalCounter] == quad1) {
                        vertices1[vertices1Counter] = enemy.Vertices[originalCounter];
                        vertices1Counter++;
                        originalCounter = (originalCounter + 1) % enemy.nVertices;
                    } else if (enemy.Quadrants[originalCounter] == quad2) {
                        vertices2[vertices2Counter] = enemy.Vertices[originalCounter];
                        vertices2Counter++;
                        originalCounter = (originalCounter + 1) % enemy.nVertices;
                    } else {
                        Debug.LogError("Enemy is in more than 2 quadrants.");
                    }
                }
                vertices1[vertices1.Length - 1] = edge1I;
                vertices2[vertices2.Length - 1] = edge2I;

                // recenter
                Vector3 center1 = new Vector3();
                Vector3 center2 = new Vector3();
                for (int j = 0; j < vertices1.Length; j++) {
                    center1 += vertices1[j] + enemy.transform.position;
                }
                center1 /= vertices1.Length;
                for (int j = 0; j < vertices2.Length; j++) {
                    center2 += vertices2[j] + enemy.transform.position;
                }
                center2 /= vertices2.Length;
                for (int j = 0; j < vertices1.Length; j++) {
                    vertices1[j] = vertices1[j] + enemy.transform.position - center1;
                }
                for (int j = 0; j < vertices2.Length; j++) {
                    vertices2[j] = vertices2[j] + enemy.transform.position - center2;
                }

                //print("vertices1: ");
                //for (int j = 0; j < vertices1.Length; j++) {
                //    print(vertices1[j].ToString());
                //}
                //print("vertices2: ");
                //for (int j = 0; j < vertices2.Length; j++) {
                //    print(vertices2[j].ToString());
                //}

                // instantiate death pieces and apply forces to them
                // note that Awake() is called during instantiation (during this frame update)
                // and Start() is called at the next frame update.
                DeathPieceController piece1 = Instantiate<GameObject>(deathPiecePrefab).GetComponent<DeathPieceController>();
                DeathPieceController piece2 = Instantiate<GameObject>(deathPiecePrefab).GetComponent<DeathPieceController>();
                piece1.Vertices = vertices1;
                piece2.Vertices = vertices2;
                piece1.transform.position = center1;
                piece2.transform.position = center2;

                // transfer material from parent
                piece1.GetComponent<Renderer>().material = enemy.GetComponent<Renderer>().material;
                piece2.GetComponent<Renderer>().material = enemy.GetComponent<Renderer>().material;

                // physics
                Rigidbody rb1 = piece1.GetComponent<Rigidbody>();
                Rigidbody rb2 = piece2.GetComponent<Rigidbody>();
                Vector3 force1;
                Vector3 force2;
                Vector3 torque1;
                Vector3 torque2;
                faultD.Normalize();
                Vector3 faultT = new Vector3(faultD.z, 0, -faultD.x);
                switch (enemies[i].KillFlag) {
                    case FaultLine.Front:
                        force1 = forceMagnitude * Vector3.up + player.faultMagnitude * faultT;
                        force2 = forceMagnitude * Vector3.up - player.faultMagnitude * faultT;
                        torque1 = torqueMagnitude * faultD;
                        torque2 = -torqueMagnitude * faultD;
                        if (quad1 == Quadrant.FrontRight) {
                            rb1.AddForce(force1);
                            rb2.AddForce(force2);
                            rb1.AddTorque(torque1);
                            rb2.AddTorque(torque2);
                        } else {
                            rb2.AddForce(force1);
                            rb1.AddForce(force2);
                            rb2.AddTorque(torque1);
                            rb1.AddTorque(torque2);
                        }
                        break;
                    case FaultLine.Right:
                        force1 = forceMagnitude * Vector3.up - player.faultMagnitude * faultD;
                        force2 = forceMagnitude * Vector3.up + player.faultMagnitude * faultD;
                        torque1 = torqueMagnitude * faultT;
                        torque2 = -torqueMagnitude * faultT;
                        if (quad1 == Quadrant.BackRight) {
                            rb1.AddForce(force1);
                            rb2.AddForce(force2);
                            rb1.AddTorque(torque1);
                            rb2.AddTorque(torque2);
                        } else {
                            rb2.AddForce(force1);
                            rb1.AddForce(force2);
                            rb2.AddTorque(torque1);
                            rb1.AddTorque(torque2);
                        }
                        break;
                    case FaultLine.Left:
                        force1 = forceMagnitude * Vector3.up + player.faultMagnitude * faultD;
                        force2 = forceMagnitude * Vector3.up - player.faultMagnitude * faultD;
                        torque1 = -torqueMagnitude * faultT;
                        torque2 = torqueMagnitude * faultT;
                        if (quad1 == Quadrant.FrontLeft) {
                            rb1.AddForce(force1);
                            rb2.AddForce(force2);
                            rb1.AddTorque(torque1);
                            rb2.AddTorque(torque2);
                        } else {
                            rb2.AddForce(force1);
                            rb1.AddForce(force2);
                            rb2.AddTorque(torque1);
                            rb1.AddTorque(torque2);
                        }
                        break;
                    case FaultLine.Back:
                        //force1 = forceMagnitude * Vector3.up - player.faultMagnitude * faultT;
                        //force2 = forceMagnitude * Vector3.up + player.faultMagnitude * faultT;
                        force1 = forceMagnitude * Vector3.up + player.faultMagnitude / 10 * faultT;
                        force2 = forceMagnitude * Vector3.up - player.faultMagnitude / 10 * faultT;
                        torque1 = torqueMagnitude * faultD;
                        torque2 = -torqueMagnitude * faultD;
                        if (quad1 == Quadrant.BackLeft) {
                            rb1.AddForce(force1);
                            rb2.AddForce(force2);
                            rb1.AddTorque(torque1);
                            rb2.AddTorque(torque2);
                        } else {
                            rb2.AddForce(force1);
                            rb1.AddForce(force2);
                            rb2.AddTorque(torque1);
                            rb1.AddTorque(torque2);
                        }
                        break;
                }
                Destroy(enemy.gameObject);
            } else if (enemies[i].KillFlag == FaultLine.None) {
                newList.Add(enemies[i]);
            }
        }
        enemies = newList;
    }

    private void CategorizeAndFlag() {
        /*
         * how to find intersection of convex polygon with ray in 2d:
         *      (using the word 'ray' to mean vector in coordinates relative to player position)
         * main idea: if no intersection, then all vertices are on the same side of the ray.
         *      so all dot products will have the same sign.
         *      otherwise, intersection.
         * implementation: working in coordinates relative to player position,
         *      take dot product of vector each vertex with ray and ray tangent
         *      use sign of results to tell which quadrant the vertex is in
         *      if the quadrant changes while processing an enemy's vertices,
         *      then flag that enemy to be killed.
         *      Later, when processing flagged enemies, we will figure out how to split the enemy.
         *      Also later, since we saved what quadrant each vertex was in, we can easily
         *      figure out which way to push them (because the ground shifts).
         */
        EnemyController enemy;
        Vector3 front = player.transform.rotation * player.Forward;
        Vector3 right = new Vector3(front.z, 0, -front.x);
        Vector3 rayVertex;
        float frontDot;
        float rightDot;
        Quadrant firstAssigned = Quadrant.None;
        for (int j = 0; j < enemies.Count; j++) {
            enemy = enemies[j];
            for (int i = 0; i < enemy.nVertices; i++) {
                // compute dot products
                rayVertex = ((enemy.transform.position + enemy.Vertices[i]) - player.transform.position);
                frontDot = Vector3.Dot(rayVertex, front);
                rightDot = Vector3.Dot(rayVertex, right);

                // assign quadrant
                if (frontDot >= 0 && rightDot >= 0) {
                    enemy.Quadrants[i] = Quadrant.FrontRight;
                } else if (frontDot >= 0 && rightDot < 0) {
                    enemy.Quadrants[i] = Quadrant.FrontLeft;
                } else if (frontDot < 0 && rightDot >= 0) {
                    enemy.Quadrants[i] = Quadrant.BackRight;
                } else if (frontDot < 0 && rightDot < 0) {
                    enemy.Quadrants[i] = Quadrant.BackLeft;
                } else {
                    // should be unreachable
                    Debug.LogError("Unhandled dot product result.");
                }

                // check for intersection
                if (i == 0) {
                    firstAssigned = enemy.Quadrants[i];
                } else {
                    if (enemy.Quadrants[i] != firstAssigned) {
                        // intersection found
                        if (firstAssigned == Quadrant.FrontRight) {
                            if (enemy.Quadrants[i] == Quadrant.FrontLeft) {
                                enemy.KillFlag = FaultLine.Front;
                            } else if (enemy.Quadrants[i] == Quadrant.BackRight) {
                                enemy.KillFlag = FaultLine.Right;
                            } else {
                                Debug.LogError("Unhandled intersection.");
                            }
                        } else if (firstAssigned == Quadrant.FrontLeft) {
                            if (enemy.Quadrants[i] == Quadrant.FrontRight) {
                                enemy.KillFlag = FaultLine.Front;
                            } else if (enemy.Quadrants[i] == Quadrant.BackLeft) {
                                enemy.KillFlag = FaultLine.Left;
                            } else {
                                Debug.LogError("Unhandled intersection.");
                            }
                        } else if (firstAssigned == Quadrant.BackLeft) {
                            if (enemy.Quadrants[i] == Quadrant.FrontLeft) {
                                enemy.KillFlag = FaultLine.Left;
                            } else if (enemy.Quadrants[i] == Quadrant.BackRight) {
                                enemy.KillFlag = FaultLine.Back;
                            } else {
                                Debug.LogError("Unhandled intersection.");
                            }
                        } else if (firstAssigned == Quadrant.BackRight) {
                            if (enemy.Quadrants[i] == Quadrant.FrontRight) {
                                enemy.KillFlag = FaultLine.Right;
                            } else if (enemy.Quadrants[i] == Quadrant.BackLeft) {
                                enemy.KillFlag = FaultLine.Back;
                            } else {
                                Debug.LogError("Unhandled intersection.");
                            }
                        } else {
                            // should be unreachable
                            Debug.LogError("Unhandled intersection.");
                        }
                    }
                }
            }
        }
    }
}
