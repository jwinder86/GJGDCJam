using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerCameraBehaviour : MonoBehaviour {

    public float vertAngle = 30f;
    public float cameraDistance = 10f;
    public float minDist = 5f;
    public float maxDist = 15f;
    
    new private Transform camera;
    new private Rigidbody rigidbody;

    private Vector3 camVelocity;
    
	void Awake () {
        camera = Camera.main.transform;
        rigidbody = GetComponent<Rigidbody>();
	}

    void Start() {
        camVelocity = Vector3.zero;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        Vector3 toPlayer = camera.position - transform.position;
        toPlayer.y = 0f;
        float len = toPlayer.magnitude;

        toPlayer.y = Mathf.Sin(vertAngle * Mathf.Deg2Rad) * len;

        toPlayer = toPlayer.normalized * cameraDistance;

        Vector3 newCamPosition = Vector3.SmoothDamp(camera.position, transform.position + toPlayer, ref camVelocity, 1f);

        // clamp distance
        Vector3 newToPlayer = newCamPosition - transform.position;
        if (newToPlayer.magnitude > maxDist) {
            newToPlayer = newToPlayer.normalized * maxDist;
            newCamPosition = transform.position + newToPlayer;
        } else if (newToPlayer.magnitude < minDist) {
            newToPlayer = newToPlayer.normalized * minDist;
            newCamPosition = transform.position + newToPlayer;
        }

        camera.position = newCamPosition;

        camera.LookAt(transform.position);
    }
}
