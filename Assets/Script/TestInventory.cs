using UnityEngine;

/// <summary>
/// Script test để thêm hạt giống vào inventory khi bắt đầu game
/// </summary>
public class TestInventory : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoAddSeedsOnStart = true;
    [SerializeField] private int appleSeedCount = 5;
    [SerializeField] private int tomatoSeedCount = 5;
    [SerializeField] private int ornamentalSeedCount = 2;

    void Start()
    {
        if (autoAddSeedsOnStart)
        {
            AddTestSeeds();
        }
    }

    /// <summary>
    /// Thêm hạt giống vào inventory để test
    /// </summary>
    public void AddTestSeeds()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[TestInventory] ✗ InventoryManager not found! Make sure InventoryManager GameObject exists in scene.");
            return;
        }

        InventoryManager.Instance.AddSeed(SeedType.Apple, appleSeedCount);
        InventoryManager.Instance.AddSeed(SeedType.Tomato, tomatoSeedCount);
        InventoryManager.Instance.AddSeed(SeedType.Ornamental, ornamentalSeedCount);

        Debug.Log($"[TestInventory] ✓ Đã thêm hạt giống vào inventory:");
        Debug.Log($"  - Apple: {appleSeedCount}");
        Debug.Log($"  - Tomato: {tomatoSeedCount}");
        Debug.Log($"  - Ornamental: {ornamentalSeedCount}");
    }

    /// <summary>
    /// Method để gọi từ button hoặc code khác
    /// </summary>
    [ContextMenu("Add Test Seeds")]
    public void AddSeedsManually()
    {
        AddTestSeeds();
    }
}

