using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Quản lý multiplayer - hiển thị players khác và sync vị trí
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    [Header("Settings")]
    public string serverURL = "http://localhost:5000";
    public float positionSyncInterval = 0.5f; // Gửi vị trí mỗi 0.5 giây
    public float otherPlayersUpdateInterval = 1f; // Cập nhật vị trí players khác mỗi 1 giây
    
    [Header("Other Player Prefab")]
    public GameObject otherPlayerPrefab; // Prefab để spawn players khác (có thể dùng character prefab)
    
    private int currentUserId = 0;
    private Transform localPlayer;
    private Dictionary<int, GameObject> otherPlayers = new Dictionary<int, GameObject>(); // userId -> GameObject
    private Dictionary<int, string> otherPlayerUsernames = new Dictionary<int, string>(); // userId -> username
    private Vector3 lastSentPosition;
    
    void Start()
    {
        // Lấy userId từ PlayerPrefs
        if (PlayerPrefs.HasKey("UserId"))
        {
            currentUserId = PlayerPrefs.GetInt("UserId");
            Debug.Log($"[MultiplayerManager] Current UserId: {currentUserId}");
        }
        
        // Tìm local player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            localPlayer = playerObj.transform;
            lastSentPosition = localPlayer.position;
        }
        else
        {
            Debug.LogWarning("[MultiplayerManager] Không tìm thấy Player với tag 'Player'");
        }
        
        // Đồng bộ serverURL từ SignalRManager hoặc TcpClientManager
        SignalRManager signalR = FindObjectOfType<SignalRManager>();
        if (signalR != null && !string.IsNullOrEmpty(signalR.serverURL))
        {
            serverURL = signalR.serverURL;
        }
        else
        {
            TcpClientManager tcp = FindObjectOfType<TcpClientManager>();
            if (tcp != null && !string.IsNullOrEmpty(tcp.serverURL))
            {
                serverURL = tcp.serverURL;
            }
        }
        
        // Bắt đầu sync vị trí
        StartCoroutine(SyncPositionCoroutine());
        StartCoroutine(UpdateOtherPlayersCoroutine());
    }
    
    // Gửi vị trí player lên server
    IEnumerator SyncPositionCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(positionSyncInterval);
            
            if (localPlayer != null && currentUserId > 0)
            {
                Vector3 currentPos = localPlayer.position;
                
                // Chỉ gửi nếu vị trí thay đổi đáng kể (tránh spam)
                if (Vector3.Distance(currentPos, lastSentPosition) > 0.1f)
                {
                    StartCoroutine(SendPositionToServer(currentPos.x, currentPos.y));
                    lastSentPosition = currentPos;
                }
            }
        }
    }
    
    // Gửi vị trí lên server qua HTTP API
    IEnumerator SendPositionToServer(float x, float y)
    {
        string url = serverURL + "/api/farm/position/save";
        
        PositionData data = new PositionData
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
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[MultiplayerManager] Lỗi gửi vị trí: {request.error}");
        }
        
        request.Dispose();
    }
    
    // Cập nhật vị trí players khác
    IEnumerator UpdateOtherPlayersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(otherPlayersUpdateInterval);
            
            if (currentUserId > 0)
            {
                StartCoroutine(LoadOtherPlayersPositions());
            }
        }
    }
    
    // Load vị trí của players khác từ server
    IEnumerator LoadOtherPlayersPositions()
    {
        // Lấy danh sách online users
        string url = serverURL + "/api/chat/online-users";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.certificateHandler = new BypassCertificateHandler();
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ParseOnlineUsers(response);
            }
        }
        
        // Load vị trí của từng player
        foreach (var userId in otherPlayers.Keys)
        {
            StartCoroutine(LoadPlayerPosition(userId));
        }
    }
    
    // Parse danh sách online users (dùng OnlineUsersResponse từ SignalRManager)
    void ParseOnlineUsers(string jsonResponse)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<OnlineUsersResponse>(jsonResponse);
            
            if (wrapper.status == "success" && wrapper.users != null)
            {
                HashSet<int> currentOnlineUserIds = new HashSet<int>();
                
                foreach (var user in wrapper.users)
                {
                    int userId = user.user_id;
                    
                    // Bỏ qua chính mình
                    if (userId == currentUserId)
                        continue;
                    
                    currentOnlineUserIds.Add(userId);
                    
                    // Lưu username
                    string username = user.username ?? $"User_{userId}";
                    if (!otherPlayerUsernames.ContainsKey(userId))
                    {
                        otherPlayerUsernames[userId] = username;
                    }
                    else
                    {
                        otherPlayerUsernames[userId] = username; // Update username nếu thay đổi
                    }
                    
                    // Spawn player nếu chưa có
                    if (!otherPlayers.ContainsKey(userId))
                    {
                        SpawnOtherPlayer(userId, username);
                    }
                }
                
                // Xóa players không còn online
                List<int> toRemove = new List<int>();
                foreach (var userId in otherPlayers.Keys)
                {
                    if (!currentOnlineUserIds.Contains(userId))
                    {
                        toRemove.Add(userId);
                    }
                }
                
                foreach (var userId in toRemove)
                {
                    RemoveOtherPlayer(userId);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MultiplayerManager] Lỗi parse online users: {ex.Message}");
        }
    }
    
    // Load vị trí của một player cụ thể
    IEnumerator LoadPlayerPosition(int userId)
    {
        string url = serverURL + $"/api/farm/position/{userId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.certificateHandler = new BypassCertificateHandler();
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ParsePlayerPosition(userId, response);
            }
        }
    }
    
    // Parse vị trí player
    void ParsePlayerPosition(int userId, string jsonResponse)
    {
        try
        {
            var response = JsonUtility.FromJson<PositionResponse>(jsonResponse);
            
            if (response.status == "success" && response.position != null)
            {
                float x = response.position.position_x;
                float y = response.position.position_y;
                
                UpdateOtherPlayerPosition(userId, x, y);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MultiplayerManager] Lỗi parse position: {ex.Message}");
        }
    }
    
    // Spawn other player
    void SpawnOtherPlayer(int userId, string username)
    {
        GameObject prefabToUse = otherPlayerPrefab;
        
        // Nếu chưa gán prefab, thử tìm character prefab từ CharacterSelect
        if (prefabToUse == null)
        {
            // Thử lấy từ CharacterSelect.selectedCharacter
            if (CharacterSelect.selectedCharacter != null)
            {
                prefabToUse = CharacterSelect.selectedCharacter;
            }
            else
            {
                Debug.LogWarning($"[MultiplayerManager] OtherPlayerPrefab chưa được gán! Không thể spawn player {username}");
                return;
            }
        }
        
        // Spawn tại vị trí (0, 0) tạm thời, sẽ update sau khi load vị trí
        GameObject otherPlayer = Instantiate(prefabToUse, Vector3.zero, Quaternion.identity);
        otherPlayer.name = $"OtherPlayer_{userId}_{username}";
        otherPlayer.tag = "OtherPlayer"; // Tag khác với "Player"
        
        // Disable Player script để không điều khiển được
        Player playerScript = otherPlayer.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }
        
        // Disable Rigidbody2D hoặc set kinematic để không bị ảnh hưởng bởi physics
        Rigidbody2D rb = otherPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        
        // Thêm username label
        AddUsernameLabel(otherPlayer, username);
        
        otherPlayers[userId] = otherPlayer;
        
        Debug.Log($"[MultiplayerManager] Đã spawn player: {userId} - {username}");
        
        // Load vị trí ngay
        StartCoroutine(LoadPlayerPosition(userId));
    }
    
    // Update vị trí other player
    void UpdateOtherPlayerPosition(int userId, float x, float y)
    {
        if (otherPlayers.ContainsKey(userId))
        {
            GameObject otherPlayer = otherPlayers[userId];
            if (otherPlayer != null)
            {
                // Smooth movement (lerp) để mượt mà hơn
                Vector3 targetPos = new Vector3(x, y, otherPlayer.transform.position.z);
                Vector3 currentPos = otherPlayer.transform.position;
                
                // Nếu khoảng cách quá xa, teleport ngay (tránh lag)
                if (Vector3.Distance(currentPos, targetPos) > 10f)
                {
                    otherPlayer.transform.position = targetPos;
                }
                else
                {
                    // Smooth interpolation
                    otherPlayer.transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * 5f);
                }
            }
        }
    }
    
    // Remove other player
    void RemoveOtherPlayer(int userId)
    {
        if (otherPlayers.ContainsKey(userId))
        {
            GameObject otherPlayer = otherPlayers[userId];
            if (otherPlayer != null)
            {
                Destroy(otherPlayer);
            }
            otherPlayers.Remove(userId);
            otherPlayerUsernames.Remove(userId);
            Debug.Log($"[MultiplayerManager] Đã xóa player: {userId}");
        }
    }
    
    // Thêm username label trên đầu player (đơn giản hóa để tránh lỗi)
    void AddUsernameLabel(GameObject player, string username)
    {
        try
        {
            // Tạo Canvas cho username
            GameObject canvasObj = new GameObject("UsernameCanvas");
            canvasObj.transform.SetParent(player.transform);
            canvasObj.transform.localPosition = new Vector3(0, 1.5f, 0); // Phía trên đầu player
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(2, 0.5f);
            
            // Tạo Text - dùng TextMeshProUGUI nếu có, nếu không thì dùng Text thường
            GameObject textObj = new GameObject("UsernameText");
            textObj.transform.SetParent(canvasObj.transform, false);
            
            // Thử dùng TextMeshProUGUI trước
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = username;
            text.fontSize = 24;
            text.color = Color.yellow;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MultiplayerManager] Không thể tạo username label: {ex.Message}");
            // Nếu không tạo được label, vẫn tiếp tục (không bắt buộc)
        }
    }
    
    void OnDestroy()
    {
        // Cleanup
        foreach (var player in otherPlayers.Values)
        {
            if (player != null)
            {
                Destroy(player);
            }
        }
        otherPlayers.Clear();
        otherPlayerUsernames.Clear();
    }
}

// Classes cho Position API (không trùng với SignalRManager)
[System.Serializable]
public class PositionData
{
    public int userId;
    public float positionX;
    public float positionY;
}

[System.Serializable]
public class PositionResponse
{
    public string status;
    public PositionInfo position;
}

[System.Serializable]
public class PositionInfo
{
    public float position_x;
    public float position_y;
    public string last_saved_at;
}

