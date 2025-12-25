using UnityEngine;
using System.Collections;

/// <summary>
/// Script quản lý quá trình phát triển của cây qua các giai đoạn
/// </summary>
public class PlantGrowth : MonoBehaviour
{
    [Header("Plant Data")]
    [SerializeField] private PlantData plantData;

    [Header("Growth Settings")]
    [SerializeField] private int currentStage = 0; // Giai đoạn hiện tại (0 = hạt, cuối = trưởng thành)
    [SerializeField] private bool autoStartGrowing = true; // Tự động bắt đầu phát triển khi start
    [SerializeField] private bool isGrowing = false;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Event khi cây lên giai đoạn mới
    public System.Action<int> OnStageChanged;
    // Event khi cây trưởng thành hoàn toàn
    public System.Action OnFullyGrown;

    private Coroutine growthCoroutine;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Tree";
        }
    }

    private void Start()
    {


        if (plantData != null && plantData.IsValid())
        {
            // Thiết lập sprite ban đầu
            UpdateSprite();

            if (autoStartGrowing)
            {
                StartGrowing();
            }
        }
        else
        {
            Debug.LogError($"[PlantGrowth] {gameObject.name}: PlantData is null or invalid!");
        }
    }

    /// <summary>
    /// Bắt đầu quá trình phát triển
    /// </summary>
    public void StartGrowing()
    {
        if (isGrowing)
        {
            Debug.LogWarning("[PlantGrowth] Plant is already growing!");
            return;
        }

        if (plantData == null || !plantData.IsValid())
        {
            Debug.LogError("[PlantGrowth] Cannot start growing: PlantData is null or invalid");
            return;
        }

        isGrowing = true;
        growthCoroutine = StartCoroutine(GrowthProcess());
    }

    /// <summary>
    /// Dừng quá trình phát triển
    /// </summary>
    public void StopGrowing()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }
        isGrowing = false;
    }

    /// <summary>
    /// Coroutine xử lý quá trình phát triển
    /// </summary>
    private IEnumerator GrowthProcess()
    {
        // Bắt đầu từ stage 0 (hạt), phát triển đến stage cuối
        int maxStage = plantData.growthStageSprites.Count - 1;

        while (currentStage < maxStage)
        {
            // Chờ thời gian của giai đoạn hiện tại
            float duration = plantData.stageDurations[currentStage];
            yield return new WaitForSeconds(duration);

            // Chuyển sang giai đoạn tiếp theo
            currentStage++;
            UpdateSprite();

            // Notify event
            OnStageChanged?.Invoke(currentStage);

            Debug.Log($"[PlantGrowth] {plantData.plantName} advanced to stage {currentStage}/{maxStage}");

            // Nếu đã đến giai đoạn cuối (trưởng thành)
            if (currentStage >= maxStage)
            {
                OnFullyGrown?.Invoke();
                Debug.Log($"[PlantGrowth] {plantData.plantName} is fully grown!");

                // Kích hoạt SeedHarvestable nếu có
                SeedHarvestable harvestable = GetComponent<SeedHarvestable>();
                if (harvestable != null)
                {
                    harvestable.ResetHarvestState();
                }
            }
        }

        isGrowing = false;
    }

    /// <summary>
    /// Cập nhật sprite theo giai đoạn hiện tại
    /// </summary>
    private void UpdateSprite()
    {
        if (plantData == null || spriteRenderer == null)
        {
            return;
        }

        if (currentStage >= 0 && currentStage < plantData.growthStageSprites.Count)
        {
            Sprite newSprite = plantData.growthStageSprites[currentStage];
            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
            }
        }
    }

    /// <summary>
    /// Thiết lập PlantData từ code
    /// </summary>
    public void SetPlantData(PlantData data)
    {
        plantData = data;
        
        // Reset về giai đoạn đầu
        currentStage = 0;
        
        if (plantData != null && plantData.IsValid())
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// Lấy giai đoạn hiện tại
    /// </summary>
    public int GetCurrentStage()
    {
        return currentStage;
    }

    /// <summary>
    /// Lấy số giai đoạn tối đa
    /// </summary>
    public int GetMaxStage()
    {
        if (plantData == null || plantData.growthStageSprites == null)
        {
            return 0;
        }
        return plantData.growthStageSprites.Count - 1;
    }

    /// <summary>
    /// Kiểm tra xem cây đã trưởng thành hoàn toàn chưa
    /// </summary>
    public bool IsFullyGrown()
    {
        return currentStage >= GetMaxStage();
    }

    /// <summary>
    /// Lấy tiến độ phát triển (0.0 - 1.0)
    /// </summary>
    public float GetGrowthProgress()
    {
        int maxStage = GetMaxStage();
        if (maxStage <= 0)
        {
            return 0f;
        }
        return (float)currentStage / maxStage;
    }

    /// <summary>
    /// Thiết lập giai đoạn trực tiếp (dùng cho save/load)
    /// </summary>
    public void SetStage(int stage)
    {
        int maxStage = GetMaxStage();
        currentStage = Mathf.Clamp(stage, 0, maxStage);
        UpdateSprite();
    }

    /// <summary>
    /// Lấy PlantData
    /// </summary>
    public PlantData GetPlantData()
    {
        return plantData;
    }

    private void OnDestroy()
    {
        StopGrowing();
    }
}

