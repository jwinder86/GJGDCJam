using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class CollisionEvent : UnityEvent<Collision> {}

[RequireComponent (typeof(Rigidbody))]
public class PlayerPhysicsBehaviour : MonoBehaviour {

    private PlayerModeManager owner;
    public PlayerModeManager Owner {
        get { return owner; }
    }

    public Vector3 position {
        get { return timeLerp(prevPosition, curPosition); }
        set { rigidbody.position = value; }
    }

    public Vector3 velocity {
        get { return timeLerp(prevVelocity, curVelocity); }
        set { rigidbody.velocity = value; }
    }

    public Vector3 absoluteVelocity {
        get { return rigidbody.velocity; }
    }

    public Quaternion rotation {
        get { return rigidbody.rotation; }
        set { rigidbody.MoveRotation(value); }
    }

    public BoxCollider wingCollider;
    public bool WingColliderEnabled {
        get { return wingCollider.enabled; }
        set {
            wingCollider.enabled  = value;
            float angle = value ? 90f : 0f;
            wingCollider.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }

    new private Rigidbody rigidbody;
    public float drag {
        get { return rigidbody.drag; }
        set { rigidbody.drag = value; }
    }

    private Vector3 prevPosition, prevVelocity;
    private Vector3 curPosition, curVelocity;
    private float lastUpdateTime;

    [SerializeField]
    public CollisionEvent handleCollision;

	void Awake () {
        this.rigidbody = GetComponent<Rigidbody>();
        owner = GetComponentInParent<PlayerModeManager>();
	}

    void Start() {
        // remove from player
        transform.parent = null;

        prevPosition = rigidbody.position;
        curPosition = rigidbody.position;
        prevVelocity = rigidbody.velocity;
        curPosition = rigidbody.velocity;

        lastUpdateTime = Time.fixedTime;
    }

    void FixedUpdate() {
        prevPosition = curPosition;
        prevVelocity = curVelocity;

        curPosition = rigidbody.position;
        curVelocity = rigidbody.velocity;

        lastUpdateTime = Time.fixedTime;
    }

    public void AddForce(Vector3 force, ForceMode mode) {
        rigidbody.AddForce(force, mode);
    }

    void OnCollisionEnter(Collision collision) {
        handleCollision.Invoke(collision);
    }

    private Vector3 timeLerp(Vector3 a, Vector3 b) {
        float nextUpdate = lastUpdateTime + Time.fixedDeltaTime;
        if (Time.time < lastUpdateTime) {
            Debug.LogWarning("Current time " + Time.time + " is before previous update " + lastUpdateTime);
            return a;
        } else  if (Time.time > nextUpdate) {
            //Debug.LogWarning("Current time " + Time.time + " is past expected next update " + nextUpdate);
            return b;
        } else {
            return Vector3.Lerp(a, b,
                Mathf.InverseLerp(lastUpdateTime, nextUpdate, Time.time));
        }
    }
}
