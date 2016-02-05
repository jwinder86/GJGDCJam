using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PlantSpawnerVisualizer : MonoBehaviour {

    // Update is called once per frame
	void Update () {
        PlantSpawner spawner = GetComponentInParent<PlantSpawner>();

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(spawner.radius * 2f, spawner.height / 2f, spawner.radius * 2f);
	}
}
