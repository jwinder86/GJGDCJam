using UnityEngine;
using System.Collections;

public class RuntimeRandomRotation : MonoBehaviour {

	void Start () {
        transform.rotation = Random.rotationUniform;
	}
}
