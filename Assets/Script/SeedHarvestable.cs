using UnityEngine;

/// <summary>
/// Script gắn vào cây trưởng thành để có thể thu hoạch hạt giống
/// </summary>
public class SeedHarvestable : MonoBehaviour
{
    [Header("Harvest Settings")]
    [SerializeField] private PlantData plantData;
    [SerializeField] private bool canHarvestMultipleTimes = false;
    [SerializeField] private float harvestCooldown = 0f; // Thời gian chờ trước khi có thể thu hoạch lại
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject harvestEffectPrefab;
    [SerializeField] private Transform seedSpawnPoint; // Điểm spawn hạt giống (nếu dùng prefab)

    private bool playerInRange = false;
    private bool hasBeenHarvested = false;
    private float lastHarvestTime = 0f;
    private SpriteRenderer spriteRenderer;
    private PlantableSpot parentPlantableSpot; // Reference đến PlantableSpot chứa cây này

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Nếu không chỉ định spawn point, dùng transform của object
        if (seedSpawnPoint == null)
        {
            seedSpawnPoint = transform;
        }
    }

    private void Start()
    {
        if (plantData == null)
        {
            Debug.LogError($"[SeedHarvestable] {gameObject.name}: PlantData is not assigned!");
        }
        else if (!plantData.IsValid())
        {
            Debug.LogError($"[SeedHarvestable] {gameObject.name}: PlantData is invalid!");
        }
    }

    private void Update()
    {
        // Kiểm tra input khi player trong vùng
        if (playerInRange && CanHarvest())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HarvestSeeds();
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem có thể thu hoạch không
    /// </summary>
    private bool CanHarvest()
    {
        // Nếu đã thu hoạch và không cho phép thu hoạch nhiều lần
        if (hasBeenHarvested && !canHarvestMultipleTimes)
        {
            return false;
        }

        // Kiểm tra cooldown
        if (harvestCooldown > 0f && Time.time - lastHarvestTime < harvestCooldown)
        {
            return false;
        }

        // Kiểm tra cây đã trưởng thành chưa (có thể kiểm tra với PlantGrowth script)
        PlantGrowth plantGrowth = GetComponent<PlantGrowth>();
        if (plantGrowth != null && !plantGrowth.IsFullyGrown())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Thu hoạch hạt giống
    /// </summary>
    private void HarvestSeeds()
    {
        if (plantData == null)
        {
            Debug.LogError("[SeedHarvestable] Cannot harvest: PlantData is null");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[SeedHarvestable] Cannot harvest: InventoryManager not found");
            return;
        }

        // Thêm hạt giống vào inventory
        InventoryManager.Instance.AddSeed(plantData.seedType, plantData.seedRewardCount);

        // Spawn seed prefab nếu có
        if (plantData.seedPrefab != null && seedSpawnPoint != null)
        {
            Instantiate(plantData.seedPrefab, seedSpawnPoint.position, Quaternion.identity);
        }

        // Spawn effect nếu có
        if (harvestEffectPrefab != null)
        {
            Instantiate(harvestEffectPrefab, transform.position, Quaternion.identity);
        }

        hasBeenHarvested = true;
        lastHarvestTime = Time.time;

        Debug.Log($"[SeedHarvestable] Harvested {plantData.seedRewardCount} {plantData.seedType} seed(s) from {plantData.plantName}");

        // Tìm PlantableSpot parent và xóa cây
        PlantableSpot plantableSpot = FindParentPlantableSpot();
        if (plantableSpot != null)
        {
            Debug.Log("[SeedHarvestable] Removing plant from PlantableSpot...");
            plantableSpot.RemovePlant();
        }
        else
        {
            // Nếu không tìm thấy PlantableSpot, tự xóa GameObject này
            Debug.Log("[SeedHarvestable] No PlantableSpot found, destroying plant GameObject");
            Destroy(gameObject);
        }

        // Vô hiệu hóa script (vì cây đã bị xóa)
        this.enabled = false;
    }

    /// <summary>
    /// Tìm PlantableSpot parent (có thể là parent hoặc grandparent)
    /// </summary>
    private PlantableSpot FindParentPlantableSpot()
    {
        // Nếu đã có reference, dùng luôn
        if (parentPlantableSpot != null)
        {
            return parentPlantableSpot;
        }

        // Kiểm tra parent
        Transform parent = transform.parent;
        while (parent != null)
        {
            PlantableSpot plantableSpot = parent.GetComponent<PlantableSpot>();
            if (plantableSpot != null)
            {
                parentPlantableSpot = plantableSpot;
                return plantableSpot;
            }
            parent = parent.parent;
        }

        // Nếu không tìm thấy trong parent, tìm trong scene
        PlantableSpot[] allSpots = FindObjectsByType<PlantableSpot>(FindObjectsSortMode.None);
        foreach (PlantableSpot spot in allSpots)
        {
            // Kiểm tra xem cây này có phải là con của spot không
            if (spot.HasPlant() && spot.GetCurrentPlant() == gameObject)
            {
                parentPlantableSpot = spot;
                return spot;
            }
        }

        return null;
    }

    /// <summary>
    /// Thiết lập PlantableSpot parent (được gọi từ PlantableSpot khi spawn cây)
    /// </summary>
    public void SetPlantableSpot(PlantableSpot spot)
    {
        parentPlantableSpot = spot;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("[SeedHarvestable] Player entered harvest range. Press E to harvest seeds.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    /// <summary>
    /// Thiết lập PlantData từ code (dùng khi spawn cây)
    /// </summary>
    public void SetPlantData(PlantData data)
    {
        plantData = data;
    }

    /// <summary>
    /// Reset trạng thái thu hoạch (dùng khi cây mới phát triển đến giai đoạn trưởng thành)
    /// </summary>
    public void ResetHarvestState()
    {
        hasBeenHarvested = false;
        this.enabled = true;
    }
}

