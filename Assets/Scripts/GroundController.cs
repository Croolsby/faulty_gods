using UnityEngine;
using System.Collections;

/*
 * The ground's position never changes, only it's vertices do.
 * Also, it's assumed that transform.x = 0, transform.z = 0;
 * transform.y is a free variable.
 */
public class GroundController : MonoBehaviour {
    public float moveSpeed;

    private Material mat;

    private Vector3[] targets;
    public Vector3[] Targets {
        get {
            return targets;
        }
        set {
            targets = value;
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

    private Vector3[] vertices;
    public Vector3[] Vertices {
        get {
            return vertices;
        }
        set {
            vertices = value;
        }
    }
    private Vector2[] uv;
    private int[] triangles;

    public void Init(Vector3[] vertices) {
        

        if (vertices.Length >= 3) {
            this.vertices = vertices;
            BuildMesh();

            quadrants = new Quadrant[vertices.Length];
            targets = new Vector3[vertices.Length];
            for (int i = 0; i < targets.Length; i++) {
                targets[i] = vertices[i];
            }

            mat = GetComponent<Renderer>().material;
            
            // destroy if no area since you can't see it
            float area = Area();
            if (Mathf.Approximately(area, 0)) {
                Destroy(gameObject);
            }
        } else {
            Destroy(gameObject);
            Debug.LogError("Ground instantiation needs at least 3 vertices.");
            print("vertices:");
            for (int i = 0; i < vertices.Length; i++) {
                print(vertices[i]);
            }
        }
        
    }

    void Start() {
        
    }

    void Update() {
            //Vector3 center = new Vector3();
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = Vector3.MoveTowards(vertices[i], targets[i], Time.deltaTime * moveSpeed);
                //center += vertices[i] + transform.position;
            }
            //center /= vertices.Length;
            //for (int i = 0; i < vertices.Length; i++) {
            //    vertices[i] += transform.position - center;
            //}
            //transform.position = center;
    }

    private float Area() {
        float area = 0;
        Vector3[] tri;
        for (int i = 0; i < triangles.Length/3; i++) {
            tri = new Vector3[3];
            tri[0] = vertices[triangles[3*i]];
            tri[1] = vertices[triangles[3*i + 1]];
            tri[2] = vertices[triangles[3*i + 2]];
            area += AreaOfTriangle(tri);
        }
        return area;
    }

    private float AreaOfTriangle(Vector3[] v) {
        Vector3 u = v[1] - v[0];
        Vector3 w = v[2] - v[0];
        return 0.5f * Vector3.Cross(u, w).magnitude;
    }

    private void BuildMesh() {
        if (vertices != null && vertices.Length >= 3) {
            uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                uv[i] = new Vector2(vertices[i].x, vertices[i].z);
            }

            // create triangles
            triangles = new int[3*(vertices.Length - 2)];
            for (int i = 0; i < vertices.Length - 2; i++) {
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
    }

    public void RandomColor() {
        mat.color = new Color(Random.Range(0.0f, 1), Random.Range(0.0f, 1), Random.Range(0.0f, 1), 1);
    }
}
