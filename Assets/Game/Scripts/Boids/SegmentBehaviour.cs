using UnityEngine;
using System.Collections;

public class SegmentBehaviour : MonoBehaviour {

    public float distToParent = 1f;

    public float rollTime = 0.2f;

    public float timeDelay = 0.2f;
    public float wingFreq = 0.3f;
    public float wingAngle = 20f;
    private float leftWingDefault;
    private float rightWingDefault;

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

        if (leftWing != null && rightWing != null) {
            leftWingDefault = leftWing.localRotation.eulerAngles.z;
            rightWingDefault = rightWing.localRotation.eulerAngles.z;
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
	public void UpdatePosition (Vector3 parentPosition, float parentRoll, float time) {
        Vector3 fromParent = lastWorldPos - parentPosition;
        transform.position = parentPosition + fromParent.normalized * scaledDist;
        lastWorldPos = transform.position;

        // calc roll
        roll = Mathf.SmoothDampAngle(roll, parentRoll, ref rollVelocity, rollTime);

        transform.LookAt(parentPosition);
        transform.Rotate(0f, 0f, roll, Space.Self);

        if (rightWing != null && leftWing != null) {
            float offset = Mathf.Sin(time * wingFreq) * wingAngle;

            leftWing.localRotation = Quaternion.Euler(0f, 0f, leftWingDefault + offset);
            rightWing.localRotation = Quaternion.Euler(0f, 0f, rightWingDefault - offset);
        }

        if (child != null) {
            child.UpdatePosition(transform.position, roll, time - timeDelay);
        }
	}

    private float avgScale {
        get {
            Vector3 scale = transform.lossyScale;
            return (scale.x + scale.y + scale.z) / 3f;
        }
    }
}
