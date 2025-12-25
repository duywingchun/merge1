using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_InputField confirmField;

    [Header("UI Message Text (tuỳ chọn)")]
    public Text messageText;

    [Header("Network Manager Reference")]
    public TcpClientManager tcpManager;

    public void OnRegisterButtonClick()
    {
        string email = emailField.text.Trim();
        string password = passwordField.text;
        string confirm = confirmField.text;

        // Kiểm tra rỗng
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            ShowMessage("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        // Kiểm tra xác nhận mật khẩu
        if (password != confirm)
        {
            ShowMessage("Mật khẩu xác nhận không khớp!");
            return;
        }

        // Nếu chưa gán tcpManager trong Unity, thì tự tìm
        if (tcpManager == null)
        {
            tcpManager = FindObjectOfType<TcpClientManager>();
        }

        if (tcpManager != null)
        {
            ShowMessage("⏳ Đang gửi dữ liệu...");
            tcpManager.RegisterAccount(email, password);
        }
        else
        {
            ShowMessage("❌ Không tìm thấy TcpClientManager trong scene!");
        }
    }

    private void ShowMessage(string msg)
    {
        Debug.Log(msg);
        if (messageText != null)
            messageText.text = msg;
    }
}
