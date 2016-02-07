using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoissonDiskSampler {

    private float radius;
    private float minDist;
    private int k;
	
    public PoissonDiskSampler(float radius, float minDist, int k = 30) {
        this.radius = radius;
        this.minDist = minDist;
        this.k = k;
    }

    public List<Vector2> generateSamples() {
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();

        active.Add(RandomVector());

        while (active.Count > 0) {
            // select random index and move to end
            int index = Random.Range(0, active.Count);
            swap(ref active, index, active.Count - 1);
            Vector2 selected = active[active.Count - 1];
            bool found = false;

            for (int i = 0; i < k; i++) {
                Vector2 newSample = selected + RandomVector();

                if (validateBounds(newSample) && validateDistSamples(ref samples, newSample)) {
                    samples.Add(newSample);
                    active.Add(newSample);
                    found = true;
                }

            }

            if (!found) {
                active.RemoveAt(active.Count - 1);
            }
        }

        return samples;
    }

    private bool validateBounds(Vector2 newSample) {
        return (newSample.sqrMagnitude < radius * radius);
    }

    private bool validateDistSamples(ref List<Vector2> samples, Vector2 newSample) {
        float sqrDist = minDist * minDist;
        for (int i = 0; i < samples.Count; i++) {
            if ((samples[i] - newSample).sqrMagnitude < sqrDist) {
                return false;
            }
        }

        return true;
    }

    private Vector2 RandomVector() {
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float radius = Random.Range(minDist, 2f * minDist);
        Vector2 point = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));

        return point;
    }

    private void swap(ref List<Vector2> list, int i, int j) {
        Vector2 temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    /*private class SpacialLookup {
        private float cellSize;
        private bool[,] data;
        private int halfDim;

        public SpacialLookup(float radius, float minDist) {
            cellSize = minDist / Mathf.Sqrt(2f);

            halfDim = Mathf.CeilToInt(radius / cellSize);
            int dim = halfDim * 2;

            data = new bool[dim, dim];

            for (int i = 0; i < dim; i++) {
                for (int j = 0; j < dim; j++) {
                    data[i, j] = false;
                }
            }
        }

        public bool Contains(Vector2 pos) {
            return data[
                posToIndex(pos.x),
                posToIndex(pos.y)];
        }

        public void Insert(Vector2 pos) {
            data[
                posToIndex(pos.x),
                posToIndex(pos.y)] = true;
        }

        private int posToIndex(float pos) {
            int index = Mathf.FloorToInt(pos / cellSize) + halfDim;

            if (index < 0 || index > data.Length) {
                Debug.LogError("Bad index " + index + " from pos " + pos + " halfDim " + halfDim);
                return 0;
            } else {
                return index;
            }
        }
    }*/
}
