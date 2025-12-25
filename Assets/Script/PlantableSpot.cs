using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script gắn vào vị trí có thể trồng cây (như ô đất)
/// </summary>
public class PlantableSpot : MonoBehaviour
{
    [Header("Plant Settings")]
    [SerializeField] private PlantData defaultPlantData; // Loại cây mặc định (nếu không dùng allowedPlants)
    [SerializeField] private List<PlantData> allowedPlants = new List<PlantData>(); // Danh sách các loại cây có thể trồng
    [SerializeField] private bool useAllowedPlantsList = true; // Bật/tắt việc sử dụng danh sách allowedPlants
    [SerializeField] private bool useSeedSelectionUI = false; // Nếu true, mở UI để chọn hạt giống
    [SerializeField] private bool spawnMaturePlant = false; // Nếu true, trồng cây ở giai đoạn trưởng thành ngay (để test)
    [SerializeField] private Vector3 plantScale = Vector3.one; // Scale của cây (để thu nhỏ/phóng to)
    [SerializeField] private string sortingLayerName = "Default"; // Layer để hiển thị cây
    [SerializeField] private int orderInLayer = 1; // Order in Layer (số càng cao càng hiển thị trên)
    [SerializeField] private GameObject plantPrefab; // Prefab của cây (nếu dùng prefab)
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer highlightRenderer; // Sprite để highlight khi player đến gần
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private GameObject plantEffectPrefab; // Effect khi trồng

    private bool playerInRange = false;
    private bool hasPlant = false;
    private GameObject currentPlant;
    private Color originalColor;

    private void Start()
    {
        // Setup highlight
        if (highlightRenderer == null)
        {
            highlightRenderer = GetComponent<SpriteRenderer>();
        }

        if (highlightRenderer != null)
        {
            originalColor = highlightRenderer.color;
        }

        // Kiểm tra xem đã có cây chưa
        CheckForExistingPlant();
    }

    private void Update()
    {
        if (playerInRange && !hasPlant)
        {
            // Hiển thị highlight
            if (highlightRenderer != null)
            {
                highlightRenderer.color = Color.Lerp(originalColor, highlightColor, 0.5f);
            }

            // Không cho phép tương tác khi đang chat
            if (ChatUI.IsChatting)
                return;
            
            // Kiểm tra input để trồng cây
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("[PlantableSpot] E key pressed, attempting to plant...");
                if (useSeedSelectionUI && SeedSelectionUI.Instance != null)
                {
                    // Mở UI chọn hạt giống
                    SeedSelectionUI.Instance.ShowSeedSelection(this, OnSeedSelected);
                }
                else
                {
                    // Trồng tự động như cũ
                    TryPlantSeed();
                }
            }
        }
        else
        {
            // Reset highlight
            if (highlightRenderer != null)
            {
                highlightRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem vị trí này đã có cây chưa
    /// </summary>
    private void CheckForExistingPlant()
    {
        // Kiểm tra xem có GameObject con nào là cây không
        PlantGrowth existingPlant = GetComponentInChildren<PlantGrowth>();
        if (existingPlant != null)
        {
            hasPlant = true;
            currentPlant = existingPlant.gameObject;
        }
    }

    /// <summary>
    /// Thử trồng hạt giống
    /// </summary>
    private void TryPlantSeed()
    {
        if (hasPlant)
        {
            Debug.Log("[PlantableSpot] This spot already has a plant!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[PlantableSpot] InventoryManager not found!");
            return;
        }

        PlantData plantDataToUse = null;

        // Nếu sử dụng danh sách allowedPlants, tìm loại cây đầu tiên có trong inventory
        if (useAllowedPlantsList && allowedPlants != null && allowedPlants.Count > 0)
        {
            plantDataToUse = FindFirstAvailablePlant();
        }

        // Nếu không tìm thấy, dùng defaultPlantData
        if (plantDataToUse == null)
        {
            plantDataToUse = defaultPlantData;
        }

        if (plantDataToUse == null)
        {
            Debug.LogWarning("[PlantableSpot] No plant data available. Please assign default plant data or add plants to allowed plants list.");
            return;
        }

        // Kiểm tra inventory
        if (!InventoryManager.Instance.HasSeed(plantDataToUse.seedType))
        {
            Debug.Log($"[PlantableSpot] You don't have {plantDataToUse.seedType} seeds!");
            return;
        }

        // Sử dụng hạt giống
        if (!InventoryManager.Instance.UseSeed(plantDataToUse.seedType, 1))
        {
            return;
        }

        // Trồng cây
        PlantSeed(plantDataToUse);
    }

    /// <summary>
    /// Tìm loại cây đầu tiên mà player có trong inventory
    /// </summary>
    private PlantData FindFirstAvailablePlant()
    {
        if (allowedPlants == null || allowedPlants.Count == 0)
        {
            return null;
        }

        foreach (PlantData plantData in allowedPlants)
        {
            if (plantData == null) continue;

            if (InventoryManager.Instance.HasSeed(plantData.seedType))
            {
                return plantData;
            }
        }

        return null;
    }

    /// <summary>
    /// Trồng cây tại vị trí này
    /// </summary>
    public void PlantSeed(PlantData plantData)
    {
        if (hasPlant)
        {
            Debug.LogWarning("[PlantableSpot] Cannot plant: spot already occupied");
            return;
        }

        GameObject newPlant;

        // Nếu có prefab, dùng prefab. Nếu không, tạo từ sprite
        if (plantPrefab != null)
        {
            newPlant = Instantiate(plantPrefab, transform.position, Quaternion.identity);
            newPlant.transform.SetParent(transform);
            newPlant.transform.localScale = plantScale; // Áp dụng scale
        }
        else
        {
            // Tạo GameObject mới cho cây
            newPlant = new GameObject(plantData.plantName + "_Plant");
            newPlant.transform.position = transform.position;
            newPlant.transform.SetParent(transform);
            newPlant.transform.localScale = plantScale; // Áp dụng scale

            // Thêm SpriteRenderer
            SpriteRenderer plantRenderer = newPlant.AddComponent<SpriteRenderer>();
            if (plantData.growthStageSprites != null && plantData.growthStageSprites.Count > 0)
            {
                plantRenderer.sprite = plantData.growthStageSprites[0]; // Sprite giai đoạn đầu
            }
            
            // Thiết lập layer và order
            plantRenderer.sortingLayerName = sortingLayerName;
            plantRenderer.sortingOrder = orderInLayer;

            // Thêm Collider2D (trigger) để có thể thu hoạch
            CircleCollider2D collider = newPlant.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }

        // Thêm PlantGrowth script
        PlantGrowth plantGrowth = newPlant.GetComponent<PlantGrowth>();
        if (plantGrowth == null)
        {
            plantGrowth = newPlant.AddComponent<PlantGrowth>();
        }
        plantGrowth.SetPlantData(plantData);

        // Nếu spawnMaturePlant = true, set cây ở giai đoạn trưởng thành ngay
        if (spawnMaturePlant)
        {
            int maxStage = plantData.growthStageSprites.Count - 1;
            if (maxStage >= 0)
            {
                plantGrowth.SetStage(maxStage); // Set ở giai đoạn cuối (trưởng thành)
                
                // Cập nhật sprite để hiển thị đúng
                SpriteRenderer plantRenderer = newPlant.GetComponent<SpriteRenderer>();
                if (plantRenderer != null && maxStage < plantData.growthStageSprites.Count)
                {
                    plantRenderer.sprite = plantData.growthStageSprites[maxStage];
                }
            }
        }

        // Thêm SeedHarvestable script (sẽ được kích hoạt khi cây trưởng thành)
        SeedHarvestable harvestable = newPlant.GetComponent<SeedHarvestable>();
        if (harvestable == null)
        {
            harvestable = newPlant.AddComponent<SeedHarvestable>();
        }
        harvestable.SetPlantData(plantData);
        harvestable.SetPlantableSpot(this); // Lưu reference đến PlantableSpot này

        currentPlant = newPlant;
        hasPlant = true;

        // Spawn effect
        if (plantEffectPrefab != null)
        {
            Instantiate(plantEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"[PlantableSpot] Planted {plantData.plantName} at {transform.position}");
        
        // Không lưu cây vào database (chỉ lưu vị trí player và hạt giống)
    }

    /// <summary>
    /// Xóa cây (dùng khi thu hoạch hoặc nhổ cây)
    /// </summary>
    public void RemovePlant()
    {
        if (currentPlant != null)
        {
            Destroy(currentPlant);
            currentPlant = null;
        }
        hasPlant = false;
    }
    
    // Đã xóa phần lưu cây vào database - chỉ lưu vị trí player và hạt giống

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"[PlantableSpot] Player entered range. HasPlant: {hasPlant}");
            if (!hasPlant)
            {
                Debug.Log("[PlantableSpot] Press E to plant a seed here.");
            }
        }
        else
        {
            Debug.Log($"[PlantableSpot] OnTriggerEnter2D: {other.tag} (not Player)");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("[PlantableSpot] Player exited range.");
        }
    }

    /// <summary>
    /// Thiết lập loại cây mặc định
    /// </summary>
    public void SetDefaultPlantData(PlantData data)
    {
        defaultPlantData = data;
    }

    /// <summary>
    /// Thêm loại cây vào danh sách được phép trồng
    /// </summary>
    public void AddAllowedPlant(PlantData plantData)
    {
        if (allowedPlants == null)
        {
            allowedPlants = new List<PlantData>();
        }

        if (plantData != null && !allowedPlants.Contains(plantData))
        {
            allowedPlants.Add(plantData);
        }
    }

    /// <summary>
    /// Xóa loại cây khỏi danh sách được phép trồng
    /// </summary>
    public void RemoveAllowedPlant(PlantData plantData)
    {
        if (allowedPlants != null && plantData != null)
        {
            allowedPlants.Remove(plantData);
        }
    }

    /// <summary>
    /// Thiết lập danh sách các loại cây được phép trồng
    /// </summary>
    public void SetAllowedPlants(List<PlantData> plants)
    {
        allowedPlants = plants;
    }

    /// <summary>
    /// Kiểm tra xem vị trí có cây chưa
    /// </summary>
    public bool HasPlant()
    {
        return hasPlant;
    }

    /// <summary>
    /// Lấy GameObject của cây hiện tại (để SeedHarvestable kiểm tra)
    /// </summary>
    public GameObject GetCurrentPlant()
    {
        return currentPlant;
    }
    
    /// <summary>
    /// Set cây hiện tại (dùng khi load từ database)
    /// </summary>
    public void SetCurrentPlant(GameObject plant)
    {
        currentPlant = plant;
        hasPlant = (plant != null);
    }
    
    /// <summary>
    /// Set trạng thái có cây (dùng khi load từ database)
    /// </summary>
    public void SetHasPlant(bool value)
    {
        hasPlant = value;
    }

    /// <summary>
    /// Callback khi player chọn hạt giống từ UI
    /// </summary>
    public void OnSeedSelected(PlantData selectedPlantData)
    {
        Debug.Log($"[PlantableSpot] OnSeedSelected called with {selectedPlantData?.name ?? "null"}");

        if (selectedPlantData == null)
        {
            Debug.LogWarning("[PlantableSpot] Selected plant data is null");
            return;
        }

        // Kiểm tra inventory
        if (InventoryManager.Instance == null || !InventoryManager.Instance.HasSeed(selectedPlantData.seedType))
        {
            Debug.Log($"[PlantableSpot] You don't have {selectedPlantData.seedType} seeds!");
            return;
        }

        Debug.Log($"[PlantableSpot] Using seed {selectedPlantData.seedType}...");

        // Sử dụng hạt giống
        if (!InventoryManager.Instance.UseSeed(selectedPlantData.seedType, 1))
        {
            Debug.LogWarning("[PlantableSpot] Failed to use seed");
            return;
        }

        Debug.Log("[PlantableSpot] Seed used, planting...");

        // Trồng cây
        PlantSeed(selectedPlantData);
    }
}

