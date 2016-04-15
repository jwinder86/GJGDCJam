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

    public int generateSamples(PlaceSample place, List<Vector2> externalSamples = null) {
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> active = new List<Vector2>();
        SpacialLookup lookup = new SpacialLookup(radius, minDist);

        //Debug.Log("Generating samples!");

        // add external samples
        if (externalSamples != null) {
            //Debug.Log("Sampler adding " + externalSamples.Count + " external samples");
            foreach (Vector2 ex in externalSamples) {
                lookup.Insert(ex);
            }
        }
        
        if (!AddFirstSamples(ref samples, ref active, ref lookup, place)) {
            if (!AddFirstSamples(ref samples, ref active, ref lookup, place)) {
                Debug.LogWarning("Unable to add first sample after 2 attempts.");
                return samples.Count;
            }
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
                //Debug.Log("No point found after " + k + " iterations, removing active point: " + selected);
                active.RemoveAt(active.Count - 1);
            }
        }

        return samples.Count;
    }

    private bool validateAndAddSample(Vector2 sample, ref List<Vector2> samples, ref List<Vector2> active, ref SpacialLookup lookup, PlaceSample place) {
        //Debug.Log("Attempting to add sample: " + sample);
        if (validateBounds(sample) && validateDistSamples(ref lookup, sample) && place(sample)) {
            samples.Add(sample);
            active.Add(sample);
            lookup.Insert(sample);

            //Debug.Log("Sample Added!");
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
                    if (validIndex(x) &&
                            validIndex(y) &&
                            data[x, y].HasValue) {
                        Vector2 other = data[x, y].Value;
                        if ((pos - other).sqrMagnitude < distSqr) {
                            //Debug.Log("Collision at " + pos + " with " + other);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Insert(Vector2 pos) {
            //Debug.Log("Inserting position: " + pos);
            if (validPosition(pos)) {
                data[
                    posToIndex(pos.x),
                    posToIndex(pos.y)] = pos;
            } /*else {
                Debug.Log("invalid position: " + pos);
            }*/
        }

        private bool validPosition(Vector2 pos) {
            int indexX = posToIndex(pos.x);
            int indexY = posToIndex(pos.y);

            if (validIndex(indexX) && validIndex(indexY)) {
                return true;
            } else {
                return false;
            }
        }

        private bool validIndex(int index) {
            return (index >= 0 && index < dim);
        }

        private int posToIndex(float pos) {
            return Mathf.FloorToInt(pos / cellSize) + halfDim;
        }
    }
}
