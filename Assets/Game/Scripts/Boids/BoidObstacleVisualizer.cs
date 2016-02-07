using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BoidObstacleVisualizer : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        BoidObstacle obstacle = GetComponentInParent<BoidObstacle>();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(obstacle.radius * 2f, obstacle.radius * 2f, obstacle.radius * 2f);

        GetComponent<Renderer>().enabled = obstacle.visualize;
    }
}
