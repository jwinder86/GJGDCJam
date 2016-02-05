using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantSpawner : MonoBehaviour {

    public PlantBehaviour prefab;

    public float height = 1f;
    public float radius = 1f;
    public float minDist = 0.5f;

	// Use this for initialization
	void Start () {
        PoissonDiskSampler sampler = new PoissonDiskSampler(radius, minDist);
        List<Vector2> samples = sampler.generateSamples();

        for (int i = 0; i < samples.Count; i++) {
            Vector2 s = samples[i];
            Vector3 pos = new Vector3(s.x + transform.position.x, transform.position.y + height / 2f, s.y + transform.position.z);

            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, height)) {
                PlantBehaviour plant = (PlantBehaviour) Instantiate(prefab, pos + hit.distance * Vector3.down, Quaternion.identity);
                plant.SetNormal(hit.normal);
            }
            
        }

        // remove visualizer
        PlantSpawnerVisualizer vis = GetComponentInChildren<PlantSpawnerVisualizer>();
        if (vis) {
            vis.gameObject.SetActive(false);
        }
        
	}
}
