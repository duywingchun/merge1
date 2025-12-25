using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UI để hiển thị inventory hạt giống
/// Nhấn phím I hoặc Tab để mở/đóng
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform seedItemContainer; // Container để chứa các item (Grid Layout Group)
    [SerializeField] private GameObject seedItemPrefab; // Prefab cho mỗi item hạt giống (optional)
    
    [Header("Text References (nếu không dùng prefab)")]
    [SerializeField] private TextMeshProUGUI appleSeedText;
    [SerializeField] private TextMeshProUGUI tomatoSeedText;
    [SerializeField] private TextMeshProUGUI ornamentalSeedText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isInventoryOpen = false;

    private void Awake()
    {
        // Đảm bảo panel bị ẩn ngay từ đầu
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        isInventoryOpen = false;
        
        // Ẩn tất cả text ngay từ đầu
        HideAllTexts();
    }

    private void Start()
    {
        Debug.Log("[InventoryUI] Start() called");

        // Đảm bảo panel vẫn bị ẩn khi Start
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Debug.Log("[InventoryUI] Panel set to inactive");
        }
        else
        {
            Debug.LogError("[InventoryUI] Inventory Panel is not assigned!");
        }
        
        isInventoryOpen = false;

        // Ẩn tất cả text
        HideAllTexts();

        // Đăng ký event từ InventoryManager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSeedCountChanged += UpdateInventoryDisplay;
            Debug.Log("[InventoryUI] Registered to InventoryManager events");
        }
        else
        {
            Debug.LogWarning("[InventoryUI] InventoryManager.Instance is null!");
        }
    }

    private void Update()
    {
        // Toggle inventory khi nhấn phím
        // Thử cả phím I và kiểm tra bằng nhiều cách
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[InventoryUI] I key pressed!");
            ToggleInventory();
        }

        // Đảm bảo panel state đúng với isInventoryOpen
        // (phòng trường hợp có code khác thay đổi panel state)
        if (inventoryPanel != null && inventoryPanel.activeSelf != isInventoryOpen)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }
    }

    /// <summary>
    /// Mở/Đóng inventory
    /// </summary>
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            Debug.Log($"[InventoryUI] Inventory {(isInventoryOpen ? "opened" : "closed")}");
        }

        if (isInventoryOpen)
        {
            UpdateInventoryDisplay();
            ShowAllTexts();
        }
        else
        {
            // Ẩn tất cả text khi đóng
            HideAllTexts();
        }
    }

    /// <summary>
    /// Ẩn tất cả text
    /// </summary>
    private void HideAllTexts()
    {
        if (appleSeedText != null)
        {
            appleSeedText.gameObject.SetActive(false);
        }
        if (tomatoSeedText != null)
        {
            tomatoSeedText.gameObject.SetActive(false);
        }
        if (ornamentalSeedText != null)
        {
            ornamentalSeedText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Cập nhật hiển thị inventory
    /// </summary>
    private void UpdateInventoryDisplay(SeedType seedType = SeedType.Apple, int count = 0)
    {
        if (InventoryManager.Instance == null) return;

        // Cập nhật tất cả số lượng (không chỉ loại vừa thay đổi)
        UpdateAllSeedCounts();

        // Nếu panel đang mở, hiển thị text. Nếu đóng, ẩn text
        if (isInventoryOpen && inventoryPanel != null && inventoryPanel.activeSelf)
        {
            ShowAllTexts();
        }
        else
        {
            HideAllTexts();
        }
    }

    /// <summary>
    /// Cập nhật tất cả số lượng hạt giống
    /// </summary>
    private void UpdateAllSeedCounts()
    {
        if (InventoryManager.Instance == null) return;

        // Cập nhật tất cả text, bất kể panel có mở hay không
        if (appleSeedText != null)
        {
            appleSeedText.text = $"Apple: {InventoryManager.Instance.GetSeedCount(SeedType.Apple)}";
        }
        if (tomatoSeedText != null)
        {
            tomatoSeedText.text = $"Tomato: {InventoryManager.Instance.GetSeedCount(SeedType.Tomato)}";
        }
        if (ornamentalSeedText != null)
        {
            ornamentalSeedText.text = $"Ornamental: {InventoryManager.Instance.GetSeedCount(SeedType.Ornamental)}";
        }
    }

    /// <summary>
    /// Hiển thị tất cả text
    /// </summary>
    private void ShowAllTexts()
    {
        if (appleSeedText != null)
        {
            appleSeedText.gameObject.SetActive(true);
        }
        if (tomatoSeedText != null)
        {
            tomatoSeedText.gameObject.SetActive(true);
        }
        if (ornamentalSeedText != null)
        {
            ornamentalSeedText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Cập nhật inventory dùng prefab (nếu có)
    /// </summary>
    private void UpdateInventoryWithPrefabs()
    {
        // Xóa các item cũ
        foreach (Transform child in seedItemContainer)
        {
            Destroy(child.gameObject);
        }

        // Tạo item mới cho mỗi loại hạt
        if (InventoryManager.Instance != null)
        {
            Dictionary<SeedType, int> allSeeds = InventoryManager.Instance.GetAllSeeds();
            foreach (var seedItem in allSeeds)
            {
                if (seedItem.Value > 0) // Chỉ hiển thị hạt có số lượng > 0
                {
                    GameObject item = Instantiate(seedItemPrefab, seedItemContainer);
                    // TODO: Setup item UI (text, image, etc.)
                    // Có thể cần tạo SeedItemUI component để handle việc này
                }
            }
        }
    }

    /// <summary>
    /// Đóng inventory (public method để gọi từ bên ngoài)
    /// </summary>
    public void CloseInventory()
    {
        isInventoryOpen = false;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký event
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSeedCountChanged -= UpdateInventoryDisplay;
        }
    }

    // Đảm bảo panel bị ẩn khi disable
    private void OnDisable()
    {
        CloseInventory();
    }
}

