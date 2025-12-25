using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject chứa thông tin về một loại cây
/// Bao gồm sprites các giai đoạn phát triển và thời gian mỗi giai đoạn
/// </summary>
[CreateAssetMenu(fileName = "NewPlantData", menuName = "MyGarden/Plant Data")]
public class PlantData : ScriptableObject
{
    [Header("Plant Information")]
    public string plantName = "New Plant";
    public SeedType seedType;
    
    [Header("Growth Stages")]
    [Tooltip("Sprites cho từng giai đoạn phát triển (0: Hạt, 1: Mầm, 2: Lá, 3: Trưởng thành)")]
    public List<Sprite> growthStageSprites = new List<Sprite>();
    
    [Tooltip("Thời gian phát triển của từng giai đoạn (giây). Số phần tử phải bằng số sprites - 1")]
    public List<float> stageDurations = new List<float>();
    
    [Header("Harvest Settings")]
    [Tooltip("Số lượng hạt giống nhận được khi thu hoạch")]
    public int seedRewardCount = 1;
    
    [Tooltip("Prefab của hạt giống khi rơi ra (optional)")]
    public GameObject seedPrefab;

    /// <summary>
    /// Kiểm tra xem dữ liệu có hợp lệ không
    /// </summary>
    public bool IsValid()
    {
        if (growthStageSprites == null || growthStageSprites.Count < 2)
        {
            Debug.LogError($"[PlantData] {plantName}: Cần ít nhất 2 sprites (hạt và trưởng thành)");
            return false;
        }

        if (stageDurations == null || stageDurations.Count != growthStageSprites.Count - 1)
        {
            Debug.LogError($"[PlantData] {plantName}: Số lượng stageDurations phải bằng số sprites - 1");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Lấy tổng thời gian phát triển từ hạt đến trưởng thành
    /// </summary>
    public float GetTotalGrowthTime()
    {
        float total = 0f;
        foreach (float duration in stageDurations)
        {
            total += duration;
        }
        return total;
    }
}

