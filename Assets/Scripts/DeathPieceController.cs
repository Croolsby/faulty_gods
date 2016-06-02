using UnityEngine;
using System.Collections;

public class DeathPieceController : MonoBehaviour {
    public float lifeTime;

    private float startTime;
    private Material mat;

    // used for procedurally generated mesh
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

    /*
     * Death pieces are always created from the EnemyController
     * which supplies the pieces with vertex data immediately after instantiation.
     */
    void Start() {
        startTime = Time.time;
        mat = GetComponent<Renderer>().material;

        if (vertices.Length >= 3) {
            //print("death piece vertices: " + vertices.Length);
            // TODO uv for shapes with radius larger than 1;
            uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) {
                uv[i] = new Vector2(vertices[i].x * 0.5f, vertices[i].z);
            }

            // create triangles
            triangles = new int[3*(vertices.Length - 2)];
            for (int i = 0; i < vertices.Length - 2; i++) {
                triangles[3*i] = 0;
                triangles[3*i + 1] = i + 1;
                triangles[3*i + 2] = i + 2;
            }

            // create back faces
            int[] backTriangles = new int[triangles.Length];
            for (int i = 0; i < vertices.Length - 2; i++) {
                backTriangles[3*i] = 0;
                backTriangles[3*i + 1] = i + 2;
                backTriangles[3*i + 2] = i + 1;
            }

            // put front faces and back faces together
            Vector3[] newVertices = new Vector3[2 * vertices.Length];
            Vector2[] newUV = new Vector2[2 * uv.Length];
            int[] newTriangles = new int[2 * triangles.Length];
            for (int i = 0; i < vertices.Length; i++) {
                newVertices[i] = vertices[i];
                newUV[i] = uv[i];
            }
            for (int i = 0; i < triangles.Length; i++) {
                newTriangles[i] = triangles[i];
            }
            for (int i = 0; i < vertices.Length; i++) {
                newVertices[i + vertices.Length] = vertices[i];
                newUV[i + vertices.Length] = new Vector3(uv[i].x + 0.5f, uv[i].y);
            }
            for (int i = 0; i < triangles.Length; i++) {
                newTriangles[i + triangles.Length] = backTriangles[i] + vertices.Length;
            }

            // add vertex data to mesh
            Mesh mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;
            mesh.RecalculateNormals();

            GetComponent<MeshCollider>().sharedMesh = mesh;
        } else {
            Debug.LogError("Death piece needs at least 3 vertices.");
        }
    }

    void Update() {
        float elaspedTime = Time.time - startTime;
        if (elaspedTime > lifeTime) {
            Destroy(gameObject);
        } else {
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1 - elaspedTime/lifeTime);
        }
    }
}
