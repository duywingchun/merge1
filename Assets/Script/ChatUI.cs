using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChatUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public TMP_InputField chatInputField;
    public Button sendButton;
    public ScrollRect chatScrollRect;
    public Transform chatContent; // Content của ScrollView
    public GameObject chatMessagePrefab; // Prefab cho mỗi tin nhắn (có thể tạo đơn giản)
    public TextMeshProUGUI onlineUsersText;
    public Button toggleChatButton; // Nút để mở/đóng chat
    public Button clearChatButton; // Nút để xóa tất cả tin nhắn
    
    [Header("Settings")]
    public float refreshInterval = 2f; // Refresh chat mỗi 2 giây
    public KeyCode clearChatKey = KeyCode.Delete; // Phím để xóa chat (mặc định: Delete)
    
    // Static flag để các script khác biết khi đang chat (để disable movement/actions)
    public static bool IsChatting { get; private set; } = false;
    
    private SignalRManager signalRManager;
    private int currentUserId = 0;
    private string currentUsername = "";
    private int lastMessageId = 0;
    private HashSet<int> displayedMessageIds = new HashSet<int>(); // Để tránh hiển thị trùng
    
    void Awake()
    {
        // Không dùng DontDestroyOnLoad - ChatPanel sẽ copy vào từng scene
    }
    
    void Start()
    {
        // Tìm SignalRManager
        signalRManager = FindFirstObjectByType<SignalRManager>();
        if (signalRManager == null)
        {
            signalRManager = gameObject.AddComponent<SignalRManager>();
        }
        
        // Đảm bảo SignalRManager không bị destroy khi chuyển scene
        if (signalRManager != null)
        {
            DontDestroyOnLoad(signalRManager.gameObject);
        }
        
        // Nếu không có UI được gán, đợi một chút (ChatUIAutoSetup có thể đang tạo)
        if (chatPanel == null)
        {
            Debug.LogWarning("ChatUI: ChatPanel chưa được gán! Đợi ChatUIAutoSetup tạo...");
            StartCoroutine(WaitForUISetup());
            return;
        }
        
        // Setup events
        SetupUIEvents();
    }
    
    IEnumerator WaitForUISetup()
    {
        // Đợi tối đa 2 giây để ChatUIAutoSetup tạo UI
        float elapsed = 0f;
        while (chatPanel == null && elapsed < 2f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (chatPanel != null)
        {
            Debug.Log("✅ ChatPanel đã được tạo, setup lại...");
            // Setup lại events
            SetupUIEvents();
        }
        else
        {
            Debug.LogError("❌ ChatPanel vẫn null sau 2 giây!");
        }
    }
    
    void SetupUIEvents()
    {
        // Setup events
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendButtonClick);
            Debug.Log("✅ SendButton onClick đã được setup");
        }
        else
        {
            Debug.LogError("❌ SendButton is null! Không thể setup onClick!");
        }
        
        if (chatInputField != null)
        {
            chatInputField.onSubmit.RemoveAllListeners();
            chatInputField.onSubmit.AddListener(OnChatInputSubmit);
            Debug.Log("✅ ChatInputField onSubmit đã được setup");
        }
        else
        {
            Debug.LogError("❌ ChatInputField is null! Không thể setup onSubmit!");
        }
        
        // Setup toggle button
        if (toggleChatButton != null)
        {
            toggleChatButton.onClick.RemoveAllListeners();
            toggleChatButton.onClick.AddListener(ToggleChatPanel);
        }
        
        // Setup clear chat button
        if (clearChatButton != null)
        {
            clearChatButton.onClick.RemoveAllListeners();
            clearChatButton.onClick.AddListener(ClearAllChatMessages);
        }
        
        // Setup input field focus events để biết khi đang chat
        if (chatInputField != null)
        {
            chatInputField.onSelect.AddListener(OnChatInputSelected);
            chatInputField.onDeselect.AddListener(OnChatInputDeselected);
        }
        
        // Ẩn chat panel mặc định
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        
        // Lấy user info từ PlayerPrefs - thử nhiều lần vì có thể chưa kịp lưu
        StartCoroutine(LoadUserIdFromPlayerPrefs());
        
        // Đăng ký event (chỉ khi signalRManager không null)
        if (signalRManager != null)
        {
            signalRManager.OnOnlineUsersUpdated += UpdateOnlineUsers;
            
            // Bắt đầu refresh chat
            StartCoroutine(RefreshChatCoroutine());
            StartCoroutine(RefreshOnlineUsersCoroutine());
        }
        else
        {
            Debug.LogWarning("ChatUI: SignalRManager không tìm thấy, chat sẽ không hoạt động!");
        }
    }
    
    IEnumerator LoadUserIdFromPlayerPrefs()
    {
        // Đợi tối đa 3 giây để PlayerPrefs được lưu
        float elapsed = 0f;
        while (!PlayerPrefs.HasKey("UserId") && elapsed < 3f)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (PlayerPrefs.HasKey("UserId"))
        {
            currentUserId = PlayerPrefs.GetInt("UserId");
            currentUsername = PlayerPrefs.GetString("Username", "User" + currentUserId);
            Debug.Log($"[ChatUI] ✅ Đã load UserId từ PlayerPrefs: {currentUserId}, Username: {currentUsername}");
            if (signalRManager != null)
            {
                signalRManager.SetUserInfo(currentUserId, currentUsername);
            }
        }
        else
        {
            Debug.LogWarning("[ChatUI] Không tìm thấy UserId trong PlayerPrefs sau 3 giây");
            Debug.LogWarning("[ChatUI] Tất cả keys trong PlayerPrefs: " + string.Join(", ", GetAllPlayerPrefsKeys()));
        }
    }
    
    // Helper method để lấy tất cả keys từ PlayerPrefs (chỉ để debug)
    private string[] GetAllPlayerPrefsKeys()
    {
        // Unity không có method để lấy tất cả keys, nên thử một số keys phổ biến
        var keys = new System.Collections.Generic.List<string>();
        if (PlayerPrefs.HasKey("UserId")) keys.Add("UserId");
        if (PlayerPrefs.HasKey("Username")) keys.Add("Username");
        if (PlayerPrefs.HasKey("Email")) keys.Add("Email");
        return keys.ToArray();
    }
    
    void Update()
    {
        // Kiểm tra phím tắt để xóa chat (Delete hoặc Ctrl+L)
        if (Input.GetKeyDown(clearChatKey) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L)))
        {
            // Chỉ xóa khi chat panel đang mở
            if (chatPanel != null && chatPanel.activeSelf)
            {
                ClearAllChatMessages();
            }
        }
    }
    
    void OnDestroy()
    {
        // Reset IsChatting flag khi destroy
        IsChatting = false;
        
        if (signalRManager != null)
        {
            signalRManager.OnOnlineUsersUpdated -= UpdateOnlineUsers;
        }
        
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
        }
        
        if (toggleChatButton != null)
        {
            toggleChatButton.onClick.RemoveAllListeners();
        }
        
        if (clearChatButton != null)
        {
            clearChatButton.onClick.RemoveAllListeners();
        }
        
        if (chatInputField != null)
        {
            chatInputField.onSelect.RemoveAllListeners();
            chatInputField.onDeselect.RemoveAllListeners();
        }
    }
    
    // Toggle chat panel
    public void ToggleChatPanel()
    {
        Debug.Log($"🔵 ToggleChatPanel called. chatPanel != null: {chatPanel != null}");
        
        if (chatPanel != null)
        {
            bool newState = !chatPanel.activeSelf;
            chatPanel.SetActive(newState);
            Debug.Log($"🔵 ChatPanel set to: {newState}");
            
            // Nếu mở panel, focus vào input field
            if (newState && chatInputField != null)
            {
                chatInputField.ActivateInputField();
            }
            else if (!newState)
            {
                // Nếu đóng panel, reset IsChatting flag
                IsChatting = false;
            }
        }
        else
        {
            Debug.LogError("❌ ChatPanel is null! Không thể toggle!");
        }
    }
    
    // Khi input field được chọn (focus)
    void OnChatInputSelected(string text)
    {
        IsChatting = true;
        Debug.Log("[ChatUI] Input field selected - IsChatting = true");
    }
    
    // Khi input field mất focus
    void OnChatInputDeselected(string text)
    {
        IsChatting = false;
        Debug.Log("[ChatUI] Input field deselected - IsChatting = false");
    }
    
    // Xóa tất cả tin nhắn trong chat
    public void ClearAllChatMessages()
    {
        if (chatContent == null)
            return;
        
        // Xóa tất cả child objects (tin nhắn) trong UI
        for (int i = chatContent.childCount - 1; i >= 0; i--)
        {
            Destroy(chatContent.GetChild(i).gameObject);
        }
        
        // KHÔNG clear displayedMessageIds và lastMessageId
        // Để tránh hiển thị lại tin nhắn cũ từ server
        // Chỉ xóa UI, giữ lại tracking để chỉ hiển thị tin nhắn mới
        
        Debug.Log("[ChatUI] Đã xóa tất cả tin nhắn (UI only, sẽ không hiển thị lại tin nhắn cũ)");
    }
    
    // Gửi tin nhắn
    void OnSendButtonClick()
    {
        Debug.Log("[ChatUI] OnSendButtonClick được gọi!");
        SendChatMessage();
    }
    
    void OnChatInputSubmit(string text)
    {
        Debug.Log($"[ChatUI] OnChatInputSubmit được gọi với text: {text}");
        SendChatMessage();
    }
    
    // Public method để có thể gọi từ bên ngoài
    public void SendChatMessage()
    {
        Debug.Log("🔵🔵🔵 [ChatUI] SendChatMessage called! 🔵🔵🔵");
        Debug.Log($"[ChatUI] InputField: {chatInputField != null}, Text: {(chatInputField != null ? chatInputField.text : "null")}");
        
        // Test: Gọi trực tiếp từ đây
        if (chatInputField != null && !string.IsNullOrEmpty(chatInputField.text))
        {
            Debug.Log($"🔵 Test: InputField text = '{chatInputField.text}'");
        }
        
        if (chatInputField == null)
        {
            Debug.LogError("[ChatUI] ChatInputField is null!");
            return;
        }
        
        if (string.IsNullOrEmpty(chatInputField.text))
        {
            Debug.LogWarning("[ChatUI] InputField is empty!");
            return;
        }
        
        if (currentUserId == 0)
        {
            Debug.LogWarning("[ChatUI] Chưa đăng nhập! UserId = 0 - Thử lấy từ PlayerPrefs...");
            // Thử lấy từ PlayerPrefs
            if (PlayerPrefs.HasKey("UserId"))
            {
                currentUserId = PlayerPrefs.GetInt("UserId");
                currentUsername = PlayerPrefs.GetString("Username", "User" + currentUserId);
                Debug.Log($"[ChatUI] ✅ Đã lấy từ PlayerPrefs: UserId={currentUserId}, Username={currentUsername}");
            }
            else
            {
                Debug.LogError("[ChatUI] ❌ Không tìm thấy UserId trong PlayerPrefs!");
                Debug.LogError($"[ChatUI] Tất cả keys: {string.Join(", ", GetAllPlayerPrefsKeys())}");
                Debug.LogError("[ChatUI] Vui lòng đăng nhập lại!");
                return;
            }
        }
        
        if (signalRManager == null)
        {
            Debug.LogWarning("[ChatUI] SignalRManager không có! Thử tìm lại...");
            signalRManager = FindFirstObjectByType<SignalRManager>();
            if (signalRManager == null)
            {
                signalRManager = gameObject.AddComponent<SignalRManager>();
                Debug.Log("[ChatUI] Đã tạo SignalRManager mới");
            }
            else
            {
                Debug.Log("[ChatUI] Đã tìm thấy SignalRManager");
            }
        }
        
        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[ChatUI] Message is empty after trim!");
            return;
        }
        
        Debug.Log($"[ChatUI] Gửi tin nhắn: {message}");
        
        // Gửi tin nhắn
        signalRManager.SendChatMessage(message);
        
        // Clear input
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }
    
    // Refresh chat messages từ server
    IEnumerator RefreshChatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            
            if (currentUserId > 0)
            {
                StartCoroutine(LoadChatHistory());
            }
        }
    }
    
    // Refresh online users
    IEnumerator RefreshOnlineUsersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Refresh mỗi 5 giây
            
            if (currentUserId > 0 && signalRManager != null)
            {
                signalRManager.GetOnlineUsers();
            }
        }
    }
    
    // Load chat history từ server
    IEnumerator LoadChatHistory()
    {
        if (signalRManager == null)
            yield break;
            
        string url = signalRManager.serverURL + "/api/chat/history?limit=20";
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            // Allow insecure HTTP connections (for local development)
            request.certificateHandler = new BypassCertificateHandler();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ParseChatHistory(response);
            }
        }
    }
    
    // Parse và hiển thị chat history
    void ParseChatHistory(string jsonResponse)
    {
        try
        {
            Debug.Log($"[ChatUI] Raw JSON response: {jsonResponse}");
            
            // Parse JSON - Unity JsonUtility cần wrapper class
            var wrapper = JsonUtility.FromJson<ChatHistoryWrapper>(jsonResponse);
            
            if (wrapper.status == "success" && wrapper.messages != null)
            {
                Debug.Log($"[ChatUI] Parsed {wrapper.messages.Length} messages");
                
                // Chỉ hiển thị tin nhắn mới (chưa hiển thị)
                foreach (var msg in wrapper.messages)
                {
                    if (!displayedMessageIds.Contains(msg.message_id))
                    {
                        // Debug: Kiểm tra message_text
                        string msgText = msg.GetMessageText();
                        Debug.Log($"[ChatUI] Parsing message: message_id={msg.message_id}, sender_id={msg.sender_id}, username='{msg.username}', message_text='{msgText}'");
                        
                        // Kiểm tra nếu message_text null hoặc rỗng
                        string messageToDisplay = string.IsNullOrEmpty(msgText) ? "(empty message)" : msgText;
                        
                        DisplayChatMessage(msg.sender_id, msg.username, messageToDisplay);
                        displayedMessageIds.Add(msg.message_id);
                        
                        if (msg.message_id > lastMessageId)
                        {
                            lastMessageId = msg.message_id;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ChatUI] Status: {wrapper.status}, Messages: {(wrapper.messages == null ? "null" : "empty")}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Lỗi parse chat: " + ex.Message);
            Debug.LogError("Stack trace: " + ex.StackTrace);
            Debug.LogError("JSON: " + jsonResponse);
        }
    }
    
    // Hiển thị một tin nhắn
    void DisplayChatMessage(int senderId, string username, string message)
    {
        if (chatContent == null)
            return;
        
        // Tạo text object đơn giản
        GameObject msgObj = new GameObject("ChatMessage_" + senderId + "_" + Time.time);
        msgObj.transform.SetParent(chatContent, false);
        
        // Thêm RectTransform
        RectTransform rectTransform = msgObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.sizeDelta = new Vector2(0, 30); // Height
        rectTransform.anchoredPosition = new Vector2(10, -chatContent.childCount * 30);
        
        TextMeshProUGUI textComponent = msgObj.AddComponent<TextMeshProUGUI>();
        
        // Highlight tin nhắn của mình
        if (senderId == currentUserId)
        {
            textComponent.text = $"<color=#FFFF00>[Bạn]</color> {message}";
        }
        else
        {
            textComponent.text = $"<color=#00FF00>{username}</color>: {message}";
        }
        
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        
        // Auto scroll to bottom
        if (chatScrollRect != null)
        {
            StartCoroutine(ScrollToBottom());
        }
    }
    
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    // Update online users display
    public void UpdateOnlineUsers(List<Dictionary<string, object>> users)
    {
        if (onlineUsersText == null)
            return;
        
        string text = "Online: ";
        if (users != null && users.Count > 0)
        {
            foreach (var user in users)
            {
                text += user["username"] + " ";
            }
        }
        else
        {
            text += "None";
        }
        
        onlineUsersText.text = text;
    }
}

[System.Serializable]
public class ChatMessage
{
    public int message_id;
    public int sender_id;
    public string username;
    // Backend trả về messageText (không có dấu gạch dưới) để Unity JsonUtility parse được
    public string messageText;
    public string sent_at;
    
    // Property để lấy message text
    public string GetMessageText() => messageText ?? "";
}

[System.Serializable]
public class ChatHistoryWrapper
{
    public string status;
    public ChatMessage[] messages;
}

