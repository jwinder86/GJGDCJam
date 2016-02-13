using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class SpeedVolumeBehaviour : MonoBehaviour {

    public float minVelocity = 10f;
    public float maxVelocity = 20f;
    public float curVelocityImpact = 0.1f;

    new private AudioSource audio;
    private float avgVelocity;
    private Vector3 prevPosition;

	// Use this for initialization
	void Awake () {
        this.audio = GetComponent<AudioSource>();
	}

    void Start() {
        avgVelocity = 0f;
        prevPosition = transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        float instVelocity = (transform.position - prevPosition).magnitude / Time.deltaTime;
        prevPosition = transform.position;

        avgVelocity = curVelocityImpact * instVelocity + (1f - curVelocityImpact) * avgVelocity;

        float vol = (avgVelocity - minVelocity) / (maxVelocity - minVelocity);
        vol = Mathf.Clamp01(vol);

        audio.volume = vol;
        audio.pitch = 1f + vol;

        //Debug.Log("Speed: " + avgVelocity + " Range: [" + minVelocity + ", " + maxVelocity + "] Volume: " + vol);
	}
}
