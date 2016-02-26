using UnityEngine;
using System.Collections;

public abstract class PlantBehaviour : MonoBehaviour {

    public float spawnDist;
    public float spawnDistRand = 0.5f;
    public float spawnTime = 1f;
    public int value = 1;

    protected bool spawned;
    public bool Spawned {
        get { return spawned; }
    }
    private float sqrSpawnDist;

    // Use this for initialization
	protected void Start () {
        spawned = false;
        sqrSpawnDist = spawnDist * Random.Range(1f - spawnDistRand, 1f + spawnDistRand);
        sqrSpawnDist = sqrSpawnDist * sqrSpawnDist;

        EnableRender(false);

        // testing
        StartCoroutine(DelaySpawn());
	}

    // Update is called once per frame
	public virtual void UpdatePlayerPos (Vector3 playerPos) {
        float sqrDist = (playerPos - transform.position).sqrMagnitude;

        if (!spawned && sqrDist < sqrSpawnDist) {
            spawned = true;
            EnableRender(true);
            StartCoroutine(SpawnRoutine());
        }
	}

    private IEnumerator DelaySpawn() {
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        if (!spawned) {
            spawned = true;
            EnableRender(true);
            StartCoroutine(SpawnRoutine());
        }
    }

    public abstract void SetNormal(Vector3 normal);

    protected abstract void EnableRender(bool render);

    protected abstract IEnumerator SpawnRoutine();
}
