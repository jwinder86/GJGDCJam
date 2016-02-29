using UnityEngine;
using System.Collections;

public class IslandManager : MonoBehaviour {

    public float minAmount = 0.05f;
    public float maxAmount = 0.9f;
    public float fullSaturationPercent = 0.3f;
    public float fullBoidPercent = 0.9f;

    public int boidCount;
    public int boidBurstCount = 3;
    private int curBoidCount;
    //public int miniBoidCount;
    //private int curMiniBoidCount;
    public int bigBoidCount = 2;
    private bool spawnedBigBoid;

    public float saturationRadius;
    private float saturationValue;
    public float SaturationValue {
        get { return saturationValue; }
    }

    public Transform boidSpawn;
    //public Transform miniBoidSpawn;
    public BoidTargetGroup boidTargets;
    //public BoidTargetGroup miniBoidTargets;

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
        //curMiniBoidCount = 0;
        spawnedBigBoid = false;

        // generate all plants
        for (int i = 0; i < spawners.Length; i++) {
            spawners[i].GeneratePlants();
        }
        System.GC.Collect();
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

        float distToPlayer = (transform.position - playerPos).magnitude;
        float percent = UpdatePercentage((float)current / (float)total, distToPlayer);

        
	}

    private float UpdatePercentage(float percentage, float distToPlayer) {
        float adjPercent = Mathf.Clamp01(Mathf.InverseLerp(minAmount, maxAmount, percentage));

        saturationValue = Mathf.Clamp01(Mathf.InverseLerp(saturationRadius * 2f, saturationRadius, distToPlayer))* Mathf.Clamp01(Mathf.InverseLerp(0f, fullSaturationPercent, adjPercent));

        float boidPercent = Mathf.Clamp01(Mathf.InverseLerp(0f, fullBoidPercent, adjPercent));
        int goalBoids = Mathf.FloorToInt((boidPercent * boidCount) / boidBurstCount) * boidBurstCount;
        while (curBoidCount < goalBoids) {
            float angle = Random.Range(0f, 360f);
            boidManager.CreateBoid(boidSpawn.position,
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)),
                boidTargets.FirstTarget);
            curBoidCount++;

            Debug.Log("Boid Spawned!");
        }

        if (adjPercent >= 1f && !spawnedBigBoid) {
            for (int i = 0; i < bigBoidCount; i++) {
                float angle = Random.Range(0f, 360f);
                boidManager.CreateBigBoid(boidSpawn.position,
                    new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
                spawnedBigBoid = true;

                Debug.Log("Big Boid Spawned!");
            }
        }

        /*int goalMiniBoids = Mathf.FloorToInt(adjPercent * miniBoidCount);
        while (curMiniBoidCount < goalMiniBoids) {
            float angle = Random.Range(0f, 360f);
            miniBoidManager.CreateBoid(miniBoidSpawn.position,
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)),
                miniBoidTargets.FirstTarget);
            curMiniBoidCount++;
        }*/

        return adjPercent;
    }
}
