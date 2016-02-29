using UnityEngine;
using System.Collections;

public class BoidTargetGroup : MonoBehaviour {

    public float targetRadius;
    public float groupRadius;
    public float randomHeight = 20f;
    public int count;

    private BoidTarget firstTarget;
    public BoidTarget FirstTarget {
        get { return firstTarget; }
    }

	// Use this for initialization
	void Start () {
        firstTarget = CreateTarget(0);
        BoidTarget prevTarget = firstTarget;

        for (int i = 1; i < count; i++) {
            BoidTarget t = CreateTarget(i);
            t.nextTarget = prevTarget;
            prevTarget = t;
        }

        firstTarget.nextTarget = prevTarget;
	}

    private Vector3 calcPos(int index) {
        float d = 360f / count;
        float angle = d * index * Mathf.Deg2Rad;

        return transform.position + (new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * groupRadius);
    }

    private BoidTarget CreateTarget(int index) {
        GameObject obj = new GameObject("Target " + (index + 1));
        BoidTarget target = obj.AddComponent<BoidTarget>();
        target.transform.parent = transform;
        target.transform.position = calcPos(index) + new Vector3(0f, Random.Range(0f, randomHeight), 0f);
        target.radius = targetRadius;

        return target;
    }
}
