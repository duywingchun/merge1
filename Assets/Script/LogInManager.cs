using TMPro;  // Nếu dùng TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public TcpClientManager tcpClientManager;

    void Start()
    {
        // Gắn sự kiện cho nút login
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    public void OnLoginClicked()
    {
        // Trim email và password để tránh lỗi do whitespace
        string email = emailInput.text?.Trim() ?? "";
        string password = passwordInput.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Vui lòng nhập đầy đủ email và mật khẩu!");
            return;
        }
        
        Debug.Log($"[LoginUI] Đang đăng nhập với email: '{email}', password length: {password.Length}");
        
        // Đăng ký event để đợi login hoàn thành
        if (tcpClientManager != null)
        {
            tcpClientManager.OnLoginResult += OnLoginComplete;
            tcpClientManager.LoginAccount(email, password);
        }
    }
    
    void OnLoginComplete(string message, int? userId)
    {
        // Hủy đăng ký event
        if (tcpClientManager != null)
        {
            tcpClientManager.OnLoginResult -= OnLoginComplete;
        }
        
        if (userId.HasValue)
        {
            Debug.Log($"✅ Login hoàn thành! UserId: {userId.Value}");
            Debug.Log($"✅ Kiểm tra PlayerPrefs: HasKey('UserId')={PlayerPrefs.HasKey("UserId")}");
            
            // Đợi một chút để đảm bảo PlayerPrefs được lưu
            StartCoroutine(WaitAndLoadScene());
        }
        else
        {
            Debug.LogError($"❌ Login thất bại: {message}");
        }
    }
    
    System.Collections.IEnumerator WaitAndLoadScene()
    {
        // Đợi 0.5 giây để đảm bảo PlayerPrefs được lưu
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"✅ Trước khi chuyển scene: HasKey('UserId')={PlayerPrefs.HasKey("UserId")}");
        SceneManager.LoadScene("selectcharacter");
    }
}
