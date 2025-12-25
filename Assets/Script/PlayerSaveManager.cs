using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Script tự động lưu vị trí player và tài nguyên
/// </summary>
public class PlayerSaveManager : MonoBehaviour
{
    [Header("Settings")]
    public string serverURL = "http://localhost:5000";
    public float savePositionInterval = 5f; // Lưu vị trí mỗi 5 giây
    public float saveResourcesInterval = 10f; // Lưu tài nguyên mỗi 10 giây
    
    private int currentUserId = 0;
    private Vector3 lastSavedPosition;
    private float lastPositionSaveTime = 0f;
    private float lastResourcesSaveTime = 0f;
    
    void Start()
    {
        // Đảm bảo GameObject active trước khi start coroutine
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("[PlayerSaveManager] GameObject inactive, waiting...");
            StartCoroutine(WaitForActiveAndStart());
            return;
        }
        
        // Lấy userId từ PlayerPrefs
        if (PlayerPrefs.HasKey("UserId"))
        {
            currentUserId = PlayerPrefs.GetInt("UserId");
            Debug.Log($"[PlayerSaveManager] UserId: {currentUserId}");
            
            // Load vị trí đã lưu
            StartCoroutine(LoadPlayerPosition());
        }
    }
    
    IEnumerator WaitForActiveAndStart()
    {
        while (!gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Lấy userId từ PlayerPrefs
        if (PlayerPrefs.HasKey("UserId"))
        {
            currentUserId = PlayerPrefs.GetInt("UserId");
            Debug.Log($"[PlayerSaveManager] UserId: {currentUserId}");
            
            // Load vị trí đã lưu
            StartCoroutine(LoadPlayerPosition());
        }
    }
    
    void Update()
    {
        if (currentUserId == 0) return;
        if (!gameObject.activeSelf) return;
        
        // Tìm Player để lấy vị trí
        GameObject player = GameObject.Find("Player");
        if (player == null || !player.activeSelf) return;
        
        // Lưu vị trí nếu đã di chuyển và đủ thời gian
        if (Time.time - lastPositionSaveTime >= savePositionInterval)
        {
            Vector3 currentPos = player.transform.position;
            if (Vector3.Distance(currentPos, lastSavedPosition) > 0.5f) // Chỉ lưu nếu di chuyển > 0.5m
            {
                SavePlayerPosition(currentPos.x, currentPos.y);
                lastSavedPosition = currentPos;
                lastPositionSaveTime = Time.time;
            }
        }
        
        // Lưu tài nguyên định kỳ
        if (Time.time - lastResourcesSaveTime >= saveResourcesInterval)
        {
            SaveResources();
            lastResourcesSaveTime = Time.time;
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentUserId > 0)
        {
            // Lưu ngay khi game bị pause (thoát game)
            SavePlayerPosition(transform.position.x, transform.position.y);
            SaveResources();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentUserId > 0)
        {
            // Lưu ngay khi mất focus (alt+tab, minimize)
            SavePlayerPosition(transform.position.x, transform.position.y);
            SaveResources();
        }
    }
    
    void OnDestroy()
    {
        if (currentUserId > 0)
        {
            // Lưu khi destroy (thoát game)
            SavePlayerPosition(transform.position.x, transform.position.y);
            SaveResources();
        }
    }
    
    /// <summary>
    /// Lưu vị trí player
    /// </summary>
    public void SavePlayerPosition(float x, float y)
    {
        if (currentUserId == 0) return;
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("[PlayerSaveManager] GameObject inactive, cannot save position");
            return;
        }
        StartCoroutine(SavePlayerPositionCoroutine(x, y));
    }
    
    IEnumerator SavePlayerPositionCoroutine(float x, float y)
    {
        string url = serverURL + "/api/farm/position/save";
        
        PlayerPositionData data = new PlayerPositionData
        {
            userId = currentUserId,
            positionX = x,
            positionY = y
        };
        
        string json = JsonUtility.ToJson(data);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();
        
        Debug.Log($"[PlayerSaveManager] SavePosition - URL: {url}");
        Debug.Log($"[PlayerSaveManager] SavePosition - Response Code: {request.responseCode}");
        Debug.Log($"[PlayerSaveManager] SavePosition - Response: {request.downloadHandler.text}");
        
        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            Debug.Log($"✅ Đã lưu vị trí: ({x}, {y})");
        }
        else
        {
            Debug.LogError($"❌ Lỗi lưu vị trí: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Load vị trí đã lưu
    /// </summary>
    IEnumerator LoadPlayerPosition()
    {
        if (currentUserId == 0) yield break;
        
        string url = serverURL + $"/api/farm/position/{currentUserId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Allow insecure HTTP connections (for local development)
            request.certificateHandler = new BypassCertificateHandler();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"[PlayerSaveManager] Load position: {response}");
                
                try
                {
                    var wrapper = JsonUtility.FromJson<PlayerPositionResponse>(response);
                    if (wrapper.status == "success" && wrapper.position != null)
                    {
                        float x = wrapper.position.position_x;
                        float y = wrapper.position.position_y;
                        
                        // Tìm Player và set vị trí
                        GameObject player = GameObject.Find("Player");
                        if (player != null && player.activeSelf)
                        {
                            player.transform.position = new Vector3(x, y, player.transform.position.z);
                            lastSavedPosition = player.transform.position;
                            Debug.Log($"✅ Đã load vị trí: ({x}, {y})");
                        }
                        else
                        {
                            lastSavedPosition = new Vector3(x, y, 0);
                            Debug.Log($"✅ Đã load vị trí (Player chưa có): ({x}, {y})");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi parse position: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Lưu tài nguyên (coins, gems)
    /// </summary>
    public void SaveResources()
    {
        if (currentUserId == 0) return;
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("[PlayerSaveManager] GameObject inactive, cannot save resources");
            return;
        }
        
        // Lấy coins và gems từ FarmController hoặc InventoryManager
        int coins = 1000; // TODO: Lấy từ game
        int gems = 50; // TODO: Lấy từ game
        
        StartCoroutine(SaveResourcesCoroutine(coins, gems));
    }
    
    IEnumerator SaveResourcesCoroutine(int coins, int gems)
    {
        string url = serverURL + "/api/farm/resources/update";
        
        ResourcesData data = new ResourcesData
        {
            userId = currentUserId,
            coins = coins,
            gems = gems
        };
        
        string json = JsonUtility.ToJson(data);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();
        
        Debug.Log($"[PlayerSaveManager] SaveResources - URL: {url}");
        Debug.Log($"[PlayerSaveManager] SaveResources - Response Code: {request.responseCode}");
        Debug.Log($"[PlayerSaveManager] SaveResources - Response: {request.downloadHandler.text}");
        
        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            Debug.Log($"✅ Đã lưu tài nguyên: coins={coins}, gems={gems}");
        }
        else
        {
            Debug.LogError($"❌ Lỗi lưu tài nguyên: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
        }
        
        request.Dispose();
    }
}

[System.Serializable]
public class PlayerPositionData
{
    public int userId;
    public float positionX;
    public float positionY;
}

[System.Serializable]
public class ResourcesData
{
    public int userId;
    public int coins;
    public int gems;
}

[System.Serializable]
public class PlayerPosition
{
    public float position_x;
    public float position_y;
    public string last_saved_at;
}

[System.Serializable]
public class PlayerPositionResponse
{
    public string status;
    public PlayerPosition position;
}

