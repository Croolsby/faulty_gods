using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundManager : MonoBehaviour, OnFireEventHandler {
    public float initialWidth;
    public float initialHeight;
    public PlayerController player;
    public GameObject groundPrefab;
    public float faultScaling;
    public float moveDistance;

    private List<GroundController> grounds;

    // Use this for initialization
    void Start() {
        InputController.AddOnFireEventHandler(this);
        grounds = new List<GroundController>();
        GroundController gc = Instantiate<GameObject>(groundPrefab).GetComponent<GroundController>();
        grounds.Add(gc);
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-initialWidth/2, 0, -initialHeight/2);
        vertices[1] = new Vector3(-initialWidth/2, 0, initialHeight/2);
        vertices[2] = new Vector3(initialWidth/2, 0, initialHeight/2);
        vertices[3] = new Vector3(initialWidth/2, 0, -initialHeight/2);
        gc.Init(vertices);
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnFireEvent() {
        /*
         * first cut all grounds along the fault line.
         * this is a similar problem as cutting enemies.
         */
        Categorize();
        IntersectAndSplit();
        //Categorize();
        //Push();
    }

    private void Push() {
        GroundController ground;
        for (int i = 0; i < grounds.Count; i++) {
            ground = grounds[i];
            Vector3 moveDir = new Vector3();
            Vector3 forward = player.transform.rotation * player.Forward;
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Quadrant quad = ground.Quadrants[0];
            switch (quad) {
                case Quadrant.FrontRight:
                case Quadrant.BackLeft:
                    moveDir = right;
                    break;
                case Quadrant.BackRight:
                case Quadrant.FrontLeft:
                    moveDir = -right;
                    break;
            }

            List<Vector3> vertsOnFault = new List<Vector3>();
            List<Vector3> destsOfVertsOnFault = new List<Vector3>();
            for (int j = 0; j < ground.Vertices.Length; j++) {
                quad = ground.Quadrants[j];
                switch (quad) {
                    case Quadrant.FrontRight:
                    case Quadrant.BackLeft:
                        moveDir = right;
                        break;
                    case Quadrant.BackRight:
                    case Quadrant.FrontLeft:
                        moveDir = -right;
                        break;
                }
                // get vertex distance to fault line
                //Vector3 proj = new Vector3();
                //switch (quad) {
                //    case Quadrant.FrontRight:
                //    case Quadrant.FrontLeft:
                //        proj = Vector3.Project(ground.Vertices[j] + ground.transform.position - player.transform.position, forward);
                //        break;
                //    case Quadrant.BackRight:
                //    case Quadrant.BackLeft:
                //        proj = Vector3.Project(ground.Vertices[j] + ground.transform.position - player.transform.position, -forward);
                //        break;
                //}
                float distFromFault = Mathf.Abs(Vector3.Dot(ground.Vertices[j] + ground.transform.position - player.transform.position, right + player.transform.position));
                //float distFromFault = (ground.Vertices[j] + ground.transform.position - player.transform.position - proj).magnitude;
                print(distFromFault);
                float distToMove = moveDistance;
                switch (quad) {
                    case Quadrant.FrontRight:
                    case Quadrant.FrontLeft:
                        if (distFromFault < 0.0000001) {
                            print("zero");
                            vertsOnFault.Add(ground.Vertices[j]);
                            destsOfVertsOnFault.Add(moveDir * distToMove + ground.Vertices[j] + ground.transform.position);
                        }
                        break;
                    case Quadrant.BackRight:
                    case Quadrant.BackLeft:
                        if (distFromFault <= moveDistance) {
                            distToMove = distFromFault;
                        }
                        break;
                }
                ground.Targets[j] = moveDir * distToMove + ground.Vertices[j] + ground.transform.position;
                print("dest: " + ground.Targets[j]);
            }

            List<Vector3> newVerts = new List<Vector3>();
            List<Vector3> newDests = new List<Vector3>();
            for (int j = 0; j < vertsOnFault.Count; j++) {
                newVerts.Add(vertsOnFault[j]);
                newDests.Add(vertsOnFault[j]);
            }
            for (int j = vertsOnFault.Count - 1; j >= 0; j--) {
                newVerts.Add(vertsOnFault[j]);
                newDests.Add(destsOfVertsOnFault[j]);
            }

            //GroundController gc = Instantiate<GameObject>(groundPrefab).GetComponent<GroundController>();
            //gc.Vertices = newVerts.ToArray();
            //gc.InitQuadrants(newVerts.Count);
            //gc.Dest = newDests.ToArray();
            //grounds.Add(gc);
        }
    }

    /*
     * Takes in a line and a convex polygon and returns
     * the original polygon if the polygon does not intersect with the line
     * or 2 polygons that are the result of the line intersection.
     * 
     * This algorithm needs to not split polygons if a vertex lies on the line, but no edges cross it.
     */
    private List<Vector3[]> Split(Vector3 origin, Vector3 direction, Vector3[] polygon) {
        List<Vector3[]> result = new List<Vector3[]>();



        return result;
    }

    private bool IsCrossing(Quadrant current, Quadrant next, Quadrant nextNext) {
        // assumes that next is on a fault line.
        bool crossing = false;
        switch (next) {
            case Quadrant.Center:
                return IsCrossingBackFrontFault(current, next, nextNext) || IsCrossingLeftRightFault(current, next, nextNext);
            case Quadrant.Back:
            case Quadrant.Front:
                return IsCrossingBackFrontFault(current, next, nextNext);
            case Quadrant.Left:
            case Quadrant.Right:
                return IsCrossingLeftRightFault(current, next, nextNext);
        }
        return crossing;
    }

    /*
     * returns true if next is the crossing point between
     * current and nextNext on the back or front fault lines.
     */
    private bool IsCrossingBackFrontFault(Quadrant current, Quadrant next, Quadrant nextNext) {
        switch (next) {
            case Quadrant.Front:
            case Quadrant.Center:
            case Quadrant.Back:
                switch (current) {
                    case Quadrant.FrontRight:
                    case Quadrant.Right:
                    case Quadrant.BackRight:
                        switch (nextNext) {
                            case Quadrant.BackLeft:
                            case Quadrant.Left:
                            case Quadrant.FrontLeft:
                                return true;
                        }
                        break;
                    case Quadrant.BackLeft:
                    case Quadrant.Left:
                    case Quadrant.FrontLeft:
                        switch (nextNext) {
                            case Quadrant.FrontRight:
                            case Quadrant.Right:
                            case Quadrant.BackRight:
                                return true;
                        }
                        break;
                }
                break;
        }
        return false;
    }

    /*
     * returns true if next is the crossing point between
     * current and nextNext on the left or right fault lines.
     */
    private bool IsCrossingLeftRightFault(Quadrant current, Quadrant next, Quadrant nextNext) {
        switch (next) {
            case Quadrant.Left:
            case Quadrant.Center:
            case Quadrant.Right:
                switch (current) {
                    case Quadrant.FrontLeft:
                    case Quadrant.Front:
                    case Quadrant.FrontRight:
                        switch (nextNext) {
                            case Quadrant.BackRight:
                            case Quadrant.Back:
                            case Quadrant.BackLeft:
                                return true;
                        }
                        break;
                    case Quadrant.BackRight:
                    case Quadrant.Back:
                    case Quadrant.BackLeft:
                        switch (nextNext) {
                            case Quadrant.FrontLeft:
                            case Quadrant.Front:
                            case Quadrant.FrontRight:
                                return true;
                        }
                        break;
                }
                break;
        }
        return false;
    }

    /*
     * Returns true if the test point is inside the convex polygon.
     */
    private bool IsInside(Vector2 point, Vector2[] convexPoly) {
        Vector3 firstCross = Vector3.Cross(convexPoly[0] - point, convexPoly[1] - point);
        Vector3 cross;
        for (int i = 1; i < convexPoly.Length; i++) {
            cross = Vector3.Cross(convexPoly[i] - point, convexPoly[(i + 1) % convexPoly.Length] - point);
            if (Mathf.Sign(Vector3.Dot(firstCross, cross)) == -1) {
                return false;
            }
        }
        return true;
    }

    /*
     * Attempt to intersect the line between v1 and v2 with all four fault lines.
     * Some edges may intersect two fault lines.
     * Result has points in order that fault lines were tried.
     * Fault lines are tried in clockwise order.
     * This does not allow intersections with the end points.
     * That should be handled in the calling function.
     */
    private List<IntersectionPoint> FindIntersectionPoints(Vector3 v1, Vector3 v2) {
        //print("input vectors for intersection: ");
        //print(v1.ToString());
        //print(v2.ToString());
        List<IntersectionPoint> result = new List<IntersectionPoint>();
        List<Vector3> resultPoints = new List<Vector3>();
        List<Quadrant> resultFaults = new List<Quadrant>();

        Vector2 edgeOrigin = new Vector2(v1.x, v1.z);
        Vector2 edgeDirection = new Vector2((v2 - v1).x, (v2 - v1).z);

        // check intersection with front fault
        Vector2 faultOrigin = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 faultDirection = new Vector2((player.transform.rotation * player.Forward).x, (player.transform.rotation * player.Forward).z);

        // using equation for line intersection:
        //      edgeO + s * edgeD = faultO + t * faultD, 0 <= s <= 1
        float s = -1;
        if (faultDirection.y*edgeDirection.x - faultDirection.x*edgeDirection.y != 0) {
            s = (faultDirection.x*edgeOrigin.y - faultDirection.y*edgeOrigin.x + faultDirection.y*faultOrigin.x - faultDirection.x*faultOrigin.y) 
                    / (faultDirection.y*edgeDirection.x - faultDirection.x*edgeDirection.y);
        }

        // check if intersection is real
        // also, this is the only time we allow the center to intersect
        Vector2 p = edgeOrigin + s * edgeDirection;
        if (!Mathf.Approximately(s, 0) && !Mathf.Approximately(s, 1) && 0 < s && s < 1 && Vector2.Dot(faultDirection, p - faultOrigin) >= 0) {
            resultPoints.Add(new Vector3(p.x, 0, p.y));
            resultFaults.Add(Quadrant.Front);
        }

        // check intersection with other 3 faults

        for (int i = 0; i < 3; i++) {
            // turn 90 degrees to the right
            faultDirection = new Vector2(faultDirection.y, -faultDirection.x);

            s = -1;
            if (faultDirection.y*edgeDirection.x - faultDirection.x*edgeDirection.y != 0) {
                s = (faultDirection.x*edgeOrigin.y - faultDirection.y*edgeOrigin.x + faultDirection.y*faultOrigin.x - faultDirection.x*faultOrigin.y) 
                    / (faultDirection.y*edgeDirection.x - faultDirection.x*edgeDirection.y);
            }

            p = edgeOrigin + s * edgeDirection;
            if (!Mathf.Approximately(s, 0) && !Mathf.Approximately(s, 1) && 0 < s && s < 1 && Vector2.Dot(faultDirection, p - faultOrigin) > 0) {
                // intersection within edge found
                resultPoints.Add(new Vector3(p.x, 0, p.y));
                switch (i) {
                    case 0:
                        resultFaults.Add(Quadrant.Right);
                        break;
                    case 1:
                        resultFaults.Add(Quadrant.Back);
                        break;
                    case 2:
                        resultFaults.Add(Quadrant.Left);
                        break;
                }
            }
        }

        if (resultPoints.Count > 2) {
            Debug.LogError("Found more than 2 intersection.");
            foreach (Vector3 v in resultPoints) {
                print(v.ToString());
            }
        }

        for (int i = 0; i < resultPoints.Count; i++) {
            result.Add(new IntersectionPoint(resultPoints[i], resultFaults[i]));
        }

        return result;
    }

    /*
     * Determines which quadrant relative to the player each vertex is in.
     * If a vertex is on the fault line, there are special quadrants that represent that.
     * See Enums.cs
     */
    private void Categorize() {
        GroundController ground;
        Quaternion inverseRotation = Quaternion.Inverse(player.transform.rotation);
        Vector3 inverseTranslation = -player.transform.position;
        Vector3 vertex;
        Quadrant original;
        for (int i = 0; i < grounds.Count; i++) {
            ground = grounds[i];
            if (ground == null || ground.Quadrants == null) {
                continue;
            }
            for (int j = 0; j < ground.Vertices.Length; j++) {
                original = ground.Quadrants[j];
                // change of basis from coordinates relative to world to relative to player
                vertex = ground.Vertices[j];
                vertex += inverseTranslation;
                vertex = inverseRotation * vertex;
                
                // x is how far in front of the player
                // z is how far to the left of the player
                float epsilon = 0.001f;
                if (Mathf.Abs(vertex.x) < epsilon && Mathf.Abs(vertex.z) < epsilon) {
                    ground.Quadrants[j] = Quadrant.Center;
                } else if (Mathf.Abs(vertex.x) < epsilon && vertex.z > 0) {
                    ground.Quadrants[j] = Quadrant.Left;
                } else if (Mathf.Abs(vertex.x) < epsilon && vertex.z < 0) {
                    ground.Quadrants[j] = Quadrant.Right;
                } else if (vertex.x > 0 && Mathf.Abs(vertex.z) < epsilon) {
                    ground.Quadrants[j] = Quadrant.Front;
                } else if (vertex.x < 0 && Mathf.Abs(vertex.z) < epsilon) {
                    ground.Quadrants[j] = Quadrant.Back;
                } else if (vertex.x > 0 && vertex.z > 0) {
                    ground.Quadrants[j] = Quadrant.FrontLeft;
                } else if (vertex.x > 0 && vertex.z < 0) {
                    ground.Quadrants[j] = Quadrant.FrontRight;
                } else if (vertex.x < 0 && vertex.z > 0) {
                    ground.Quadrants[j] = Quadrant.BackLeft;
                } else if (vertex.x < 0 && vertex.z < 0) {
                    ground.Quadrants[j] = Quadrant.BackRight;
                }

                //if (original != ground.Quadrants[j]) {
                //    print("difference: " + Mathf.Abs(vertex.x) + ", " + Mathf.Abs(vertex.z));
                //    print(original + " -> " + ground.Quadrants[j]);
                //}
            }
        }
    }

    /*
     * Next the intersections are found, and the new points are generated
     * as soon as an intersecting edge is found.
     * Also, while traversing the edges, the vertices are placed into
     * into a new arrays based on quadrant. When an intersection point is found, two copies of the point are
     * added in the correct arrays. 
     */
    private void IntersectAndSplit() {
        List<GroundController> newList = new List<GroundController>();
        GroundController ground;
        for (int i = 0; i < grounds.Count; i++) {
            ground = grounds[i];
            if (ground == null || ground.Quadrants == null) {
                // the ground probably destroyed itself because of 0 area
                continue;
            }

            // find index of the beginning of a string of vertices of common quadrant
            int firstSeen = 0;
            Quadrant current;
            Quadrant next;
            for (int j = 0; j < ground.Vertices.Length; j++) {
                current = ground.Quadrants[j];
                next = ground.Quadrants[(j + 1) % ground.Vertices.Length];
                if (current != next) {
                    firstSeen = (j + 1) % ground.Vertices.Length;
                    break;
                }
            }

            // if no firstSeen found, then there are no intersections
            if (firstSeen == 0) {
                newList.Add(ground);
                continue;
            }

            //print("ground.Vertices.Length: " + ground.Vertices.Length);
            //for (int j = 0; j < ground.Vertices.Length; j++) {
            //    print(ground.Vertices[j] + " " + ground.Quadrants[j]);
            //}

            // we will need to know if the player is inside the ground or not
            Vector2 testPoint = new Vector2(player.transform.position.x, player.transform.position.z);
            Vector2[] testPoly = new Vector2[ground.Vertices.Length];
            Vector3 temp;
            for (int j = 0; j < ground.Vertices.Length; j++) {
                temp = ground.Vertices[j];
                testPoly[j] = new Vector2(temp.x, temp.z);
            }
            bool isInside = IsInside(testPoint, testPoly);

            /*
             * create a new array of vertices that is a copy of ground.vertices plus
             * vertices created by the intersection of an edge with a fault line.
             * The final array has all vertices in order if you were to traverse the edges clockwise.
             */
            int k;
            int kNext;
            List<Vector3> newVerts = new List<Vector3>();
            List<Quadrant> newQuads = new List<Quadrant>();
            List<IntersectionPoint> iPoints;
            for (int j = 0; j < ground.Vertices.Length; j++) {
                k = (j) % ground.Vertices.Length;
                kNext = (k + 1) % ground.Vertices.Length;
                current = ground.Quadrants[k];
                next = ground.Quadrants[kNext];

                // append the current vertex
                newVerts.Add(ground.Vertices[k]);
                newQuads.Add(current);

                // check if there is intersection between this vertex and the next
                if (current != next) {
                    // get the intersection points
                    iPoints = FindIntersectionPoints(ground.Vertices[k], ground.Vertices[kNext]);

                    // remove intersection points that are end points
                    for (int t = iPoints.Count - 1; t >= 0; t--) {
                        if (iPoints[t].fault == current || iPoints[t].fault == next) {
                            iPoints.RemoveAt(t);
                        }
                    }

                    //if (iPoints.Count == 2) {
                    //    print("order of points from double intersection:");
                    //    print(iPoints[0].fault);
                    //    print(iPoints[1].fault);
                    //}

                    // if player is outside of polygon, reverse points
                    // this takes advantage of the fact that FindIntersectionPoints
                    // goes through fault lines in clockwise order,
                    // and we traverse edges in clockwise order.
                    if (!isInside && iPoints.Count == 2 && (iPoints[0].fault != Quadrant.Front || iPoints[1].fault != Quadrant.Left)) {
                        iPoints.Add(iPoints[0]);
                        iPoints.RemoveAt(0);
                    } else if (isInside && iPoints.Count == 2 && iPoints[0].fault == Quadrant.Front && iPoints[1].fault == Quadrant.Left) {
                        iPoints.Add(iPoints[0]);
                        iPoints.RemoveAt(0);
                    }

                    // append them once each
                    for (int t = 0; t < iPoints.Count; t++) {
                        newVerts.Add(iPoints[t].point);
                        newQuads.Add(iPoints[t].fault);
                    }
                }
            }

            /*
             * If no new vertices were found, then this ground does not need to be split, so just return it.
             */
            if (newVerts.Count == ground.Vertices.Length) {
                newList.Add(ground);
                continue;
            }

            /*
             * This is a hack:
             * Check for duplicate vertices. There shouldn't be any at this stage.
             */
            List<Vector3> tempVerts = new List<Vector3>();
            List<Quadrant> tempQuads = new List<Quadrant>();
            for (int j = 0; j < newVerts.Count; j++) {
                if (newVerts[j] != newVerts[(j + 1) % newVerts.Count]) {
                    tempVerts.Add(newVerts[j]);
                    tempQuads.Add(newQuads[j]);
                } else {
                    Debug.LogError("Unexpected duplicate.");
                    continue;
                }
            }
            newVerts = tempVerts;
            newQuads = tempQuads;

            //print("newVerts.Count: " + newVerts.Count);
            //for (int j = 0; j < newVerts.Count; j++) {
            //    print(newVerts[j] + " " + newQuads[j]);
            //}

            /*
             * split the polygon along the fault lines.
             * This means that for any vertex that is on a fault line and is a crossing point
             * will need to be duplicated.
             */
            tempVerts = new List<Vector3>();
            tempQuads = new List<Quadrant>();
            Quadrant nextNext;
            int startIndex = -1;
            for (int j = 0; j < newVerts.Count; j++) {
                current = newQuads[j];
                next = newQuads[(j + 1) % newVerts.Count];
                nextNext = newQuads[(j + 2) % newVerts.Count];

                tempVerts.Add(newVerts[j]);
                tempQuads.Add(newQuads[j]);

                switch (next) {
                    case Quadrant.Center:
                    case Quadrant.Front:
                    case Quadrant.Right:
                    case Quadrant.Back:
                    case Quadrant.Left:
                        // in this case we want to know if the next vertex needs to be split or stay.
                        // there are two cases:
                        // 1. the next next point is on the other side of the fault from current
                        // 2. it is not.
                        // so if traversing from current -> next -> next next
                        // we cross a fault line, then we need to add a copy of next.
                        if (IsCrossing(current, next, nextNext)) {
                            tempVerts.Add(newVerts[(j + 1) % newVerts.Count]);
                            tempQuads.Add(next);
                            if (startIndex == -1) {
                                startIndex = (j + 2) % newVerts.Count;
                            }
                        }
                        break;
                }
            }
            newVerts = tempVerts;
            newQuads = tempQuads;
            if (startIndex == -1) {
                startIndex = 0;
            }

            //print("newVerts.Count: " + newVerts.Count);
            //for (int j = 0; j < newVerts.Count; j++) {
            //    print(newVerts[j] + " " + newQuads[j]);
            //}

            /*
             * Now we have an array of vertices that is a copy of the original plus
             * two copies of any vertices that were crossing points of the original edges
             * inserted into the proper place in the array.
             * Also, all vertices have been sorted into the appropriate quadrant (including
             * quadarants on fault lines).
             * To split into children, traverse the vertices starting at startIndex.
             * startIndex is the index of the second of the pair of vertices that got duplicated,
             * or if no vertices got duplicated, it is 0.
             * Each vertex we see goes into the current child array.
             * Any time we encounter a pair of duplicates, we make a new child array the current one.
             * A special case arises if we encounter a pair of duplicates adjecent to another pair of
             * duplicates. This means the original edge was intersected twice.
             * For now we ignore this case, but in this case, we will always obtain 4 child arrays.
             * Any child array with only 2 vertices is the result of this case.
             * So we will either merge children with 2 vertices or add a 3rd vertex.
             */
            List<List<Vector3>> childVerts = new List<List<Vector3>>();
            childVerts.Add(new List<Vector3>());
            List<List<Quadrant>> childQuads = new List<List<Quadrant>>();
            childQuads.Add(new List<Quadrant>());
            for (int j = 0; j < newVerts.Count; j++) {
                k = (j + startIndex) % newVerts.Count;
                kNext = (k + 1) % newVerts.Count;

                // put the vertex with the current child
                childVerts[childVerts.Count - 1].Add(newVerts[k]);
                childQuads[childQuads.Count - 1].Add(newQuads[k]);
                //print("childVerts.Count: " + childVerts.Count);

                // check if next is a duplicate
                if (newVerts[k] == newVerts[kNext] && newVerts[kNext] != newVerts[startIndex]) {
                    // create a new child
                    childVerts.Add(new List<Vector3>());
                    childQuads.Add(new List<Quadrant>());
                }
            }

            //print("childVerts.Count: " + childVerts.Count);
            //for (int j = 0; j < childVerts.Count; j++) {
            //    print("child " + j + ": ");
            //    for (int t = 0; t < childVerts[j].Count; t++) {
            //        print(childVerts[j][t] + " " + childQuads[j][t]);
            //    }
            //}

            /*
             * to handle special case:
             * if player is inside ground, then we ignore the special case. It will be handled in the next step.
             * Otherwise, the player is outside the polygon.
             * Then merge the child with only 2 vertices into the non-adjacent child.
             */
            if (!isInside) {
                int specialCase = -1;
                for (int j = 0; j < childVerts.Count; j++) {
                    if (childVerts[j].Count == 2) {
                        specialCase = j;
                    }
                }
                if (specialCase != -1) {
                    if (childVerts.Count != 4) {
                        Debug.LogError("There should be 4 children for a special case occurence.");
                        continue;
                    }
                    for (int j = 0; j < 2; j++) {
                        childVerts[(specialCase + 2) % childVerts.Count].Add(childVerts[specialCase][j]);
                        childQuads[(specialCase + 2) % childVerts.Count].Add(childQuads[specialCase][j]);
                    }
                    childVerts.RemoveAt(specialCase);
                    childQuads.RemoveAt(specialCase);
                }
            } else {
                /*
                 * if the player is inside, then the center point must be added to each child
                 */
                for (int j = 0; j < childVerts.Count; j++) {
                    childVerts[j].Add(player.transform.position);
                    childQuads[j].Add(Quadrant.Center);
                }
            }

            //print("childVerts.Count: " + childVerts.Count);
            //for (int j = 0; j < childVerts.Count; j++) {
            //    print("child " + j + ": ");
            //    for (int t = 0; t < childVerts[j].Count; t++) {
            //        print(childVerts[j][t] + " " + childQuads[j][t]);
            //    }
            //}

            // now instantiate children
            GroundController gc;
            for (int j = 0; j < childVerts.Count; j++) {
                gc = Instantiate<GameObject>(groundPrefab).GetComponent<GroundController>();
                newList.Add(gc);
                gc.Init(childVerts[j].ToArray());
                gc.Quadrants = childQuads[j].ToArray();
                gc.RandomColor();
            }
            Destroy(ground.gameObject);
        }
        grounds = newList;
    }
    //private void Categorize() {
    //    GroundController ground;
    //    Vector3 front = player.transform.rotation * player.Forward;
    //    Vector3 right = new Vector3(front.z, 0, -front.x);
    //    Vector3 rayVertex;
    //    float frontDot;
    //    float rightDot;
    //    Quadrant original;
    //    for (int j = 0; j < grounds.Count; j++) {
    //        ground = grounds[j];
    //        if (ground == null || ground.Quadrants == null) {
    //            continue;
    //        }
    //        for (int i = 0; i < ground.Vertices.Length; i++) {
    //            original = ground.Quadrants[i];
    //            // compute dot products
    //            rayVertex = (ground.Vertices[i] - player.transform.position);
    //            frontDot = Vector3.Dot(rayVertex, front);
    //            rightDot = Vector3.Dot(rayVertex, right);

    //            // assign quadrant
    //            if (frontDot > 0 && Mathf.Approximately(rightDot, 0)) {
    //                ground.Quadrants[i] = Quadrant.Front;
    //            } else if (frontDot < 0 && Mathf.Approximately(rightDot, 0)) {
    //                ground.Quadrants[i] = Quadrant.Back;
    //            } else if (Mathf.Approximately(frontDot, 0) && rightDot > 0) {
    //                ground.Quadrants[i] = Quadrant.Right;
    //            } else if (Mathf.Approximately(frontDot, 0) && rightDot < 0) {
    //                ground.Quadrants[i] = Quadrant.Left;
    //            } else if (Mathf.Approximately(frontDot, 0) && Mathf.Approximately(rightDot, 0)) {
    //                ground.Quadrants[i] = Quadrant.Center;

    //            } else if (frontDot > 0 && rightDot > 0) {
    //                ground.Quadrants[i] = Quadrant.FrontRight;
    //            } else if (frontDot > 0 && rightDot < 0) {
    //                ground.Quadrants[i] = Quadrant.FrontLeft;
    //            } else if (frontDot < 0 && rightDot > 0) {
    //                ground.Quadrants[i] = Quadrant.BackRight;
    //            } else if (frontDot < 0 && rightDot < 0) {
    //                ground.Quadrants[i] = Quadrant.BackLeft;
    //            } else {
    //                Debug.LogError("frontDot: " + frontDot + ", rightDot: " + rightDot);
    //            }

    //            if (ground.Quadrants[i] != original) {
    //                print("changed quadrant: " + original + " -> " + ground.Quadrants[i]);
    //            }
    //        }
    //    }
    //}
}
