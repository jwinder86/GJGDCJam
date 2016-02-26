using UnityEngine;
using System.Collections;

public class IslandManager : MonoBehaviour {

    public float minAmount = 0.05f;
    public float maxAmount = 0.85f;

    public int boidCount;
    private int curBoidCount;
    public int miniBoidCount;
    private int curMiniBoidCount;

    public Transform boidSpawn;
    public Transform miniBoidSpawn;
    public BoidTargetGroup boidTargets;
    public BoidTargetGroup miniBoidTargets;

    private PlantSpawner[] spawners;
    private BoidManager boidManager;
    private MiniBoidManager miniBoidManager;
    private PlayerPhysicsBehaviour player;

    void Awake() {
        spawners = GetComponentsInChildren<PlantSpawner>();
        boidManager = FindObjectOfType<BoidManager>();
        miniBoidManager = FindObjectOfType<MiniBoidManager>();
        player = FindObjectOfType<PlayerPhysicsBehaviour>();
    }

    void Start() {
        curBoidCount = 0;
        curMiniBoidCount = 0;

        // generate all plants
        for (int i = 0; i < spawners.Length; i++) {
            spawners[i].GeneratePlants();
            System.GC.Collect();
        }
    }

	// Update is called once per frame
	void Update () {
        Vector3 playerPos = player.transform.position;
        int total = 0;
        int current = 0;

        for (int i = 0; i < spawners.Length; i++) {
            spawners[i].UpdatePlants(playerPos);

            total += spawners[i].TotalValue;
            current += spawners[i].CurrentValue;
        }

        UpdatePercentage((float)current / (float)total);
	}

    private void UpdatePercentage(float percentage) {
        float adjPercent = Mathf.Clamp01(Mathf.InverseLerp(minAmount, maxAmount, percentage));

        int goalBoids = Mathf.FloorToInt(adjPercent * boidCount);
        while (curBoidCount < goalBoids) {
            float angle = Random.Range(0f, 360f);
            boidManager.CreateBoid(boidSpawn.position,
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)),
                boidTargets.FirstTarget);
            curBoidCount++;

            Debug.Log("Boid Spawned!");
        }

        /*int goalMiniBoids = Mathf.FloorToInt(adjPercent * miniBoidCount);
        while (curMiniBoidCount < goalMiniBoids) {
            float angle = Random.Range(0f, 360f);
            miniBoidManager.CreateBoid(miniBoidSpawn.position,
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)),
                miniBoidTargets.FirstTarget);
            curMiniBoidCount++;
        }*/
    }
}
