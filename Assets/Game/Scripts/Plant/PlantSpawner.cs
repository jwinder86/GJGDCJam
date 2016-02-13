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

    public int updateFrames = 10;
    private int curUpdateFrame = 0;

    private PlayerMovementBehaviour player;
    private List<PlantBehaviour> plants;
    private float updateRadius;

    void Awake() {
        player = FindObjectOfType<PlayerMovementBehaviour>();
    }

    // Use this for initialization
    void Start() {
        plants = new List<PlantBehaviour>();

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

        // calc update radius
        updateRadius = radius;
        foreach (PlantBehaviour prefab in prefabs) {
            float prefabRadius = radius + prefab.spawnDist * (1f + prefab.spawnDistRand);
            radius = Mathf.Max(radius, prefabRadius);
        }
    }

    void Update() {
        Vector3 playerPos = player.transform.position;
        float playerDist = (transform.position - playerPos).magnitude;

        if (playerDist < updateRadius) {
            for (int i = 0; i < plants.Count; i++) {
                plants[i].UpdatePlayerPos(playerPos);
            }
        } else {
            for (int i = curUpdateFrame; i < plants.Count; i += updateFrames) {
                plants[i].UpdatePlayerPos(playerPos);
            }
            curUpdateFrame++;
            curUpdateFrame = curUpdateFrame % updateFrames;
        }
    }

	void GeneratePlants(PlantBehaviour[] selectedPrefabs, List<Vector2> samples, Quaternion rotAdjust) {
        Debug.Log("Sample count: " + samples.Count);

        // force a single plant
        if (samples.Count == 0 && minOnePlant) {
            samples.Add(Vector2.zero);
        }

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

                    plants.Add(plant);
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
