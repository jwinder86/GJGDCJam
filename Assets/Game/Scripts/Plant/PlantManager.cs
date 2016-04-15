using UnityEngine;
using System.Collections;

public interface PlantManager {
    void GeneratePlants();
    void CleanUp();
    void UpdatePlants(Vector3 playerPos);

    int TotalValue { get; }
    int CurrentValue { get; }
}
