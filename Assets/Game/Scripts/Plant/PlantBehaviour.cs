using UnityEngine;
using System.Collections;

public abstract class PlantBehaviour : MonoBehaviour {

    public float spawnDist;
    public float spawnDistRand = 0.5f;
    public float spawnTime = 1f;

    private bool spawned;
    private float sqrSpawnDist;
    private PlayerMovementBehaviour player;

    protected void Awake() {
        player = FindObjectOfType<PlayerMovementBehaviour>();
    }

	// Use this for initialization
	protected void Start () {
        spawned = false;
        sqrSpawnDist = spawnDist * Random.Range(1f - spawnDistRand, 1f + spawnDistRand);
        sqrSpawnDist = sqrSpawnDist * sqrSpawnDist;
	}

    // Update is called once per frame
	void Update () {
        if (!spawned && (player.transform.position - transform.position).sqrMagnitude < sqrSpawnDist) {
            spawned = true;
            StartCoroutine(SpawnRoutine());
        }
	}

    public abstract void SetNormal(Vector3 normal);

    protected abstract IEnumerator SpawnRoutine();
}
