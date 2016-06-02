using UnityEngine;
using System.Collections;

public class IntersectionPoint {
    public Vector3 point;
    public Quadrant fault;

    public IntersectionPoint(Vector3 p, Quadrant q) {
        this.point = p;
        this.fault = q;
    }
}
