using UnityEngine;
using UnityEngine.Events;

public class BoidTarget : MonoBehaviour {
    public float radius;
    public BoidTarget nextTarget;

    public bool InRange(Vector3 position) {
        return (position - transform.position).sqrMagnitude <= radius * radius;
    }
}
