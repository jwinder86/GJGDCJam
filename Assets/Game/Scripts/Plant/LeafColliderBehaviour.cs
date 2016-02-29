using UnityEngine;
using System.Collections;

public class LeafColliderBehaviour : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
        PlayerPhysicsBehaviour phys = other.GetComponentInParent<PlayerPhysicsBehaviour>();
        if (phys != null) {
            Debug.Log("Leaves hit player!");
            phys.Owner.HandleLeavesTrigger();
        } else {
            Debug.Log("Leaves hit something?");
        }
    }
}
