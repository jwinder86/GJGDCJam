using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider))]
public class BoidObstacle : MonoBehaviour {

    public float radius = 10f;
    public bool visualize = true;

    void Start() {
        BoidObstacleVisualizer vis = GetComponentInChildren<BoidObstacleVisualizer>();
        if (vis != null) {
            vis.gameObject.SetActive(false);
        }

        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null) {
            collider.radius = this.radius;
        } else {
            Debug.LogError("Obstacle missing collider" + this);
        }
    }
}
