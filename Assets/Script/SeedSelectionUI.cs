using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI để chọn loại hạt giống khi trồng cây
/// </summary>
public class SeedSelectionUI : MonoBehaviour
{
    public static SeedSelectionUI Instance { get; private set; }
    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button appleButton;
    [SerializeField] private Button tomatoButton;
    [SerializeField] private Button ornamentalButton;
    [SerializeField] private Button cancelButton;

    [Header("Text References (Optional)")]
    [SerializeField] private TextMeshProUGUI appleCountText;
    [SerializeField] private TextMeshProUGUI tomatoCountText;
    [SerializeField] private TextMeshProUGUI ornamentalCountText;

    // Lưu TextMeshProUGUI từ buttons (tự động lấy)
    private TextMeshProUGUI appleButtonText;
    private TextMeshProUGUI tomatoButtonText;
    private TextMeshProUGUI ornamentalButtonText;

    private PlantableSpot currentPlantableSpot;
    private System.Action<PlantData> onSeedSelected;
    private System.Action<SeedType, int> inventoryUpdateHandler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Đóng panel khi bắt đầu
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        // Lấy TextMeshProUGUI từ buttons (nếu có)
        if (appleButton != null)
        {
            appleButtonText = appleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (appleButtonText != null)
            {
                Debug.Log("[SeedSelectionUI] Found Apple button text");
            }
            else
            {
                Debug.LogWarning("[SeedSelectionUI] Apple button text not found!");
            }
            appleButton.onClick.AddListener(() => SelectSeed(SeedType.Apple));
        }
        if (tomatoButton != null)
        {
            tomatoButtonText = tomatoButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tomatoButtonText != null)
            {
                Debug.Log("[SeedSelectionUI] Found Tomato button text");
            }
            else
            {
                Debug.LogWarning("[SeedSelectionUI] Tomato button text not found!");
            }
            tomatoButton.onClick.AddListener(() => SelectSeed(SeedType.Tomato));
        }
        if (ornamentalButton != null)
        {
            ornamentalButtonText = ornamentalButton.GetComponentInChildren<TextMeshProUGUI>();
            if (ornamentalButtonText != null)
            {
                Debug.Log("[SeedSelectionUI] Found Ornamental button text");
            }
            else
            {
                Debug.LogWarning("[SeedSelectionUI] Ornamental button text not found!");
            }
            ornamentalButton.onClick.AddListener(() => SelectSeed(SeedType.Ornamental));
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelSelection);
        }

        // Đăng ký event từ InventoryManager để tự động cập nhật số lượng
        inventoryUpdateHandler = (seedType, count) => {
            // Chỉ cập nhật nếu panel đang mở
            if (selectionPanel != null && selectionPanel.activeSelf)
            {
                UpdateSeedCounts();
            }
        };

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnSeedCountChanged += inventoryUpdateHandler;
        }
    }

    private void Update()
    {
        // Đóng bằng phím Escape
        if (selectionPanel != null && selectionPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelSelection();
        }
    }

    /// <summary>
    /// Mở UI chọn hạt giống
    /// </summary>
    public void ShowSeedSelection(PlantableSpot plantableSpot, System.Action<PlantData> callback)
    {
        Debug.Log("[SeedSelectionUI] ShowSeedSelection called");
        
        currentPlantableSpot = plantableSpot;
        onSeedSelected = callback;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            Debug.Log("[SeedSelectionUI] Panel opened, updating seed counts...");
            UpdateSeedCounts();
        }
        else
        {
            Debug.LogError("[SeedSelectionUI] Selection Panel is not assigned!");
        }
    }

    /// <summary>
    /// Chọn loại hạt giống
    /// </summary>
    private void SelectSeed(SeedType seedType)
    {
        Debug.Log($"[SeedSelectionUI] SelectSeed called with {seedType}");

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SeedSelectionUI] InventoryManager not found!");
            return;
        }

        // Kiểm tra xem có hạt giống không
        if (!InventoryManager.Instance.HasSeed(seedType))
        {
            Debug.Log($"[SeedSelectionUI] You don't have {seedType} seeds!");
            return;
        }

        Debug.Log($"[SeedSelectionUI] Player has {seedType} seeds, finding PlantData...");

        // Tìm PlantData tương ứng
        PlantData plantData = FindPlantDataBySeedType(seedType);
        if (plantData == null)
        {
            Debug.LogError($"[SeedSelectionUI] PlantData not found for {seedType}!");
            return;
        }

        Debug.Log($"[SeedSelectionUI] Found PlantData: {plantData.name}, calling callback...");

        // Gọi callback
        if (onSeedSelected != null)
        {
            onSeedSelected.Invoke(plantData);
            Debug.Log("[SeedSelectionUI] Callback invoked successfully");
        }
        else
        {
            Debug.LogError("[SeedSelectionUI] onSeedSelected callback is null!");
        }

        // Đóng panel
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
            Debug.Log("[SeedSelectionUI] Panel closed");
        }
    }

    /// <summary>
    /// Hủy chọn
    /// </summary>
    private void CancelSelection()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
        currentPlantableSpot = null;
        onSeedSelected = null;
    }

    /// <summary>
    /// Cập nhật số lượng hạt giống hiển thị
    /// </summary>
    private void UpdateSeedCounts()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[SeedSelectionUI] InventoryManager is null, cannot update counts");
            return;
        }

        int appleCount = InventoryManager.Instance.GetSeedCount(SeedType.Apple);
        int tomatoCount = InventoryManager.Instance.GetSeedCount(SeedType.Tomato);
        int ornamentalCount = InventoryManager.Instance.GetSeedCount(SeedType.Ornamental);

        Debug.Log($"[SeedSelectionUI] Counts - Apple: {appleCount}, Tomato: {tomatoCount}, Ornamental: {ornamentalCount}");

        // Nếu có text riêng, cập nhật text riêng
        if (appleCountText != null)
        {
            appleCountText.text = appleCount.ToString();
        }
        if (tomatoCountText != null)
        {
            tomatoCountText.text = tomatoCount.ToString();
        }
        if (ornamentalCountText != null)
        {
            ornamentalCountText.text = ornamentalCount.ToString();
        }

        // Cập nhật text trong button (nếu có)
        if (appleButtonText != null)
        {
            appleButtonText.text = $"Apple ({appleCount})";
            Debug.Log($"[SeedSelectionUI] Updated Apple button text: {appleButtonText.text}");
        }
        else
        {
            Debug.LogWarning("[SeedSelectionUI] Apple button text is null!");
        }
        
        if (tomatoButtonText != null)
        {
            tomatoButtonText.text = $"Tomato ({tomatoCount})";
            Debug.Log($"[SeedSelectionUI] Updated Tomato button text: {tomatoButtonText.text}");
        }
        else
        {
            Debug.LogWarning("[SeedSelectionUI] Tomato button text is null!");
        }
        
        if (ornamentalButtonText != null)
        {
            ornamentalButtonText.text = $"Ornamental ({ornamentalCount})";
            Debug.Log($"[SeedSelectionUI] Updated Ornamental button text: {ornamentalButtonText.text}");
        }
        else
        {
            Debug.LogWarning("[SeedSelectionUI] Ornamental button text is null!");
        }
    }

    /// <summary>
    /// Tìm PlantData theo SeedType
    /// </summary>
    private PlantData FindPlantDataBySeedType(SeedType seedType)
    {
        // Tìm từ PlantDataManager
        if (PlantDataManager.Instance != null)
        {
            return PlantDataManager.Instance.GetPlantData(seedType);
        }

        // Fallback: Tìm trong allowedPlants của PlantableSpot
        if (currentPlantableSpot != null)
        {
            // Có thể thêm method GetAllowedPlants() vào PlantableSpot nếu cần
        }

        return null;
    }

    private void OnDestroy()
    {
        // Hủy đăng ký event
        if (InventoryManager.Instance != null && inventoryUpdateHandler != null)
        {
            InventoryManager.Instance.OnSeedCountChanged -= inventoryUpdateHandler;
        }
    }
}

