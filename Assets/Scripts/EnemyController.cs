using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {
    public int nVertices;
    public int radius;

    // used by EnemyManager
    private FaultLine killFlag;
    public FaultLine KillFlag {
        get {
            return killFlag;
        }
        set {
            killFlag = value;
        }
    }
    private Quadrant[] quadrants;
    public Quadrant[] Quadrants {
        get {
            return quadrants;
        }
        set {
            quadrants = value;
        }
    }

    // used for procedurally generated mesh
    private Vector3[] vertices;
    public Vector3[] Vertices {
        get {
            return vertices;
        }
    }
    private Vector2[] uv;
    private int[] triangles;

    /*
     * initialization in Awake() because need to ensure mesh is built
     * by the time the EnemyManager needs to access it.
     */
    void Awake() {
        killFlag = FaultLine.None;
        quadrants = new Quadrant[nVertices];

        // create vertices
        vertices = new Vector3[nVertices];
        uv = new Vector2[nVertices];
        float angleIncrement = -2*Mathf.PI/nVertices;
        float angle = 0;
        for (int i = 0; i < nVertices; i++) {
            vertices[i] = new Vector3(radius*Mathf.Cos(angle), 0, radius*Mathf.Sin(angle));
            uv[i] = new Vector2(radius*Mathf.Cos(angle), radius*Mathf.Sin(angle));
            angle += angleIncrement;
        }

        // create triangles
        triangles = new int[3*(nVertices - 2)];
        for (int i = 0; i < nVertices - 2; i++) {
            triangles[3*i] = 0;
            triangles[3*i + 1] = i + 1;
            triangles[3*i + 2] = i + 2;
        }

        // add vertex data to mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {

    }

    public void Kill() {
        /*
         * On death, the enemy breaks into 2 pieces along the fault line.
         * The 2 pieces are instantiated, and this object is destroyed.
         * The 2 pieces are given an impulse proportional to direction of the ground shift
         * and also there is a component out of the plane.
         * There is also some torque applied.
         * Physics simulation with a lot of air drag is ran on the piece.
         * The result is a piece that pops out of the plane, but slows its movement over time.
         * It fades out and is destroyed.
         */
        Object.Destroy(this.gameObject);
    }
}
