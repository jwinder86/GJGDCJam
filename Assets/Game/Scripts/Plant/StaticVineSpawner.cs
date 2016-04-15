using UnityEngine;
using System.Collections;
using System;

public class StaticVineSpawner : MonoBehaviour, PlantManager {

    public ProceduralVinePlant prefab;
    public Transform target;

    private SoundManager soundManager;
    private ProceduralVinePlant instance;

    public int TotalValue {
        get { return 1; }
    }

    public int CurrentValue {
        get {
            return (instance == null || !instance.Spawned) ? 0 : 1;
        }
    }

    public void GeneratePlants() {
        instance = (ProceduralVinePlant)Instantiate(prefab, transform.position, Quaternion.identity);
        instance.transform.parent = transform;
        instance.SetNormal((target.position - transform.position).normalized);
        instance.SetEndpoint(target.position);
        instance.SoundManager = FindObjectOfType<SoundManager>(); ;
    }

    public void CleanUp() { }

    public void UpdatePlants(Vector3 playerPos) {
        float playerDist = (transform.position - playerPos).magnitude;

        if (playerDist <= prefab.spawnDist * (1f + prefab.spawnDistRand)) {
            instance.UpdatePlayerPos(playerPos);
        }
    }
}
