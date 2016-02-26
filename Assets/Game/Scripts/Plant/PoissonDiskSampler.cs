using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoissonDiskSampler {

    private float radius;
    private float minDist;
    private int k;

    public delegate bool PlaceSample(Vector2 sample);

    public PoissonDiskSampler(float radius, float minDist, int k = 30) {
        this.radius = radius;
        this.minDist = minDist;
        this.k = k;
    }

    public List<Vector2> generateSamples(PlaceSample place) {
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();
        SpacialLookup lookup = new SpacialLookup(radius, minDist);

        if (!AddFirstSamples(ref samples, ref active, ref lookup, place)) {
            Debug.LogError("Unable to add first sample.");
            return samples;
        }

        while (active.Count > 0) {
            // select random index and move to end
            int index = Random.Range(0, active.Count);
            swap(ref active, index, active.Count - 1);
            Vector2 selected = active[active.Count - 1];
            bool found = false;

            for (int i = 0; i < k; i++) {
                Vector2 newSample = selected + RandomVector();

                if (validateAndAddSample(newSample, ref samples, ref active, ref lookup, place)) {
                    found = true;
                }

            }

            if (!found) {
                active.RemoveAt(active.Count - 1);
            }
        }

        return samples;
    }

    private bool validateAndAddSample(Vector2 sample, ref List<Vector2> samples, ref List<Vector2> active, ref SpacialLookup lookup, PlaceSample place) {
        if (validateBounds(sample) && validateDistSamples(ref lookup, sample) && place(sample)) {
            samples.Add(sample);
            active.Add(sample);
            lookup.Insert(sample);
            return true;
        }
        return false;
    }

    private bool AddFirstSamples(ref List<Vector2> samples, ref List<Vector2> active, ref SpacialLookup lookup, PlaceSample place) {
        bool found = false;
        for (int i = 0; i < k; i++) {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float radius = Random.Range(0f, this.radius);
            Vector2 point = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));

            if (validateAndAddSample(point, ref samples, ref active, ref lookup, place)) {
                found = true;
            }
        }

        return found;
    }

    private bool validateBounds(Vector2 newSample) {
        return (newSample.sqrMagnitude < radius * radius);
    }

    /*private bool validateDistSamples(ref List<Vector2> samples, Vector2 newSample) {
        float sqrDist = minDist * minDist;
        for (int i = 0; i < samples.Count; i++) {
            if ((samples[i] - newSample).sqrMagnitude < sqrDist) {
                return false;
            }
        }

        return true;
    }*/

    private bool validateDistSamples(ref SpacialLookup lookup, Vector2 newSample) {
        return !lookup.Collides(newSample, minDist);
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

    private class SpacialLookup {
        private float cellSize;
        private Vector2?[,] data;
        private int dim, halfDim;

        public SpacialLookup(float radius, float minDist) {
            cellSize = minDist / Mathf.Sqrt(2f);

            halfDim = Mathf.CeilToInt(radius / cellSize);
            dim = halfDim * 2;

            data = new Vector2?[dim, dim];

            for (int i = 0; i < dim; i++) {
                for (int j = 0; j < dim; j++) {
                    data[i, j] = null;
                }
            }
        }

        public bool Collides(Vector2 pos, float dist) {
            float distSqr = dist * dist;

            int posX = posToIndex(pos.x);
            int posY = posToIndex(pos.y);

            for (int x = posX-1; x <= posX+1; x++) {
                for (int y = posY-1; y <= posY+1; y++) {
                    if (x >= 0 && x < dim &&
                            y >= 0 && y < dim &&
                            data[x, y].HasValue) {
                        Vector2 other = data[x, y].Value;
                        if ((pos - other).sqrMagnitude < distSqr) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Insert(Vector2 pos) {
            data[
                posToIndex(pos.x),
                posToIndex(pos.y)] = pos;
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
    }
}
