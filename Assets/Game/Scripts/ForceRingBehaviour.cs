using UnityEngine;
using System.Collections;

public class ForceRingBehaviour : MonoBehaviour {

    public float speed = 60f;

    void OnTriggerEnter(Collider other) {
        Debug.Log("Triggered");

        PlayerModeManager player = other.GetComponentInParent<PlayerModeManager>();
        if (player != null && player.Mode == PlayerModeManager.PlayerMode.Flight) {
            Debug.Log("Moving Player!");
            player.GetComponent<Rigidbody>().velocity = transform.forward * speed;
        }
    }
}
