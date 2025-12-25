using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script để spawn cây có sẵn ở Forest scene để player có thể thu hoạch hạt giống
/// </summary>
public class ForestSeedSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private List<PlantData> plantTypesToSpawn = new List<PlantData>(); // Các loại cây muốn spawn
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>(); // Vị trí spawn cây
    [SerializeField] private bool spawnOnStart = true; // Spawn khi Start
    [SerializeField] private bool spawnMaturePlants = true; // Spawn cây trưởng thành để có thể thu hoạch ngay

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnAllPlants();
        }
    }

    /// <summary>
    /// Spawn tất cả cây
    /// </summary>
    public void SpawnAllPlants()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[ForestSeedSpawner] No spawn points assigned!");
            return;
        }

        if (plantTypesToSpawn == null || plantTypesToSpawn.Count == 0)
        {
            Debug.LogWarning("[ForestSeedSpawner] No plant types to spawn!");
            return;
        }

        int plantIndex = 0;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            // Chọn loại cây (có thể random hoặc theo thứ tự)
            PlantData plantData = plantTypesToSpawn[plantIndex % plantTypesToSpawn.Count];
            plantIndex++;

            SpawnPlant(spawnPoint.position, plantData);
        }
    }

    /// <summary>
    /// Spawn một cây tại vị trí
    /// </summary>
    private void SpawnPlant(Vector3 position, PlantData plantData)
    {
        if (plantData == null)
        {
            Debug.LogWarning("[ForestSeedSpawner] PlantData is null!");
            return;
        }

        // Tạo GameObject cây
        GameObject newPlant = new GameObject(plantData.plantName + "_Forest");
        newPlant.transform.position = position;

        // Thêm SpriteRenderer
        SpriteRenderer plantRenderer = newPlant.AddComponent<SpriteRenderer>();
        if (plantData.growthStageSprites != null && plantData.growthStageSprites.Count > 0)
        {
            int spriteIndex = spawnMaturePlants ? plantData.growthStageSprites.Count - 1 : 0;
            plantRenderer.sprite = plantData.growthStageSprites[spriteIndex];
        }

        // Thiết lập layer
        plantRenderer.sortingLayerName = "Tree";
        plantRenderer.sortingOrder = 5;

        // Thêm Collider2D (trigger) để có thể thu hoạch
        CircleCollider2D collider = newPlant.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1f;

        // Thêm PlantGrowth script
        PlantGrowth plantGrowth = newPlant.AddComponent<PlantGrowth>();
        plantGrowth.SetPlantData(plantData);

        // Nếu spawn mature, set ở giai đoạn cuối
        if (spawnMaturePlants)
        {
            int maxStage = plantData.growthStageSprites.Count - 1;
            plantGrowth.SetStage(maxStage);
        }

        // Thêm SeedHarvestable script
        SeedHarvestable harvestable = newPlant.AddComponent<SeedHarvestable>();
        harvestable.SetPlantData(plantData);

        Debug.Log($"[ForestSeedSpawner] Spawned {plantData.plantName} at {position}");
    }
}

