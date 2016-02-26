using UnityEngine;
using System.Collections;

[RequireComponent (typeof(AudioSource))]
public class ForceRingBehaviour : MonoBehaviour {

    public float speed = 60f;

    new private AudioSource audio;

    void Awake() {
        audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other) {
        PlayerPhysicsBehaviour player = other.GetComponentInParent<PlayerPhysicsBehaviour>();
        if (player != null && player.Owner.Mode == PlayerModeManager.PlayerMode.Flight) {
            audio.Play();

            Vector3 velocity = player.velocity;
            float dot = Vector3.Dot(velocity, transform.forward);

            if (dot > 0f) {
                player.velocity = transform.forward * speed;
            } else {
                player.velocity = transform.forward * -speed;
            }
        }
    }
}
