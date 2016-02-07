using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantSpawner : MonoBehaviour {

    public PlantBehaviour[] prefabs;
    public bool randomPrefab = false;

    public float height = 1f;
    public float radius = 1f;
    public float minDist = 0.5f;

    public float maxAngle = 15f;

    public bool visInEditor;

	// Use this for initialization
    void Start() {
        PoissonDiskSampler sampler = new PoissonDiskSampler(radius, minDist);
        List<Vector2> samples = sampler.generateSamples();

        if (randomPrefab) {
            GeneratePlants(prefabs, samples, Quaternion.identity);
        } else {
            float rot = 0f;
            foreach (PlantBehaviour prefab in prefabs) {
                Quaternion rotAdjust = Quaternion.AngleAxis(rot, Vector3.up);
                GeneratePlants(new PlantBehaviour[] { prefab }, samples, rotAdjust);

                rot += 360f / prefabs.Length;
            }
        }
    }

	void GeneratePlants(PlantBehaviour[] selectedPrefabs, List<Vector2> samples, Quaternion rotAdjust) {
        Debug.Log("Sample count: " + samples.Count);

        for (int i = 0; i < samples.Count; i++) {
            Vector2 s = samples[i];
            Vector3 pos = rotAdjust * new Vector3(s.x, height / 2f, s.y);
            Vector3 worldPos = transform.TransformPoint(pos);

            RaycastHit hit;
            if (Physics.Raycast(worldPos, -transform.up, out hit, height)) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && 
                    Vector3.Angle(transform.up, hit.normal) <= maxAngle) {
                    PlantBehaviour plant = (PlantBehaviour)Instantiate(ChoosePrefab(selectedPrefabs), worldPos + hit.distance * -transform.up, Quaternion.identity);
                    plant.transform.parent = transform;
                    plant.SetNormal(hit.normal);
                }
            }
        }

        // remove visualizer
        PlantSpawnerVisualizer vis = GetComponentInChildren<PlantSpawnerVisualizer>();
        if (vis) {
            vis.gameObject.SetActive(false);
        }
        
	}

    PlantBehaviour ChoosePrefab(PlantBehaviour[] prefabs) {
        return prefabs[Random.Range(0, prefabs.Length)];
    }
}
