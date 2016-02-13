using UnityEngine;
using System.Collections;

public class ForceRingBehaviour : MonoBehaviour {

    public float speed = 60f;

    void OnTriggerEnter(Collider other) {
        Debug.Log("Triggered");

        PlayerPhysicsBehaviour player = other.GetComponentInParent<PlayerPhysicsBehaviour>();
        if (player != null && player.Owner.Mode == PlayerModeManager.PlayerMode.Flight) {
            Debug.Log("Moving Player!");
            player.velocity = transform.forward * speed;
        }
    }
}
