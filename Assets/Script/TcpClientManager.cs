using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class RegisterData
{
    public string type = "register";
    public string email;
    public string password;
}
public class LoginData
{
    public string type = "login";
    public string email;
    public string password;
}

public class TcpClientManager : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 5000;

    public async void RegisterAccount(string email, string password)
    {
        TcpClient client = null;

        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort);
            NetworkStream stream = client.GetStream();

            // Chuẩn bị JSON
            RegisterData data = new RegisterData
            {
                email = email,
                password = password
            };

            string json = JsonUtility.ToJson(data) + "\n"; // Thêm newline để server biết kết thúc
            byte[] bytesToSend = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

            // Nhận phản hồi
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Debug.Log($"Server response: {response}");

            stream.Close();
            client.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi kết nối: {ex.Message}");
            client?.Close();
        }
    }
    public async void LoginAccount(string email, string password)
    {
        TcpClient client = null;

        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, serverPort);
            NetworkStream stream = client.GetStream();

            // Chuẩn bị dữ liệu JSON
            LoginData data = new LoginData
            {
                email = email,
                password = password
            };

            string json = JsonUtility.ToJson(data) + "\n"; // Thêm newline để server nhận biết
            byte[] bytesToSend = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

            // Nhận phản hồi từ server
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Debug.Log($"Server response: {response}");

            // Xử lý kết quả từ server (giả sử server trả về JSON kiểu {"status":"success"} )
            if (response.Contains("success"))
            {
                Debug.Log("Đăng nhập thành công!");
                // TODO: Chuyển scene hoặc hiện thông báo thành công
            }
            else
            {
                Debug.LogWarning("Sai tài khoản hoặc mật khẩu!");
                // TODO: Hiện thông báo lỗi
            }

            stream.Close();
            client.Close();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi kết nối: {ex.Message}");
            client?.Close();
        }
    }    
}

