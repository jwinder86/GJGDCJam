using UnityEngine;
using System.Collections;

public class SegmentBehaviour : MonoBehaviour {

    public float distToParent = 1f;

    public float rollTime = 0.2f;

    public Transform dorsalFin;
    public Transform leftWing;
    public Transform rightWing;

    [HideInInspector]
    public SegmentBehaviour child;

    [HideInInspector]
    public BoidBehaviour owner;

    private float scaledDist;
    private Vector3 lastWorldPos;
    private float roll, rollVelocity;

	public void Initialize (Vector3 parentPosition, BoidBehaviour owner) {
        this.owner = owner;
        scaledDist = distToParent * avgScale;

        transform.position = parentPosition - (transform.forward * scaledDist);
        lastWorldPos = transform.position;

        if (child != null) {
            child.Initialize(transform.position, owner);
        }
    }

    public void RemoveWings() {
        if (leftWing != null) {
            Destroy(leftWing.gameObject);
        }

        if (rightWing != null) {
            Destroy(rightWing.gameObject);
        }
    }

    public void RemoveDorsalFin() {
        if (dorsalFin != null) {
            Destroy(dorsalFin.gameObject);
        }
    }
	
	// Update is called once per frame
	public void UpdatePosition (Vector3 parentPosition, float parentRoll) {
        Vector3 fromParent = lastWorldPos - parentPosition;
        transform.position = parentPosition + fromParent.normalized * scaledDist;
        lastWorldPos = transform.position;

        // calc roll
        roll = Mathf.SmoothDampAngle(roll, parentRoll, ref rollVelocity, rollTime);

        transform.LookAt(parentPosition);
        transform.Rotate(0f, 0f, roll, Space.Self);

        if (child != null) {
            child.UpdatePosition(transform.position, roll);
        }
	}

    private float avgScale {
        get {
            Vector3 scale = transform.lossyScale;
            return (scale.x + scale.y + scale.z) / 3f;
        }
    }
}
