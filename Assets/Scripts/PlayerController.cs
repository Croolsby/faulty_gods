using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public int nVertices;
    public float radius;
    public float maxSpeed;
    public float maxRotation;
    public float faultMagnitude;

    // moveDir used to limit rotational velocity
    private Vector3 moveDir;
    public Vector3 MoveDir {
        get {
            return moveDir;
        }
    }
    // moveSpeed used as a Vector3 for linear interpolation
    // but it should be dereferenced by its magnitude for actual speed
    private Vector3 moveSpeed;
    // forward is readonly but needs to be initialized in Start()
    private Vector3 forward;
    public Vector3 Forward {
        get {
            return forward;
        }
    }

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    void Awake() {
        moveDir = new Vector3();
        moveSpeed = new Vector3();
        forward = transform.rotation * new Vector3(1, 0, 0);

        // create vertices
        vertices = new Vector3[4];
        vertices[0] = new Vector3(1, 0, 0);
        vertices[1] = new Vector3(0, 0, 0.5f);
        vertices[2] = new Vector3(-0.618f, 0, 0);
        vertices[3] = new Vector3(0, 0, -0.5f);

        uv = new Vector2[4];
        for (int i = 0; i < vertices.Length; i++) {
            uv[i] = vertices[i];
        }

        triangles = new int[3 * 2];
        for (int i = 0; i < 2; i++) {
            triangles[3*i] = 0;
            triangles[3*i + 1] = i + 2;
            triangles[3*i + 2] = i + 1;
        }

        //vertices = new Vector3[nVertices];
        //uv = new Vector2[nVertices];
        //float angleIncrement = -2*Mathf.PI/nVertices;
        //float angle = 0;
        //for (int i = 0; i < nVertices; i++) {
        //    vertices[i] = new Vector3(radius*Mathf.Cos(angle), 0, radius*Mathf.Sin(angle));
        //    uv[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        //    angle += angleIncrement;
        //}

        //// create triangles
        //triangles = new int[3*(nVertices - 2)];
        //for (int i = 0; i < nVertices - 2; i++) {
        //    triangles[3*i] = 0;
        //    triangles[3*i + 1] = i + 1;
        //    triangles[3*i + 2] = i + 2;
        //}

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

    void FixedUpdate() {

    }

    // Update is called once per frame
    void Update() {
        // movement handling
        if (InputController.leftStick.magnitude != 0) {
            // RotateTowards to simulate turning weight
            moveDir = Vector3.RotateTowards(moveDir, InputController.leftStick, Mathf.PI/180*maxRotation, maxSpeed);
            // MoveTowards to simulate slowing down while turning sharply
            // maxDistance for MoveTowards is proportional to arc length of RotateTowards
            moveSpeed = Vector3.MoveTowards(moveSpeed, maxSpeed * InputController.leftStick, maxSpeed/1.5f * Mathf.PI/180*maxRotation);
            //print("rotation: " + (moveDir - InputController.leftStick).magnitude + ", speed: " + moveSpeed.magnitude);

            // apply velocity to transform
            transform.position += Time.deltaTime * moveSpeed.magnitude * moveDir;

            // rotate transform so that constant forward direction is inline with velocity
            Quaternion fromTo = Quaternion.FromToRotation(forward, moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, fromTo, maxRotation);
        }

        // attack handling
        if (InputController.fireButton) {
        
        }
    }
}
