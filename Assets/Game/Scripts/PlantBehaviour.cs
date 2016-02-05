using UnityEngine;
using System.Collections;

public class PlantBehaviour : MonoBehaviour {

    private const float MIN_SCALE = 0.01f;
    private readonly Vector3 MIN_SCALE_VECTOR = new Vector3(MIN_SCALE, MIN_SCALE, MIN_SCALE);

    public float spawnDist;
    public float spawnDistRand = 0.5f;
    public float spawnTime = 1f;

    public bool randomizeYRotation;

    public float yScaleRandomization = 0f;

    new public Renderer renderer;

    private bool spawned;
    private PlayerMovementBehaviour player;

    private Vector3 goalScale;

    void Awake() {
        player = FindObjectOfType<PlayerMovementBehaviour>();
    }

	// Use this for initialization
	void Start () {
        renderer.enabled = false;
        spawned = false;

        spawnDist *= Random.Range(1f - spawnDistRand, 1f + spawnDistRand);

        if (randomizeYRotation) {
            transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
        }

        goalScale = transform.localScale;
        goalScale.y += Random.Range(-yScaleRandomization, yScaleRandomization);
	}

    public void SetNormal(Vector3 normal) {
        transform.rotation *= Quaternion.FromToRotation(Vector3.up, normal);
    }
	
	// Update is called once per frame
	void Update () {
        if (!spawned && (player.transform.position - transform.position).magnitude < spawnDist) {
            spawned = true;
            StartCoroutine(SpawnRoutine());
        }
	}

    private IEnumerator SpawnRoutine() {
        renderer.enabled = true;
        transform.localScale = MIN_SCALE_VECTOR;

        for (float timer = 0f; timer < spawnTime; timer += Time.deltaTime) {
            Vector3 scale = Vector3.Lerp(MIN_SCALE_VECTOR, goalScale, timer / spawnTime);
            transform.localScale = scale;

            yield return null;
        }

        transform.localScale = goalScale;

        this.enabled = false;
    }
}
