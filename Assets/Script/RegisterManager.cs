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

    void Start()
    {
        // Kiểm tra các field có được gán chưa
        if (emailField == null)
        {
            Debug.LogWarning("RegisterUI: emailField chưa được gán!");
        }
        if (passwordField == null)
        {
            Debug.LogWarning("RegisterUI: passwordField chưa được gán!");
        }
        if (confirmField == null)
        {
            Debug.LogWarning("RegisterUI: confirmField chưa được gán!");
        }

        // Tìm TcpClientManager nếu chưa gán
        if (tcpManager == null)
        {
            tcpManager = FindFirstObjectByType<TcpClientManager>();
            if (tcpManager == null)
            {
                tcpManager = FindObjectOfType<TcpClientManager>();
            }
        }
        
        // Đăng ký lắng nghe kết quả đăng ký
        if (tcpManager != null)
        {
            tcpManager.OnRegisterResult += HandleRegisterResult;
            Debug.Log("RegisterUI: Đã kết nối với TcpClientManager");
        }
        else
        {
            Debug.LogWarning("RegisterUI: Không tìm thấy TcpClientManager trong scene!");
        }
    }

    void OnDestroy()
    {
        // Hủy đăng ký khi object bị destroy
        if (tcpManager != null)
        {
            tcpManager.OnRegisterResult -= HandleRegisterResult;
        }
    }

    public void OnRegisterButtonClick()
    {
        // Kiểm tra các field có tồn tại không
        if (emailField == null || passwordField == null || confirmField == null)
        {
            ShowMessage("❌ Lỗi: Các trường nhập liệu chưa được gán!");
            Debug.LogError("RegisterUI: Một hoặc nhiều input field bị null!");
            return;
        }

        // Trim email và password để tránh lỗi do whitespace
        string email = emailField.text?.Trim() ?? "";
        string password = passwordField.text ?? "";
        string confirm = confirmField.text ?? "";

        Debug.Log($"[RegisterUI] Email: '{email}', Password length: {password.Length}, Confirm length: {confirm.Length}");

        // Kiểm tra rỗng
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            ShowMessage("❌ Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        // Kiểm tra định dạng email
        if (!IsValidEmail(email))
        {
            ShowMessage("❌ Email không hợp lệ! Vui lòng nhập đúng định dạng email (ví dụ: user@gmail.com)");
            return;
        }

        // Kiểm tra độ dài password
        if (password.Length < 6)
        {
            ShowMessage("❌ Mật khẩu phải có ít nhất 6 ký tự!");
            return;
        }

        // Kiểm tra xác nhận mật khẩu
        if (password != confirm)
        {
            ShowMessage("❌ Mật khẩu xác nhận không khớp!");
            return;
        }

        // Nếu chưa gán tcpManager trong Unity, thì tự tìm
        if (tcpManager == null)
        {
            tcpManager = FindFirstObjectByType<TcpClientManager>();
            if (tcpManager == null)
            {
                tcpManager = FindObjectOfType<TcpClientManager>();
            }
        }

        if (tcpManager != null)
        {
            Debug.Log($"[RegisterUI] Đang gửi request đăng ký với email: {email}");
            ShowMessage("⏳ Đang gửi dữ liệu...");
            tcpManager.RegisterAccount(email, password);
        }
        else
        {
            ShowMessage("❌ Không tìm thấy TcpClientManager trong scene!");
            Debug.LogError("RegisterUI: Không tìm thấy TcpClientManager!");
        }
    }

    // Kiểm tra email hợp lệ
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Xử lý kết quả đăng ký
    private void HandleRegisterResult(string message, bool success)
    {
        if (success)
        {
            ShowMessage("✅ " + message);
            Debug.Log($"[RegisterUI] ✅ Đăng ký thành công: {message}");
            
            // Xóa các trường nhập liệu sau khi đăng ký thành công
            if (emailField != null) emailField.text = "";
            if (passwordField != null) passwordField.text = "";
            if (confirmField != null) confirmField.text = "";
        }
        else
        {
            ShowMessage("❌ " + message);
            Debug.LogError($"[RegisterUI] ❌ Đăng ký thất bại: {message}");
        }
    }

    private void ShowMessage(string msg)
    {
        Debug.Log(msg);
        if (messageText != null)
            messageText.text = msg;
    }
}
