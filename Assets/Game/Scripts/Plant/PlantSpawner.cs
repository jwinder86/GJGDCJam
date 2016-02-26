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

    public bool minOnePlant = false;

    public bool visInEditor;
    public bool extraLogging = false;

    //public int updateFrames = 10;
    //private int curUpdateFrame = 0;
    private int totalValue;
    public int TotalValue {
        get { return totalValue; }
    }

    private int currentValue;
    public int CurrentValue {
        get { return currentValue; }
    }

    private List<PlantBehaviour> plants;
    private float updateRadius;
    private int layerMask;

    void Awake() {
        layerMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("PlantObstacle"));

        totalValue = -1;
        currentValue = -1;
    }

    // Use this for initialization
    void Start() {
        // remove visualizer
        PlantSpawnerVisualizer vis = GetComponentInChildren<PlantSpawnerVisualizer>();
        if (vis) {
            vis.gameObject.SetActive(false);
        }

        // calc update radius
        updateRadius = radius;
        foreach (PlantBehaviour prefab in prefabs) {
            float prefabRadius = radius + prefab.spawnDist * (1f + prefab.spawnDistRand);
            updateRadius = Mathf.Max(updateRadius, prefabRadius);
        }
    }

    public void GeneratePlants() {
        plants = new List<PlantBehaviour>();

        if (randomPrefab) {
            GeneratePlants(prefabs);
        } else {
            foreach (PlantBehaviour prefab in prefabs) {
                GeneratePlants(new PlantBehaviour[] { prefab });
            }
        }

        // calc total value
        totalValue = 0;
        currentValue = 0;
        foreach (PlantBehaviour plant in plants) {
            totalValue += plant.value;
        }
    }

    public void UpdatePlants(Vector3 playerPos) {
        float playerDist = (transform.position - playerPos).magnitude;
        
        if (playerDist < updateRadius) {
            currentValue = 0;
            for (int i = 0; i < plants.Count; i++) {
                if (!plants[i].Spawned) {
                    plants[i].UpdatePlayerPos(playerPos);
                } else {
                    currentValue += plants[i].value;
                }
            }
        }
    }

	private void GeneratePlants(PlantBehaviour[] selectedPrefabs) {
        PoissonDiskSampler sampler = new PoissonDiskSampler(radius, minDist);
        List<Vector2> samples = sampler.generateSamples(delegate(Vector2 s) { return PlacePlant(s, selectedPrefabs); });

        if (extraLogging) { Debug.Log("Sample count: " + samples.Count); }

        // force a single plant
        if (samples.Count == 0 && minOnePlant) {
            PlacePlant(Vector2.zero, selectedPrefabs);
            samples.Add(Vector2.zero);
        }
	}

    private bool PlacePlant(Vector2 sample, PlantBehaviour[] selectedPrefabs) {
        Vector3 pos = new Vector3(sample.x, height / 2f, sample.y);
        Vector3 worldPos = transform.TransformPoint(pos);

        RaycastHit hit;
        if (Physics.Raycast(worldPos, -transform.up, out hit, height, layerMask)) {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") &&
                Vector3.Angle(transform.up, hit.normal) <= maxAngle) {
                PlantBehaviour plant = (PlantBehaviour)Instantiate(ChoosePrefab(selectedPrefabs), worldPos + hit.distance * -transform.up, Quaternion.identity);
                plant.transform.parent = transform;
                plant.SetNormal(hit.normal);

                plants.Add(plant);
                return true;
            }

            if (extraLogging) { Debug.Log("Raycast hit non-ground at " + worldPos); }
        } else if (extraLogging) { Debug.Log("Raycast missed at" + worldPos); }

        return false;
    }

    PlantBehaviour ChoosePrefab(PlantBehaviour[] prefabs) {
        return prefabs[Random.Range(0, prefabs.Length)];
    }
}
