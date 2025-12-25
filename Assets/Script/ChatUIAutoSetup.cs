using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Script tự động tạo ChatPanel và các UI elements nếu chưa có
/// Chỉ cần attach script này vào ChatManager, nó sẽ tự động setup
/// </summary>
[RequireComponent(typeof(ChatUI))]
public class ChatUIAutoSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool autoSetupOnStart = true;
    public bool hideChatPanelOnStart = true;
    
    private ChatUI chatUI;
    private Canvas canvas;
    
    void Awake()
    {
        Debug.Log("[ChatUIAutoSetup] Awake() được gọi");
        // Chạy trong Awake để đảm bảo setup trước ChatUI.Start()
        if (!autoSetupOnStart)
        {
            Debug.Log("[ChatUIAutoSetup] Auto setup bị tắt, bỏ qua");
            return;
        }
        
        chatUI = GetComponent<ChatUI>();
        if (chatUI == null)
        {
            Debug.LogError("ChatUIAutoSetup: Không tìm thấy ChatUI component!");
            return;
        }
        
        Debug.Log("[ChatUIAutoSetup] Đã tìm thấy ChatUI component");
        
        // Tìm hoặc tạo Canvas TRƯỚC khi setup UI
        FindOrCreateCanvas();
        
        SetupChatUI();
        
        Debug.Log("[ChatUIAutoSetup] Awake() hoàn thành");
    }
    
    void Start()
    {
        // Không cần làm gì, đã setup trong Awake
    }
    
    void FindOrCreateCanvas()
    {
        // Tìm hoặc tạo Canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("🔨 Tạo Canvas mới...");
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("✅ Đã tạo Canvas");
        }
        else
        {
            Debug.Log($"✅ Tìm thấy Canvas: {canvas.name}");
        }
        
        // Đảm bảo có EventSystem (cần cho InputField)
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.Log("🔨 Tạo EventSystem...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ Đã tạo EventSystem");
        }
        
        // Đảm bảo canvas không null
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas vẫn null sau khi tìm/tạo!");
        }
    }
    
    void SetupChatUI()
    {
        // 1. Tạo ChatPanel nếu chưa có
        if (chatUI.chatPanel == null)
        {
            chatUI.chatPanel = CreateChatPanel();
        }
        
        // 2. Tạo ScrollView nếu chưa có
        if (chatUI.chatScrollRect == null)
        {
            chatUI.chatScrollRect = CreateScrollView();
        }
        
        // 3. Tạo ChatContent nếu chưa có
        if (chatUI.chatContent == null)
        {
            chatUI.chatContent = CreateChatContent();
        }
        
        // 4. Tạo InputField nếu chưa có
        if (chatUI.chatInputField == null)
        {
            chatUI.chatInputField = CreateInputField();
        }
        
        // 5. Tạo SendButton nếu chưa có
        if (chatUI.sendButton == null)
        {
            chatUI.sendButton = CreateSendButton();
        }
        
        // Đảm bảo SendButton có onClick listener - SETUP LẠI SAU KHI TẤT CẢ ĐÃ TẠO XONG
        StartCoroutine(SetupSendButtonDelayed());
        
        // 6. Tạo ToggleButton (luôn tạo mới vì nó ở Canvas, không phải trong ChatPanel)
        // Kiểm tra xem đã có ToggleChatButton trong scene chưa
        Button existingToggle = FindFirstObjectByType<Button>();
        if (existingToggle != null && existingToggle.name == "ToggleChatButton")
        {
            chatUI.toggleChatButton = existingToggle;
            Debug.Log("✅ Tìm thấy ToggleChatButton đã có sẵn");
        }
        else
        {
            chatUI.toggleChatButton = CreateToggleButton();
            Debug.Log("✅ Đã tạo ToggleChatButton mới");
        }
        
        // 7. Tạo OnlineUsersText nếu chưa có
        if (chatUI.onlineUsersText == null)
        {
            chatUI.onlineUsersText = CreateOnlineUsersText();
        }
        
        // Ẩn ChatPanel mặc định
        if (hideChatPanelOnStart && chatUI.chatPanel != null)
        {
            chatUI.chatPanel.SetActive(false);
        }
        
        // Đảm bảo InputField có thể nhập được
        if (chatUI.chatInputField != null)
        {
            chatUI.chatInputField.interactable = true;
            chatUI.chatInputField.readOnly = false;
        }
        
        // Đảm bảo ToggleButton hiển thị và hoạt động
        if (chatUI.toggleChatButton != null)
        {
            chatUI.toggleChatButton.gameObject.SetActive(true);
            chatUI.toggleChatButton.interactable = true;
            
            // Setup onClick cho ToggleButton - dùng method riêng
            chatUI.toggleChatButton.onClick.RemoveAllListeners();
            chatUI.toggleChatButton.onClick.AddListener(OnToggleChatButtonClick);
            
            Debug.Log($"✅ ToggleChatButton đã được setup: Active={chatUI.toggleChatButton.gameObject.activeSelf}, Interactable={chatUI.toggleChatButton.interactable}");
            Debug.Log($"ToggleChatButton onClick listeners: {chatUI.toggleChatButton.onClick.GetPersistentEventCount()}");
            Debug.Log($"ToggleChatButton parent: {chatUI.toggleChatButton.transform.parent?.name}");
        }
        else
        {
            Debug.LogError("❌ ToggleChatButton vẫn null sau khi setup!");
        }
        
        Debug.Log("✅ ChatUI đã được tự động setup!");
        Debug.Log($"ChatPanel: {chatUI.chatPanel != null}, InputField: {chatUI.chatInputField != null}, SendButton: {chatUI.sendButton != null}, ToggleButton: {chatUI.toggleChatButton != null}");
        
        // Đảm bảo SendButton có thể click được
        if (chatUI.sendButton != null)
        {
            chatUI.sendButton.interactable = true;
            Debug.Log($"SendButton interactable: {chatUI.sendButton.interactable}");
        }
    }
    
    IEnumerator SetupSendButtonDelayed()
    {
        // Đợi 2 frame để đảm bảo tất cả đã được tạo
        yield return null;
        yield return null;
        
        if (chatUI.sendButton != null)
        {
            // Xóa listener cũ nếu có
            chatUI.sendButton.onClick.RemoveAllListeners();
            
            // Thêm listener mới - dùng lambda để test
            chatUI.sendButton.onClick.AddListener(() => {
                Debug.Log("🔴🔴🔴 LAMBDA CLICKED! 🔴🔴🔴");
                OnSendButtonClickDirect();
            });
            
            // Cũng thêm trực tiếp method
            chatUI.sendButton.onClick.AddListener(OnSendButtonClickDirect);
            
            chatUI.sendButton.interactable = true;
            chatUI.sendButton.enabled = true;
            
            // Đảm bảo button active
            chatUI.sendButton.gameObject.SetActive(true);
            
            // Đảm bảo parent active
            if (chatUI.sendButton.transform.parent != null)
            {
                chatUI.sendButton.transform.parent.gameObject.SetActive(true);
            }
            
            Debug.Log("✅✅✅ SendButton onClick đã được setup lại trong ChatUIAutoSetup (delayed)");
            Debug.Log($"SendButton GameObject active: {chatUI.sendButton.gameObject.activeSelf}");
            Debug.Log($"SendButton interactable: {chatUI.sendButton.interactable}");
            Debug.Log($"SendButton enabled: {chatUI.sendButton.enabled}");
            Debug.Log($"SendButton parent active: {(chatUI.sendButton.transform.parent != null ? chatUI.sendButton.transform.parent.gameObject.activeSelf : "null")}");
            Debug.Log($"SendButton onClick listeners: {chatUI.sendButton.onClick.GetPersistentEventCount()}");
            
            // Kiểm tra EventSystem
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            Debug.Log($"EventSystem exists: {eventSystem != null}");
        }
        else
        {
            Debug.LogError("❌ SendButton vẫn null sau khi delay!");
        }
    }
    
    // Method để gọi khi click SendButton
    public void OnSendButtonClickDirect()
    {
        Debug.Log("🔵🔵🔵 [ChatUIAutoSetup] OnSendButtonClickDirect được gọi! 🔵🔵🔵");
        Debug.Log($"chatUI != null: {chatUI != null}");
        
        if (chatUI != null)
        {
            Debug.Log("Gọi chatUI.SendChatMessage()...");
            chatUI.SendChatMessage();
        }
        else
        {
            Debug.LogError("❌ chatUI is null!");
        }
    }
    
    // Method để gọi khi click ToggleChatButton
    public void OnToggleChatButtonClick()
    {
        Debug.Log("🔵 [ChatUIAutoSetup] OnToggleChatButtonClick được gọi!");
        Debug.Log($"chatUI != null: {chatUI != null}");
        Debug.Log($"chatPanel != null: {(chatUI != null && chatUI.chatPanel != null)}");
        
        if (chatUI != null)
        {
            Debug.Log("Gọi chatUI.ToggleChatPanel()...");
            chatUI.ToggleChatPanel();
        }
        else
        {
            Debug.LogError("❌ chatUI is null!");
        }
    }
    
    GameObject CreateChatPanel()
    {
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas is null! Không thể tạo ChatPanel!");
            FindOrCreateCanvas(); // Thử tìm lại
            if (canvas == null)
            {
                Debug.LogError("❌ Vẫn không tìm thấy Canvas!");
                return null;
            }
        }
        
        Debug.Log($"🔨 Tạo ChatPanel, Canvas: {canvas.name}");
        GameObject panel = new GameObject("ChatPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        // RectTransform
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(400, 300);
        
        // Image (background)
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        
        return panel;
    }
    
    ScrollRect CreateScrollView()
    {
        GameObject scrollViewObj = new GameObject("ChatScrollView");
        scrollViewObj.transform.SetParent(chatUI.chatPanel.transform, false);
        
        RectTransform rect = scrollViewObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(10, 50);
        rect.offsetMax = new Vector2(-10, -50);
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(0, 0, 0, 0.5f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scrollRect.viewport = viewportRect;
        
        return scrollRect;
    }
    
    Transform CreateChatContent()
    {
        GameObject contentObj = new GameObject("ChatContent");
        contentObj.transform.SetParent(chatUI.chatScrollRect.viewport, false);
        
        RectTransform rect = contentObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5;
        layout.padding = new RectOffset(5, 5, 5, 5);
        
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        chatUI.chatScrollRect.content = rect;
        
        return rect;
    }
    
    TMP_InputField CreateInputField()
    {
        GameObject inputObj = new GameObject("ChatInputField");
        inputObj.transform.SetParent(chatUI.chatPanel.transform, false);
        
        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(300, 30);
        
        Image img = inputObj.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.2f);
        
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        
        // Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(5, 5);
        textAreaRect.offsetMax = new Vector2(-5, -5);
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 14;
        text.color = Color.white;
        
        inputField.textViewport = textAreaRect;
        inputField.textComponent = text;
        
        // Tạo Placeholder riêng
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Nhập tin nhắn...";
        placeholderText.fontSize = 14;
        placeholderText.color = new Color(1, 1, 1, 0.5f);
        placeholderText.fontStyle = FontStyles.Italic;
        
        inputField.placeholder = placeholderText;
        inputField.interactable = true; // Đảm bảo có thể nhập
        inputField.readOnly = false;
        
        return inputField;
    }
    
    Button CreateSendButton()
    {
        GameObject buttonObj = new GameObject("SendButton");
        buttonObj.transform.SetParent(chatUI.chatPanel.transform, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(320, 10);
        rect.sizeDelta = new Vector2(70, 30);
        
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = img;
        button.interactable = true;
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Gửi";
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false; // Không block click
        
        // Setup onClick ngay tại đây
        button.onClick.AddListener(OnSendButtonClickDirect);
        
        // Đảm bảo button có thể nhận click
        buttonObj.SetActive(true);
        button.interactable = true;
        button.enabled = true;
        
        // Đảm bảo Image có thể nhận raycast
        img.raycastTarget = true;
        
        Debug.Log("✅ SendButton onClick đã được setup trong CreateSendButton");
        Debug.Log($"Button created - Active: {buttonObj.activeSelf}, Interactable: {button.interactable}, Enabled: {button.enabled}");
        Debug.Log($"Image raycastTarget: {img.raycastTarget}");
        
        return button;
    }
    
    Button CreateToggleButton()
    {
        // Kiểm tra xem đã có ToggleChatButton chưa
        Transform existing = canvas.transform.Find("ToggleChatButton");
        if (existing != null)
        {
            Button existingButton = existing.GetComponent<Button>();
            if (existingButton != null)
            {
                Debug.Log("✅ Tìm thấy ToggleChatButton đã có, sử dụng lại");
                existingButton.gameObject.SetActive(true);
                existingButton.interactable = true;
                return existingButton;
            }
        }
        
        Debug.Log("🔨 Bắt đầu tạo ToggleChatButton...");
        Debug.Log($"Canvas: {canvas != null}, Canvas name: {(canvas != null ? canvas.name : "null")}");
        
        GameObject buttonObj = new GameObject("ToggleChatButton");
        buttonObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-80, 50); // Nhích vào trong hơn (từ -50 thành -80)
        rect.sizeDelta = new Vector2(100, 40);
        
        Debug.Log($"Button RectTransform: anchorMin={rect.anchorMin}, anchorMax={rect.anchorMax}, pos={rect.anchoredPosition}, size={rect.sizeDelta}");
        
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = img;
        button.interactable = true;
        
        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Chat";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false; // Không block click
        
        // Setup onClick ngay tại đây
        button.onClick.AddListener(OnToggleChatButtonClick);
        
        // Đảm bảo button hiển thị
        buttonObj.SetActive(true);
        
        // Đảm bảo Canvas hiển thị
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
        
        Debug.Log($"✅ Đã tạo ToggleChatButton - Active: {buttonObj.activeSelf}, Interactable: {button.interactable}");
        Debug.Log($"Button onClick listeners: {button.onClick.GetPersistentEventCount()}");
        
        return button;
    }
    
    TextMeshProUGUI CreateOnlineUsersText()
    {
        GameObject textObj = new GameObject("OnlineUsersText");
        textObj.transform.SetParent(chatUI.chatPanel.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(0, 20);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Online: ";
        text.fontSize = 12;
        text.color = Color.yellow;
        
        return text;
    }
}

