using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FractalPlantComponent : MonoBehaviour {

    new public Renderer renderer;

    public int minLevels, maxLevels;
    public float maxAngle;

    public bool randomizeYRotation;

    public float relativeScale = 0.9f;
    public float minColliderScale = 0.2f;

    public Vector3 mountPoint;
    public Vector3 mount {
        get { return transform.TransformPoint(mountPoint); }
    }

    public float minTerminalDistance = 2f;

    public float splitAngle = 30f;
    public float[] childCountProbablities;

    private FractalPlantComponent[] children;

    private Vector3 goalScale;

	public void GenerateRecursive(int level, FractalPlantComponent selfPrefab, FractalPlantComponent termPrefab, ref List<Vector3> termPositions) {

        goalScale = transform.localScale;

        // rotate about upward axis
        if (randomizeYRotation) {
            transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
        }

        // rotate random angle
        if (maxAngle > 0f) {
            transform.Rotate(fromAngle(Random.Range(0f, 360f)), Random.Range(0f, maxAngle));
        }

        // recurse
        if (minLevels > 0 && maxLevels > 0) {
            int levelCount = Random.Range(minLevels, maxLevels + 1);
            if (level < levelCount) {
                InstantiateChildren(level, selfPrefab, termPrefab, ref termPositions);
            } else {
                InstantiateTerminalComponent(level, termPrefab, ref termPositions);
            }
        }

        // evaluate scale
        if (estimateScale() < minColliderScale) {
            Collider c = GetComponent<Collider>();
            if (c != null) { Destroy(c); }
        }
    }

    private void InstantiateChildren(int level, FractalPlantComponent selfPrefab, FractalPlantComponent termPrefab, ref List<Vector3> termPositions) {
        // pick children count
        float sum = 0f;
        System.Array.ForEach(childCountProbablities, e => sum += e);

        // choose children count
        float r = Random.Range(0f, sum);
        int childCount = System.Array.FindIndex(childCountProbablities, e => {
            if (e > r) {
                return true;
            } else {
                r -= e;
                return false;
            }
        });

        // ensure no failures
        if (childCount == -1) {
            childCount = childCountProbablities.Length - 1;
        }

        children = new FractalPlantComponent[childCount];

        // create children
        float curAngle = Random.Range(0f, 360f);
        for (int i = 0; i < childCount; i++) {
            FractalPlantComponent child = (FractalPlantComponent) Instantiate(selfPrefab);
            child.transform.parent = transform;
            child.transform.localPosition = mountPoint;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = new Vector3(relativeScale, relativeScale, relativeScale);

            // rotate for split
            if (childCount > 1) {
                child.transform.Rotate(fromAngle(curAngle), splitAngle);
                curAngle += 360f / childCount;
            }

            child.GenerateRecursive(level + 1, selfPrefab, termPrefab, ref termPositions);

            children[i] = child;
        }
    }

    public int Count() {
        int sum = 1;
        if (children != null) {
            for (int i = 0; i < children.Length; i++) {
                sum += children[i].Count();
            }
        }

        return sum;
    }

    public void PopulateRenderers(ref List<Renderer> list) {
        list.Add(renderer);

        if (children != null) {
            for (int i = 0; i < children.Length; i++) {
                children[i].PopulateRenderers(ref list);
            }
        }
    }

    public void LerpScaleRecursive(float t) {
        transform.localScale = Vector3.Lerp(ScalePlantBehaviour.MIN_SCALE_VECTOR, goalScale, t);

        if (children != null) {
            for (int i = 0; i < children.Length; i++) {
                children[i].LerpScaleRecursive(t);
            }
        }
    }

    private Vector3 fromAngle(float angle) {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    private void InstantiateTerminalComponent(int level, FractalPlantComponent termPrefab, ref List<Vector3> termPositions) {
        Vector3 newWorldPos = transform.TransformPoint(mountPoint);

        if (termPrefab != null &&
            validateTermPosition(newWorldPos, termPrefab.minTerminalDistance, termPositions)) {

            termPositions.Add(newWorldPos);

            FractalPlantComponent child = Instantiate(termPrefab);
            child.transform.position = newWorldPos;
            child.transform.parent = transform;
            //child.transform.localPosition = mountPoint;


            float scale = 1f / estimateScale();
            child.transform.localScale = new Vector3(scale, scale, scale);

            child.GenerateRecursive(level + 1, termPrefab, null, ref termPositions);

            children = new FractalPlantComponent[1];
            children[0] = child;
        }
    }

    private bool validateTermPosition(Vector3 worldPos, float minDist, List<Vector3> termPositions) {
        foreach (Vector3 pos in termPositions) {
            if ((pos - worldPos).magnitude < minDist) {
                Debug.Log("Too close to existing terminal node");
                return false;
            }
        }

        return true;
    }

    private float estimateScale() {
        return (transform.lossyScale.x +
            transform.lossyScale.y +
            transform.lossyScale.z) / 3f;
    }
}
