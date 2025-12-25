using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager để quản lý và tìm PlantData theo SeedType
/// </summary>
public class PlantDataManager : MonoBehaviour
{
    public static PlantDataManager Instance { get; private set; }

    [Header("Plant Data Assets")]
    [SerializeField] private PlantData applePlantData;
    [SerializeField] private PlantData tomatoPlantData;
    [SerializeField] private PlantData ornamentalPlantData;

    private Dictionary<SeedType, PlantData> plantDataDictionary = new Dictionary<SeedType, PlantData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Khởi tạo dictionary
    /// </summary>
    private void InitializeDictionary()
    {
        if (applePlantData != null)
        {
            plantDataDictionary[SeedType.Apple] = applePlantData;
        }
        if (tomatoPlantData != null)
        {
            plantDataDictionary[SeedType.Tomato] = tomatoPlantData;
        }
        if (ornamentalPlantData != null)
        {
            plantDataDictionary[SeedType.Ornamental] = ornamentalPlantData;
        }
    }

    /// <summary>
    /// Lấy PlantData theo SeedType
    /// </summary>
    public PlantData GetPlantData(SeedType seedType)
    {
        if (plantDataDictionary.ContainsKey(seedType))
        {
            return plantDataDictionary[seedType];
        }
        return null;
    }

    /// <summary>
    /// Thêm PlantData
    /// </summary>
    public void RegisterPlantData(SeedType seedType, PlantData plantData)
    {
        if (plantData != null)
        {
            plantDataDictionary[seedType] = plantData;
        }
    }
}

