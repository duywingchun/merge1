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
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Vui lòng nhập đầy đủ email và mật khẩu!");
            return;
        }
        else
        {
            SceneManager.LoadScene("selectcharacter");
        }
        tcpClientManager.LoginAccount(email, password);
    }
}
