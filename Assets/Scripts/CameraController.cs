using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public Transform player;
    public float distance;
    public float maxSpeed;
    public float maxRotation;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        // linearly interpolate position towards point directly above player
        Vector3 dest = new Vector3(player.position.x, distance, player.position.z);
        transform.position = Vector3.MoveTowards(transform.position, dest, Time.deltaTime * maxSpeed * (dest - transform.position).magnitude);

        // turn camera to look toward player
        Quaternion look = Quaternion.LookRotation(player.position - transform.position, player.position - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, Time.deltaTime * maxRotation * Quaternion.Angle(look, transform.rotation));
    }
}
