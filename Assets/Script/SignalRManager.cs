using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Script đơn giản để kết nối SignalR (cần cài package SignalR Client cho Unity)
// Tạm thời dùng WebSocket đơn giản hoặc HTTP polling

public class SignalRManager : MonoBehaviour
{
    public string serverURL = "http://localhost:5000";
    public TcpClientManager tcpManager;
    
    private int currentUserId = 0;
    private string currentUsername = "";
    
    // Events
    public System.Action<int, string, string> OnChatMessageReceived; // userId, username, message
    public System.Action<List<Dictionary<string, object>>> OnOnlineUsersUpdated;
    
    void Start()
    {
        if (tcpManager == null)
        {
            tcpManager = FindFirstObjectByType<TcpClientManager>();
        }
        
        // Đồng bộ serverURL từ TcpClientManager nếu có
        if (tcpManager != null && !string.IsNullOrEmpty(tcpManager.serverURL))
        {
            serverURL = tcpManager.serverURL;
            Debug.Log($"[SignalRManager] Đã đồng bộ serverURL từ TcpClientManager: {serverURL}");
        }
        
        // Lấy userId từ PlayerPrefs sau khi đăng nhập
        if (PlayerPrefs.HasKey("UserId"))
        {
            currentUserId = PlayerPrefs.GetInt("UserId");
            StartCoroutine(LoadUserInfo());
        }
    }
    
    IEnumerator LoadUserInfo()
    {
        // Lấy username từ database (hoặc lưu khi đăng nhập)
        // Tạm thời dùng email làm username
        yield return null;
    }
    
    // Gửi tin nhắn chat (dùng HTTP API tạm thời, sau này sẽ dùng SignalR)
    public void SendChatMessage(string message)
    {
        Debug.Log($"[SignalRManager] SendChatMessage called. Message: {message}, UserId: {currentUserId}, Username: {currentUsername}");
        
        if (currentUserId == 0)
        {
            Debug.LogWarning("[SignalRManager] Chưa đăng nhập! UserId = 0");
            // Thử lấy từ PlayerPrefs
            if (PlayerPrefs.HasKey("UserId"))
            {
                currentUserId = PlayerPrefs.GetInt("UserId");
                currentUsername = PlayerPrefs.GetString("Username", "User" + currentUserId);
                Debug.Log($"[SignalRManager] Đã lấy từ PlayerPrefs: UserId={currentUserId}, Username={currentUsername}");
            }
            else
            {
                Debug.LogError("[SignalRManager] Không tìm thấy UserId trong PlayerPrefs!");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(currentUsername))
        {
            currentUsername = PlayerPrefs.GetString("Username", "User" + currentUserId);
            Debug.Log($"[SignalRManager] Đã lấy username: {currentUsername}");
        }
        
        Debug.Log($"[SignalRManager] Bắt đầu gửi tin nhắn...");
        StartCoroutine(SendChatCoroutine(message));
    }
    
    IEnumerator SendChatCoroutine(string message)
    {
        string url = serverURL + "/api/chat/send";
        
        ChatRequestData data = new ChatRequestData
        {
            userId = currentUserId,
            username = currentUsername,
            message = message
        };
        
        string json = JsonUtility.ToJson(data);
        Debug.Log($"[SignalRManager] URL: {url}");
        Debug.Log($"[SignalRManager] JSON gửi đi: {json}");
        
        UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();
        
        Debug.Log($"[SignalRManager] Response Code: {request.responseCode}");
        Debug.Log($"[SignalRManager] Response: {request.downloadHandler.text}");
        
        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Chat message sent successfully!");
        }
        else
        {
            Debug.LogError($"❌ Lỗi gửi chat: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
        }
        
        request.Dispose();
    }
    
    // Lấy danh sách users online
    public void GetOnlineUsers()
    {
        StartCoroutine(GetOnlineUsersCoroutine());
    }
    
    IEnumerator GetOnlineUsersCoroutine()
    {
        string url = serverURL + "/api/chat/online-users";
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            // Allow insecure HTTP connections (for local development)
            request.certificateHandler = new BypassCertificateHandler();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log("Online users: " + response);
                
                try
                {
                    // Parse JSON đơn giản
                    var jsonResponse = JsonUtility.FromJson<OnlineUsersResponse>(response);
                    if (jsonResponse.status == "success" && jsonResponse.users != null)
                    {
                        var usersList = new List<Dictionary<string, object>>();
                        foreach (var user in jsonResponse.users)
                        {
                            var userDict = new Dictionary<string, object>
                            {
                                {"user_id", user.user_id},
                                {"username", user.username},
                                {"email", user.email}
                            };
                            usersList.Add(userDict);
                        }
                        OnOnlineUsersUpdated?.Invoke(usersList);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Lỗi parse online users: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Lỗi lấy online users: " + request.error);
            }
        }
    }
    
    // Set user info sau khi đăng nhập
    public void SetUserInfo(int userId, string username)
    {
        currentUserId = userId;
        currentUsername = username;
        PlayerPrefs.SetInt("UserId", userId);
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.Save();
        
        Debug.Log($"User info set: {userId} - {username}");
    }
}

[System.Serializable]
public class ChatRequestData
{
    public int userId;
    public string username;
    public string message;
}

[System.Serializable]
public class OnlineUser
{
    public int user_id;
    public string username;
    public string email;
}

[System.Serializable]
public class OnlineUsersResponse
{
    public string status;
    public OnlineUser[] users;
}

