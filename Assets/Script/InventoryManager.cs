using UnityEngine;
using System.Collections;
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
    
    private bool hasLoadedFromDatabase = false; // Flag để tránh load nhiều lần
    private bool isSaving = false; // Flag để tránh save nhiều lần cùng lúc

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
        
        // Tự động lưu vào database
        SaveInventoryToDatabase();
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
        
        // Tự động lưu vào database
        SaveInventoryToDatabase();
        
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

    /// <summary>
    /// Lưu inventory vào database
    /// </summary>
    private void SaveInventoryToDatabase()
    {
        if (!PlayerPrefs.HasKey("UserId"))
        {
            Debug.LogWarning("[InventoryManager] Chưa đăng nhập, không thể lưu inventory!");
            return;
        }
        
        if (isSaving)
        {
            Debug.Log("[InventoryManager] Đang lưu, bỏ qua request này");
            return;
        }

        int userId = PlayerPrefs.GetInt("UserId");
        Debug.Log($"[InventoryManager] 🔵 Bắt đầu lưu inventory, userId: {userId}");
        StartCoroutine(SaveInventoryCoroutine(userId));
    }

    /// <summary>
    /// Load inventory từ database
    /// </summary>
    public void LoadInventoryFromDatabase()
    {
        if (hasLoadedFromDatabase)
        {
            Debug.Log("[InventoryManager] Đã load từ database rồi, bỏ qua");
            return;
        }
        
        if (!PlayerPrefs.HasKey("UserId"))
        {
            Debug.LogWarning("[InventoryManager] Chưa đăng nhập, không thể load inventory!");
            return;
        }

        int userId = PlayerPrefs.GetInt("UserId");
        hasLoadedFromDatabase = true; // Đánh dấu đã load
        StartCoroutine(LoadInventoryCoroutine(userId));
    }

    IEnumerator SaveInventoryCoroutine(int userId)
    {
        isSaving = true; // Đánh dấu đang lưu
        
        string serverURL = "http://localhost:5000";
        string url = serverURL + "/api/farm/inventory/save";
        
        // Chuyển đổi Dictionary<SeedType, int> thành List<SeedData>
        List<SeedData> seedList = new List<SeedData>();
        foreach (var seed in seedsInventory)
        {
            if (seed.Value > 0) // Chỉ lưu những loại có số lượng > 0
            {
                seedList.Add(new SeedData
                {
                    seedType = seed.Key.ToString(),
                    quantity = seed.Value
                });
            }
        }
        
        InventoryRequestData data = new InventoryRequestData
        {
            userId = userId,
            seeds = seedList
        };
        
        string json = JsonUtility.ToJson(data);
        
        Debug.Log($"[InventoryManager] SaveInventory - URL: {url}");
        Debug.Log($"[InventoryManager] SaveInventory - JSON: {json}");
        
        UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();
        
        Debug.Log($"[InventoryManager] SaveInventory - Response Code: {request.responseCode}");
        Debug.Log($"[InventoryManager] SaveInventory - Response: {request.downloadHandler.text}");
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            Debug.Log("✅ Đã lưu inventory thành công!");
        }
        else
        {
            Debug.LogError($"❌ Lỗi lưu inventory: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
        }
        
        request.Dispose();
        isSaving = false; // Đánh dấu đã lưu xong
    }

    IEnumerator LoadInventoryCoroutine(int userId)
    {
        string serverURL = "http://localhost:5000";
        string url = serverURL + $"/api/farm/inventory/{userId}";
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            // Allow insecure HTTP connections (for local development)
            request.certificateHandler = new BypassCertificateHandler();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"[InventoryManager] Load inventory: {response}");
                
                try
                {
                    // Parse JSON thủ công vì Unity JsonUtility không hỗ trợ Dictionary
                    // Response format: {"status":"success","seeds":{"SeedType1":5,"SeedType2":10}}
                    Debug.Log($"[InventoryManager] Raw response: {response}");
                    
                    if (response.Contains("\"status\":\"success\""))
                    {
                        // Parse seeds từ JSON
                        // Format: "seeds":{"Apple":10,"Tomato":5}
                        int seedsStart = response.IndexOf("\"seeds\":{");
                        if (seedsStart > 0)
                        {
                            // Tìm vị trí kết thúc của seeds object (dấu } cuối cùng trước dấu } của response)
                            int seedsEnd = response.LastIndexOf("}");
                            if (seedsEnd > seedsStart)
                            {
                                string seedsJson = response.Substring(seedsStart + 8, seedsEnd - seedsStart - 7);
                                Debug.Log($"[InventoryManager] Seeds JSON: {seedsJson}");
                                
                                // Chỉ clear và load nếu có dữ liệu từ database
                                if (!string.IsNullOrEmpty(seedsJson) && seedsJson.Trim() != "{}")
                                {
                                    // Clear inventory hiện tại
                                    seedsInventory.Clear();
                                    
                                    // Parse từng cặp key:value
                                    // Xóa dấu {} nếu có
                                    seedsJson = seedsJson.Trim().TrimStart('{').TrimEnd('}');
                                    
                                    if (!string.IsNullOrEmpty(seedsJson))
                                    {
                                        string[] pairs = seedsJson.Split(',');
                                        foreach (string pair in pairs)
                                        {
                                            if (string.IsNullOrEmpty(pair.Trim())) continue;
                                            
                                            string[] keyValue = pair.Split(':');
                                            if (keyValue.Length == 2)
                                            {
                                                string seedTypeStr = keyValue[0].Trim().Trim('"');
                                                string quantityStr = keyValue[1].Trim();
                                                
                                                if (int.TryParse(quantityStr, out int quantity))
                                                {
                                                    if (System.Enum.TryParse<SeedType>(seedTypeStr, out SeedType seedType))
                                                    {
                                                        seedsInventory[seedType] = quantity;
                                                        OnSeedCountChanged?.Invoke(seedType, quantity);
                                                        Debug.Log($"[InventoryManager] Loaded: {seedType} = {quantity}");
                                                    }
                                                    else
                                                    {
                                                        Debug.LogWarning($"[InventoryManager] Unknown seed type: {seedTypeStr}");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.LogWarning($"[InventoryManager] Cannot parse quantity: {quantityStr}");
                                                }
                                            }
                                        }
                                        
                                        Debug.Log("✅ Đã load inventory từ database!");
                                    }
                                }
                                else
                                {
                                    Debug.Log("[InventoryManager] Database inventory rỗng, sẽ thêm hạt giống mặc định");
                                    // Thêm hạt giống mặc định nếu database rỗng
                                    StartCoroutine(AddDefaultSeedsIfEmpty());
                                }
                            }
                            else
                            {
                                Debug.LogWarning("[InventoryManager] Không tìm thấy vị trí kết thúc seeds object");
                            }
                        }
                        else
                        {
                            Debug.Log("[InventoryManager] Không tìm thấy seeds trong response, giữ nguyên inventory hiện tại");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[InventoryManager] Response không có status success: {response}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi parse inventory: {ex.Message}");
                    Debug.LogError($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogError($"❌ Lỗi load inventory: {request.error}");
            }
        }
    }

    void Start()
    {
        // Load inventory khi vào game
        if (PlayerPrefs.HasKey("UserId"))
        {
            // Đợi một chút để đảm bảo đã đăng nhập xong
            StartCoroutine(LoadInventoryAfterDelay());
        }
        else
        {
            // Nếu chưa đăng nhập, vẫn thêm hạt giống mặc định để test
            AddDefaultSeedsForTesting();
        }
    }
    
    IEnumerator LoadInventoryAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để đảm bảo đã đăng nhập
        
        LoadInventoryFromDatabase();
        
        // Nếu inventory rỗng, thêm hạt giống mặc định để test
        yield return new WaitForSeconds(0.5f); // Đợi load xong
        StartCoroutine(AddDefaultSeedsIfEmpty());
    }
    
    /// <summary>
    /// Thêm hạt giống mặc định nếu inventory rỗng (sau khi load từ database)
    /// </summary>
    IEnumerator AddDefaultSeedsIfEmpty()
    {
        yield return new WaitForSeconds(0.5f); // Đợi load xong
        
        bool isEmpty = true;
        foreach (var seed in seedsInventory)
        {
            if (seed.Value > 0)
            {
                isEmpty = false;
                break;
            }
        }
        
        if (isEmpty)
        {
            Debug.Log("[InventoryManager] Inventory rỗng, thêm hạt giống mặc định để test...");
            AddDefaultSeedsForTesting();
        }
    }
    
    /// <summary>
    /// Thêm hạt giống mặc định để test
    /// </summary>
    private void AddDefaultSeedsForTesting()
    {
        // Thêm hạt giống mặc định - mỗi loại 5 cái
        // Thêm trực tiếp vào dictionary thay vì dùng AddSeed để tránh trigger save nhiều lần
        foreach (SeedType seedType in System.Enum.GetValues(typeof(SeedType)))
        {
            if (!seedsInventory.ContainsKey(seedType) || seedsInventory[seedType] == 0)
            {
                seedsInventory[seedType] = 5;
                OnSeedCountChanged?.Invoke(seedType, 5);
            }
        }
        
        Debug.Log("[InventoryManager] ✅ Đã thêm hạt giống mặc định: mỗi loại 5 cái");
        
        // Lưu vào database sau khi thêm xong
        StartCoroutine(SaveInventoryAfterDelay());
    }
    
    IEnumerator SaveInventoryAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Đợi 1 giây để đảm bảo đã khởi tạo xong
        SaveInventoryToDatabase();
    }
}

[System.Serializable]
public class InventoryRequestData
{
    public int userId;
    public List<SeedData> seeds;
}

[System.Serializable]
public class SeedData
{
    public string seedType;
    public int quantity;
}


