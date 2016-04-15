using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlantSpawner : MonoBehaviour, PlantManager {

    public PlantSpawner[] avoidSamplesFrom;

    public PlantBehaviour[] prefabs;
    public bool randomPrefab = false;

    public float height = 1f;
    public float radius = 1f;
    public float minDist = 0.5f;

    public float maxAngle = 15f;

    public bool minOnePlant = false;

    public bool visInEditor;
    public bool extraLogging = false;

    private SoundManager soundManager;

    private bool generationStarted;
    private bool generationCompleted;
    private List<Vector3> worldSpaceSamples;
    public List<Vector3> Samples {
        get {
            if (!generationCompleted) {
                GeneratePlants();
            }

            return worldSpaceSamples;
        }
    }

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

    public void CleanUp() {
        worldSpaceSamples = null;
    }

    public void GeneratePlants() {
        //Debug.Log("Starting generation: " + name);

        if (generationStarted) {
            if (!generationCompleted) {
                Debug.LogError("GeneratePlants called after already started but not yet completed: " + name);
            }

            return;
        }
        generationStarted = true;

        List<Vector3> prereqSamples = new List<Vector3>();
        if (avoidSamplesFrom != null) {
            //Debug.Log("Requesting samples from prereqs: " + name);
            for (int i = 0; i < avoidSamplesFrom.Length; i++) {
                prereqSamples.AddRange(avoidSamplesFrom[i].Samples);
            }
        }
        List<Vector2> localSamples = ConvertToLocal(prereqSamples);
        //Debug.Log("Received and converted " + localSamples.Count + " samples: " + name);

        worldSpaceSamples = new List<Vector3>();
        plants = new List<PlantBehaviour>();

        if (randomPrefab) {
            GeneratePlants(prefabs, localSamples);
        } else {
            foreach (PlantBehaviour prefab in prefabs) {
                GeneratePlants(new PlantBehaviour[] { prefab }, localSamples);
            }
        }

        // calc total value
        totalValue = 0;
        currentValue = 0;
        foreach (PlantBehaviour plant in plants) {
            if (plant != null) {
                totalValue += plant.value;
            }
        }

        generationCompleted = true;
    }

    public void UpdatePlants(Vector3 playerPos) {
        float playerDist = (transform.position - playerPos).magnitude;
        
        if (playerDist < updateRadius) {
            currentValue = 0;
            for (int i = 0; i < plants.Count; i++) {
                if (plants[i] != null) {
                    if (!plants[i].Spawned) {
                        plants[i].UpdatePlayerPos(playerPos);
                    } else {
                        currentValue += plants[i].value;
                    }
                }
            }
        }
    }

	private void GeneratePlants(PlantBehaviour[] selectedPrefabs, List<Vector2> externalSamples) {
        PoissonDiskSampler sampler = new PoissonDiskSampler(radius, minDist);
        int sampleCount = sampler.generateSamples(delegate(Vector2 s) { return PlacePlant(s, selectedPrefabs); }, externalSamples);

        if (extraLogging) { Debug.Log("Sample count: " + sampleCount); }

        // force a single plant
        if (sampleCount == 0) {
            if (minOnePlant) {
                PlacePlant(Vector2.zero, selectedPrefabs);
            } else {
                Debug.LogError("Failed to create plants: " + name + " in " + transform.parent.name);
            }
        }
	}

    private bool PlacePlant(Vector2 sample, PlantBehaviour[] selectedPrefabs) {
        // get sound manager
        if (soundManager == null) {
            soundManager = FindObjectOfType<SoundManager>();
        }

        Vector3 pos = new Vector3(sample.x, height / 2f, sample.y);
        Vector3 worldPos = transform.TransformPoint(pos);

        RaycastHit hit;
        if (Physics.Raycast(worldPos, -transform.up, out hit, height, layerMask)) {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") &&
                Vector3.Angle(transform.up, hit.normal) <= maxAngle) {
                Vector3 position = worldPos + hit.distance * -transform.up;

                worldSpaceSamples.Add(position);

                PlantBehaviour plant = (PlantBehaviour)Instantiate(ChoosePrefab(selectedPrefabs), position, Quaternion.identity);
                plant.transform.parent = transform;
                plant.SetNormal(hit.normal);
                plant.SoundManager = soundManager;

                plants.Add(plant);
                return true;
            }

            if (extraLogging) { Debug.Log("Raycast hit non-ground at " + worldPos); }
        } else if (extraLogging) { Debug.Log("Raycast missed at" + worldPos); }

        return false;
    }

    private PlantBehaviour ChoosePrefab(PlantBehaviour[] prefabs) {
        return prefabs[Random.Range(0, prefabs.Length)];
    }

    private List<Vector2> ConvertToLocal(List<Vector3> prereqSamples) {
        List<Vector2> samples = new List<Vector2>(prereqSamples.Count);

        foreach (Vector3 worldPos in prereqSamples) {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            samples.Add(new Vector2(localPos.x, localPos.z));
        }

        return samples;
    }
}
