using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class PlayerModeManager : MonoBehaviour {

    public enum PlayerMode {
        Hover,
        Flight
    }

    public float transitionTime;
    public float transitionDrag;

    private PlayerMode mode;
    public PlayerMode Mode {
        get { return mode; }
    }

    //private PlayerHoverBehaviour hover;
    private PlayerMovementBehaviour hoverMove;
    private PlayerFlightBehaviour flight;
    new private Rigidbody rigidbody;
    private float defaultDrag;

    private Animator animator;

    void Awake() {
        //hover = GetComponent<PlayerHoverBehaviour>();
        hoverMove = GetComponent<PlayerMovementBehaviour>();
        flight = GetComponent<PlayerFlightBehaviour>();
        rigidbody = GetComponent<Rigidbody>();

        animator = GetComponentInChildren<Animator>();
    }

	// Use this for initialization
	void Start () {
        mode = PlayerMode.Hover;
        hoverMove.enabled = true;
        flight.enabled = false;
        animator.SetBool("Flying", false);

        defaultDrag = rigidbody.drag;
	}
	
	// Update is called once per frame
	void Update () {
        // manual toggle
	    if (Input.GetKeyDown("joystick button 1")) {
            setMode(mode == PlayerMode.Flight ? PlayerMode.Hover : PlayerMode.Flight);
        }
	}

    void OnCollisionEnter(Collision collision) {
        if (mode == PlayerMode.Flight) {
            setMode(PlayerMode.Hover);
        }
    }

    private void setMode(PlayerMode newMode) {
        if (newMode == mode) {
            return;
        } else {
            StopAllCoroutines();
            StartCoroutine(ChangeMode(newMode));
        }
    }

    private IEnumerator ChangeMode(PlayerMode newMode) {
        PlayerMode oldMode = mode;
        mode = newMode;

        hoverMove.enabled = false;
        flight.enabled = false;
        animator.SetBool("Flying", newMode == PlayerMode.Flight);

        float heading = (oldMode == PlayerMode.Flight ? flight.Heading : hoverMove.Heading);
        yield return StartCoroutine(AlignRotation(heading));

        if (newMode == PlayerMode.Flight) {
            flight.InitializeHeading(heading);
            flight.enabled = true;
        } else {
            hoverMove.InitializeAngles(heading);
            hoverMove.enabled = true;
        }

        mode = newMode;
    }

    private IEnumerator AlignRotation(float heading) {
        Quaternion start = transform.rotation;
        Quaternion goal = Quaternion.AngleAxis(heading, Vector3.up);

        for (float timer = 0f; timer < transitionTime; timer += Time.deltaTime) {
            rigidbody.drag = Mathf.Lerp(defaultDrag, transitionDrag, timer / transitionTime);
            transform.rotation = Quaternion.Slerp(start, goal, timer / transitionTime);
            yield return null;
        }

        transform.rotation = goal;

        rigidbody.drag = defaultDrag;
    }
}
