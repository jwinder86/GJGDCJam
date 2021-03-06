using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class SpeedEffectBehaviour : MonoBehaviour {

    public float minVelocity = 10f;
    public float maxVelocity = 20f;
    public float curVelocityImpact = 0.1f;

    private bool speedEffectActive;
    public bool SpeedEffectActive {
        get { return speedEffectActive; }
        set {
            speedEffectActive = value;
            if (value == false) {
                avgVelocity = 0f;
            }
        }
    }

    new private AudioSource audio;
    private float avgVelocity;
    private Vector3 prevPosition;
    private PlayerCameraBehaviour cam;

	// Use this for initialization
	void Awake () {
        this.audio = GetComponent<AudioSource>();
        cam = GetComponentInParent<PlayerCameraBehaviour>();
	}

    void Start() {
        avgVelocity = 0f;
        prevPosition = transform.position;

        speedEffectActive = true;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        float instVelocity = (transform.position - prevPosition).magnitude / Time.deltaTime;
        prevPosition = transform.position;

        avgVelocity = curVelocityImpact * instVelocity + (1f - curVelocityImpact) * avgVelocity;
        float normalizedAvgVel = Mathf.Clamp01(Mathf.InverseLerp(minVelocity, maxVelocity, avgVelocity));

        if (speedEffectActive) {
            audio.volume = normalizedAvgVel;
            audio.pitch = 1f + normalizedAvgVel;

            cam.Shake(normalizedAvgVel);
        } else {
            audio.volume = 0f;
            audio.pitch = 1f;

            avgVelocity = 0f;
        }
	}
}
