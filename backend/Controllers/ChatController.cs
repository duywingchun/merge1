using Microsoft.AspNetCore.Mvc;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    [HttpGet("history")]
    public IActionResult GetChatHistory([FromQuery] int limit = 50)
    {
        var messagesDict = DatabaseHelper.GetChatHistory(limit);
        
        // Convert Dictionary to ChatMessageDto để Unity JsonUtility parse được
        var messages = messagesDict.Select(m => new ChatMessageDto
        {
            message_id = Convert.ToInt32(m["message_id"]),
            sender_id = Convert.ToInt32(m["sender_id"]),
            username = m["username"]?.ToString() ?? "Unknown",
            messageText = m["message_text"]?.ToString() ?? "", // Map message_text từ DB sang messageText cho Unity
            sent_at = m["sent_at"]?.ToString() ?? ""
        }).ToArray();
        
        return Ok(new { status = "success", messages = messages });
    }

    [HttpGet("online-users")]
    public IActionResult GetOnlineUsers()
    {
        var users = DatabaseHelper.GetOnlineUsers();
        return Ok(new { status = "success", users = users });
    }

    [HttpPost("send")]
    public IActionResult SendChat([FromBody] ChatRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.message))
        {
            return BadRequest(new { status = "error", message = "Message không được để trống" });
        }

        // Lưu vào database
        // Lấy username nếu chưa có
        string username = request.username;
        if (string.IsNullOrEmpty(username))
        {
            username = DatabaseHelper.GetUsernameById(request.userId);
        }
        DatabaseHelper.SaveChatMessage(request.userId, username, request.message);
        
        // Broadcast qua SignalR (nếu có hub)
        // Note: Cần inject IHubContext để broadcast từ controller
        // Tạm thời chỉ lưu vào database
        
        return Ok(new { status = "success", message = "Đã gửi tin nhắn" });
    }
}

public class ChatRequest
{
    public int userId { get; set; }
    public string username { get; set; } = "";
    public string message { get; set; } = "";
}

public class ChatMessageDto
{
    public int message_id { get; set; }
    public int sender_id { get; set; }
    public string username { get; set; } = "";
    // Đổi tên để Unity JsonUtility parse được (không dùng dấu gạch dưới)
    public string messageText { get; set; } = "";
    public string sent_at { get; set; } = "";
}

