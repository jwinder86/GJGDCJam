using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralVinePlant : PlantBehaviour {

    private const float LENGTH_EPSILON = 0.001f;
    private const int SAMPLE_ITERATIONS = 15;

    public ProceduralVineSegment segmentPrefab;

    public float randomAngle;
    public float maxLength;
    public int attempts = 30;

    public float minHang = 2f;
    public float maxHang = 3f;
    private Vector3 hang;

    public float centerLeavesProb = 0.9f;
    public float endLeavesProb = 0.3f;

    public AnimationCurve yCurve;
    public AnimationCurve xzCurve;

    private int layerMask;

    private Vector3 endpoint;
    private Vector3 goalScale;
    private bool endpointPreSelect = false;

    private LODGroup lod;
    private Renderer[] renderers;

    void Awake() {
        layerMask = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("PlantObstacle"));
        lod = GetComponent<LODGroup>();
    }

    new void Start() {
        base.Start();

        GenerateVine();
        renderers = GetComponentsInChildren<Renderer>();
        GenerateLODs(renderers);

        goalScale = transform.localScale;
        transform.localScale = ScalePlantBehaviour.MIN_SCALE_VECTOR;
        EnableRender(false);
    }

    public void SetEndpoint(Vector3 endpoint) {
        this.endpoint = endpoint;
        endpointPreSelect = true;
    }

    public override void SetNormal(Vector3 normal) {
        transform.rotation *= Quaternion.FromToRotation(Vector3.up, normal);
    }

    protected override void EnableRender(bool render) {
        if (renderers != null) {
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].enabled = render;
            }
        }
    }

    protected override IEnumerator SpawnRoutine() {
        if (lod != null) {
            lod.ForceLOD(0);
        }

        soundManager.PlaySound(SoundType.Creak, transform);

        transform.localScale = ScalePlantBehaviour.MIN_SCALE_VECTOR;

        for (float timer = 0f; timer < spawnTime; timer += Time.deltaTime) {
            Vector3 scale = new Vector3(
                Mathf.Lerp(ScalePlantBehaviour.MIN_SCALE_VECTOR.x, goalScale.x, xzCurve.Evaluate(timer / spawnTime)),
                Mathf.Lerp(ScalePlantBehaviour.MIN_SCALE_VECTOR.y, goalScale.y, yCurve.Evaluate(timer / spawnTime)),
                Mathf.Lerp(ScalePlantBehaviour.MIN_SCALE_VECTOR.z, goalScale.z, xzCurve.Evaluate(timer / spawnTime)));
            transform.localScale = scale;

            yield return null;
        }

        transform.localScale = goalScale;
        if (lod != null) {
            lod.ForceLOD(-1);
        }
    }

    private void GenerateVine() {
        if (endpointPreSelect) {
            GenerateVineSegments();
        } else {
            bool found = false;
            for (int i = 0; i < attempts && !found; i++) {
                found = RandomEndpoint(out endpoint);
            }

            if (found) {
                GenerateVineSegments();
            } else {
                Fail();
            }
        }
    }

    private void GenerateVineSegments() {
        hang = new Vector3(0f, Random.Range(minHang, maxHang) * -2f, 0f);

        float lastPos = 0.5f;
        while (lastPos < 1f) {
            float nextPos = findSample(lastPos, segmentPrefab.length, true);

            ProceduralVineSegment segment = Instantiate(segmentPrefab);
            segment.transform.parent = transform;
            segment.transform.position = sample(lastPos);
            segment.transform.LookAt(sample(nextPos));

            float segProb = Mathf.Lerp(centerLeavesProb, endLeavesProb,
                Mathf.InverseLerp(0.5f, 1f, lastPos));
            if (Random.value > segProb) {
                segment.RemoveLeaves();
            }

            lastPos = nextPos;
        }

        lastPos = 0.5f;
        while (lastPos > 0f) {
            float nextPos = findSample(lastPos, segmentPrefab.length, false);

            ProceduralVineSegment segment = Instantiate(segmentPrefab);
            segment.transform.parent = transform;
            segment.transform.position = sample(lastPos);
            segment.transform.LookAt(sample(nextPos));

            float segProb = Mathf.Lerp(centerLeavesProb, endLeavesProb,
                Mathf.InverseLerp(0.5f, 0f, lastPos));
            if (Random.value > segProb) {
                segment.RemoveLeaves();
            }

            lastPos = nextPos;
        }
    }

    private bool RandomEndpoint(out Vector3 endpoint) {
        Vector3 dir = randomDirection(transform.up);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, maxLength, layerMask)) {
            if (Vector3.Angle(hit.normal, -dir) < randomAngle) {
                endpoint = hit.point;
                return true;
            }
        }

        endpoint = Vector3.zero;
        return false;
    }

    private Vector3 randomDirection(Vector3 direction) {
        Quaternion randRot = Quaternion.Slerp(Quaternion.identity, Random.rotation, randomAngle / 360f);
        return randRot * direction;
    }

    // make plant useless
    private void Fail() {
        value = 0;
        spawnDist = 0f;
        spawnDistRand = 0f;
        base.Start();
        gameObject.SetActive(false);
    }

    private float findSample(float start, float distance, bool forwards) {
        float adjustmentScale = 0.1f;
        Vector3 startPosition = sample(start);

        float curSample = start;

        for (int i = 0; i < SAMPLE_ITERATIONS; i++) {
            float curDist = (sample(curSample) - startPosition).magnitude;
            while (curDist < distance) {
                curSample += forwards ? adjustmentScale : -adjustmentScale;
                curDist = (sample(curSample) - startPosition).magnitude;

                if (Mathf.Abs(curDist - distance) <= LENGTH_EPSILON) {
                    return curSample;
                }
            }

            //Debug.Log("Sample point " + curSample + " bypassed goal distance by " + Mathf.Abs(curDist - distance) + ", more than epsilon. Reducing scale to " + (adjustmentScale / 2f) + " and retrying from " + (curSample - (forwards ? adjustmentScale : -adjustmentScale)));
            curSample -= forwards ? adjustmentScale : -adjustmentScale;
            adjustmentScale /= 2f;
        }

        Debug.LogError("Did not reach within epsilon of goal after " + SAMPLE_ITERATIONS + " tries, returning best estimate: " + curSample);
        return curSample;
    }

    private Vector3 sample(float t) {
        Vector3 a = Vector3.LerpUnclamped(transform.position, endpoint + hang, t);
        Vector3 b = Vector3.LerpUnclamped(transform.position + hang, endpoint, t);
        return Vector3.LerpUnclamped(a, b, t);
    }

    private void GenerateLODs(Renderer[] renderers) {
        LOD[] existingLODs = lod.GetLODs();

        if (existingLODs.Length != 2) {
            Debug.LogError("Unexpected LOD count: " + existingLODs.Length + ", " + name);
        }

        List<Renderer> lod0s = new List<Renderer>();
        List<Renderer> lod1s = new List<Renderer>();

        for (int i = 0; i < renderers.Length; i++) {
            if (renderers[i].name.Equals("LOD0")) {
                lod0s.Add(renderers[i]);
            } else if (renderers[i].name.Equals("LOD1")) {
                lod1s.Add(renderers[i]);
            } else {
                Debug.LogError("Unexpected LOD name: " + renderers[i].name);
            }
        }

        LOD lod0 = new LOD(existingLODs[0].screenRelativeTransitionHeight, lod0s.ToArray());
        lod0.fadeTransitionWidth = existingLODs[0].fadeTransitionWidth;

        LOD lod1 = new LOD(existingLODs[1].screenRelativeTransitionHeight, lod1s.ToArray());
        lod1.fadeTransitionWidth = existingLODs[1].fadeTransitionWidth;

        lod.SetLODs(new LOD[] { lod0, lod1 });
        lod.RecalculateBounds();
    }
}
