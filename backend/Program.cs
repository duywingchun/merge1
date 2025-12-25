using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on all interfaces (0.0.0.0) for remote connections
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên property
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", policy =>
    {
        policy.AllowAnyOrigin()  // Cho phép tất cả origin (để Unity kết nối được)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure pipeline
app.UseCors("AllowUnity");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

// Start the server (URL already configured in builder.WebHost.UseUrls above)
app.Run();

// SignalR Hub cho real-time communication
public class GameHub : Hub
{
    // Khi client kết nối
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }

    // Khi client ngắt kết nối
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
    }

    // Gửi tin nhắn chat
    public async Task SendChatMessage(string username, string message)
    {
        await Clients.All.SendAsync("ReceiveChatMessage", username, message);
    }

    // Cập nhật vị trí player
    public async Task UpdatePlayerPosition(string userId, float x, float y)
    {
        await Clients.Others.SendAsync("PlayerMoved", userId, x, y);
    }

    // Tham gia room (scene)
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    // Rời room
    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }
}

