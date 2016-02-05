using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerHoverBehaviour : MonoBehaviour {

    public float hoverHeight;
    public float antigravHeight;
    public float hoverForce;

    public float yVelDampen = 0.1f;

    public float sphereCastRadius = 1f;

    private int layerMask;
    private float antiGravForce;
    new private Rigidbody rigidbody;

	void Awake () {
        layerMask = ~(1 << LayerMask.NameToLayer("Player"));
        antiGravForce = -Physics.gravity.y;
        rigidbody = GetComponent<Rigidbody>();
    }
	
	void FixedUpdate () {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphereCastRadius, Vector3.down, out hit, antigravHeight, layerMask)) {
            float force;
            float dampenMult;
            if (hit.distance > hoverHeight) {
                force = Mathf.Lerp(antiGravForce, 0f, (hit.distance - hoverHeight) / (antigravHeight - hoverHeight));
                dampenMult = Mathf.Lerp(1f, 0f, (hit.distance - hoverHeight) / (antigravHeight - hoverHeight));
            } else {
                force = Mathf.Lerp(hoverForce, 0f, Mathf.Pow(hit.distance / hoverHeight, 2)) + antiGravForce;
                dampenMult = 1f;
            }

            // dampen y velocity
            float yVel = rigidbody.velocity.y;
            float yDamp = yVel * yVel * yVelDampen * Mathf.Sign(yVel) * dampenMult;

            rigidbody.AddForce(new Vector3(0f, force - yDamp, 0f), ForceMode.Acceleration);
            //Debug.Log("Dist: " + hit.distance + ", Accel applied: " + (force - yDamp) + ", Y speed " + rigidbody.velocity.y);
        } else {
            //Debug.Log("No force applied");
        }
	}
}
