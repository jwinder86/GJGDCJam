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
        get { return timeLerp(prevRotation, curRotation); }
        set { rigidbody.rotation = value; }
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
    private Quaternion prevRotation, curRotation;
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
        prevRotation = rigidbody.rotation;
        curRotation = rigidbody.rotation;

        lastUpdateTime = Time.fixedTime;
    }

    void FixedUpdate() {
        prevPosition = curPosition;
        prevVelocity = curVelocity;
        prevRotation = curRotation;

        curPosition = rigidbody.position;
        curVelocity = rigidbody.velocity;
        curRotation = rigidbody.rotation;

        lastUpdateTime = Time.fixedTime;
    }

    public void Teleport(Vector3 position, Quaternion rotation) {
        rigidbody.position = position;
        curPosition = position;
        prevPosition = position;

        rigidbody.rotation = rotation;

        rigidbody.velocity = Vector3.zero;
        curVelocity = Vector3.zero;
        prevPosition = Vector3.zero;
    }

    public void AddForce(Vector3 force, ForceMode mode) {
        rigidbody.AddForce(force, mode);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode) {
        rigidbody.AddForceAtPosition(force, position, mode);
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

    private Quaternion timeLerp(Quaternion a, Quaternion b) {
        float nextUpdate = lastUpdateTime + Time.fixedDeltaTime;
        if (Time.time < lastUpdateTime) {
            Debug.LogWarning("Current time " + Time.time + " is before previous update " + lastUpdateTime);
            return a;
        } else if (Time.time > nextUpdate) {
            //Debug.LogWarning("Current time " + Time.time + " is past expected next update " + nextUpdate);
            return b;
        } else {
            return Quaternion.Slerp(a, b,
                Mathf.InverseLerp(lastUpdateTime, nextUpdate, Time.time));
        }
    }

    public void ConstrainRotation(bool constrain) {
        if (constrain) {
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.angularVelocity = Vector3.zero;
        } else {
            rigidbody.constraints = RigidbodyConstraints.None;
            rigidbody.angularVelocity = Quaternion.Lerp(Random.rotation, Quaternion.identity, 0.1f).eulerAngles;
        }
    }
}
