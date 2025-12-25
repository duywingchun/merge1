using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton quản lý inventory của player
/// Lưu trữ và quản lý các hạt giống
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private Dictionary<SeedType, int> seedsInventory = new Dictionary<SeedType, int>();

    // Event để notify UI khi inventory thay đổi
    public System.Action<SeedType, int> OnSeedCountChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Khởi tạo inventory (có thể thêm hạt giống mặc định ở đây)
    /// </summary>
    private void InitializeInventory()
    {
        // Khởi tạo tất cả loại hạt với số lượng 0
        foreach (SeedType seedType in System.Enum.GetValues(typeof(SeedType)))
        {
            if (!seedsInventory.ContainsKey(seedType))
            {
                seedsInventory[seedType] = 0;
            }
        }

        Debug.Log("[InventoryManager] Inventory initialized");
    }

    /// <summary>
    /// Thêm hạt giống vào inventory
    /// </summary>
    public void AddSeed(SeedType seedType, int quantity = 1)
    {
        if (seedsInventory.ContainsKey(seedType))
        {
            seedsInventory[seedType] += quantity;
        }
        else
        {
            seedsInventory[seedType] = quantity;
        }

        Debug.Log($"[InventoryManager] Added {quantity} {seedType} seed(s). Total: {seedsInventory[seedType]}");
        
        // Notify UI
        OnSeedCountChanged?.Invoke(seedType, seedsInventory[seedType]);
    }

    /// <summary>
    /// Lấy số lượng hạt giống hiện có
    /// </summary>
    public int GetSeedCount(SeedType seedType)
    {
        if (seedsInventory.ContainsKey(seedType))
        {
            return seedsInventory[seedType];
        }
        return 0;
    }

    /// <summary>
    /// Kiểm tra xem có đủ hạt giống không
    /// </summary>
    public bool HasSeed(SeedType seedType, int quantity = 1)
    {
        return GetSeedCount(seedType) >= quantity;
    }

    /// <summary>
    /// Sử dụng hạt giống (trừ khỏi inventory)
    /// </summary>
    public bool UseSeed(SeedType seedType, int quantity = 1)
    {
        if (!HasSeed(seedType, quantity))
        {
            Debug.LogWarning($"[InventoryManager] Không đủ {seedType} seed. Yêu cầu: {quantity}, Hiện có: {GetSeedCount(seedType)}");
            return false;
        }

        seedsInventory[seedType] -= quantity;
        Debug.Log($"[InventoryManager] Used {quantity} {seedType} seed(s). Remaining: {seedsInventory[seedType]}");
        
        // Notify UI
        OnSeedCountChanged?.Invoke(seedType, seedsInventory[seedType]);
        return true;
    }

    /// <summary>
    /// Lấy tất cả hạt giống trong inventory (dùng cho UI)
    /// </summary>
    public Dictionary<SeedType, int> GetAllSeeds()
    {
        return new Dictionary<SeedType, int>(seedsInventory);
    }

    /// <summary>
    /// Debug: In ra tất cả hạt giống trong inventory
    /// </summary>
    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        foreach (var item in seedsInventory)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
    }
}

