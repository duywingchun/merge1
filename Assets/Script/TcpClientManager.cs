// Updated: HTTP API connection
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class RegisterData
{
    public string email;
    public string password;
}

[System.Serializable]
public class LoginData
{
    public string email;
    public string password;
}

[System.Serializable]
public class ApiResponse
{
    public string status;
    public string message;
    public int userId;
}

public class TcpClientManager : MonoBehaviour
{
    public string serverURL = "http://localhost:5000";
    
    // Event để UI có thể lắng nghe kết quả
    public System.Action<string, bool> OnRegisterResult; // message, success
    public System.Action<string, int?> OnLoginResult; // message, userId (null nếu lỗi)

    public void RegisterAccount(string email, string password)
    {
        StartCoroutine(RegisterCoroutine(email, password));
    }

    IEnumerator RegisterCoroutine(string email, string password)
    {
        // Đảm bảo email và password được trim
        email = email?.Trim() ?? "";
        password = password ?? "";
        
        string url = serverURL + "/api/auth/register";
        
        RegisterData data = new RegisterData
        {
            email = email,
            password = password
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[Register] URL: " + url);
        Debug.Log("[Register] Email: '" + email + "', Password length: " + password.Length);
        Debug.Log("[Register] JSON gửi đi: " + json);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();

        Debug.Log("[Register] Response Code: " + request.responseCode);
        Debug.Log("[Register] Response: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            string response = request.downloadHandler.text;
            Debug.Log("[Register] ✅ Server response: " + response);
            
            try
            {
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);
                if (apiResponse != null && apiResponse.status == "success")
                {
                    Debug.Log("✅ Đăng ký thành công! Message: " + apiResponse.message);
                    OnRegisterResult?.Invoke(apiResponse.message, true);
                }
                else
                {
                    string errorMsg = apiResponse != null ? apiResponse.message : "Lỗi không xác định";
                    Debug.LogWarning("❌ Đăng ký thất bại: " + errorMsg);
                    OnRegisterResult?.Invoke(errorMsg, false);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Lỗi parse JSON: " + ex.Message);
                Debug.LogError("Response text: " + response);
                OnRegisterResult?.Invoke("Lỗi xử lý phản hồi từ server", false);
            }
        }
        else
        {
            string errorMsg = "Lỗi kết nối server";
            string responseBody = request.downloadHandler?.text ?? "";
            
            if (!string.IsNullOrEmpty(responseBody))
            {
                try
                {
                    ApiResponse errorResponse = JsonUtility.FromJson<ApiResponse>(responseBody);
                    if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.message))
                    {
                        errorMsg = errorResponse.message;
                    }
                }
                catch 
                {
                    // Nếu không parse được JSON, dùng response body làm error message
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        errorMsg = responseBody;
                    }
                }
            }
            
            Debug.LogError("❌ [Register] Lỗi: " + (request.error ?? "Unknown error"));
            Debug.LogError("[Register] Response Code: " + request.responseCode);
            Debug.LogError("[Register] Response Body: " + responseBody);
            OnRegisterResult?.Invoke(errorMsg, false);
        }
        
        request.Dispose();
    }

    public void LoginAccount(string email, string password)
    {
        StartCoroutine(LoginCoroutine(email, password));
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        // Đảm bảo email và password được trim
        email = email?.Trim() ?? "";
        password = password ?? "";
        
        string url = serverURL + "/api/auth/login";
        
        LoginData data = new LoginData
        {
            email = email,
            password = password
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log("[Login] URL: " + url);
        Debug.Log("[Login] Email: '" + email + "', Password length: " + password.Length);
        Debug.Log("[Login] JSON gửi đi: " + json);
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Allow insecure HTTP connections (for local development)
        request.certificateHandler = new BypassCertificateHandler();
        
        yield return request.SendWebRequest();

        Debug.Log("[Login] Response Code: " + request.responseCode);
        Debug.Log("[Login] Response: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            string response = request.downloadHandler.text;
            Debug.Log("✅ Server response: " + response);
            
            try
            {
                ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);
                if (apiResponse.status == "success")
                {
                    Debug.Log("✅ Đăng nhập thành công! UserID: " + apiResponse.userId);
                    // Lưu userId và username để dùng sau
                    PlayerPrefs.SetInt("UserId", apiResponse.userId);
                    // Lấy username từ email (phần trước @)
                    string userEmail = data.email;
                    string username = userEmail.Split('@')[0];
                    PlayerPrefs.SetString("Username", username);
                    PlayerPrefs.Save();
                    
                    Debug.Log($"✅✅✅ Đã lưu vào PlayerPrefs: UserId={PlayerPrefs.GetInt("UserId")}, Username={PlayerPrefs.GetString("Username")}");
                    Debug.Log($"✅ Kiểm tra lại: HasKey('UserId')={PlayerPrefs.HasKey("UserId")}");
                    
                    // Set user info cho SignalRManager
                    SignalRManager signalR = FindFirstObjectByType<SignalRManager>();
                    if (signalR != null)
                    {
                        signalR.SetUserInfo(apiResponse.userId, username);
                    }
                    
                    OnLoginResult?.Invoke(apiResponse.message, apiResponse.userId);
                }
                else
                {
                    Debug.LogWarning("❌ " + apiResponse.message);
                    OnLoginResult?.Invoke(apiResponse.message, null);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Lỗi parse JSON: " + ex.Message);
                OnLoginResult?.Invoke("Lỗi xử lý phản hồi từ server", null);
            }
        }
        else
        {
            string errorMsg = "Lỗi kết nối server";
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    ApiResponse errorResponse = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
                    errorMsg = errorResponse.message;
                }
                catch { }
            }
            Debug.LogError("❌ Lỗi: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Response Body: " + request.downloadHandler.text);
            OnLoginResult?.Invoke(errorMsg, null);
        }
        
        request.Dispose();
    }
}
