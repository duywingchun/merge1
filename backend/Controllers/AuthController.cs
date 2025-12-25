using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest? request)
    {
        try
        {
            // Kiểm tra request null
            if (request == null)
            {
                Console.WriteLine("[Register] ❌ Request body null");
                return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
            }

            // Trim email và password để tránh lỗi do whitespace
            string email = request.email?.Trim() ?? "";
            string password = request.password ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine($"[Register] ❌ Email hoặc password rỗng");
                return BadRequest(new { status = "error", message = "Email và password không được để trống" });
            }

            // Kiểm tra độ dài password
            if (password.Length < 6)
            {
                Console.WriteLine($"[Register] ❌ Password quá ngắn: {password.Length} ký tự");
                return BadRequest(new { status = "error", message = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            Console.WriteLine($"[Register] 📝 Nhận request đăng ký - Email: '{email}', Password length: {password.Length}");

            bool success = DatabaseHelper.RegisterUser(email, password);
            
            if (success)
            {
                Console.WriteLine($"[Register] ✅ Đăng ký thành công cho email: {email}");
                return Ok(new { status = "success", message = "Đăng ký thành công!" });
            }
            else
            {
                Console.WriteLine($"[Register] ❌ Đăng ký thất bại cho email: {email}");
                return BadRequest(new { status = "error", message = "Email đã tồn tại hoặc có lỗi xảy ra" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Register] ❌ Exception: {ex.Message}");
            Console.WriteLine($"[Register] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { status = "error", message = "Lỗi server: " + ex.Message });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest? request)
    {
        // Kiểm tra request null
        if (request == null)
        {
            return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
        }

        // Trim email và password để tránh lỗi do whitespace
        string email = request.email?.Trim() ?? "";
        string password = request.password ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return BadRequest(new { status = "error", message = "Email và password không được để trống" });
        }

        Console.WriteLine($"[Login] Email: '{email}', Password length: {password.Length}");

        int? userId = DatabaseHelper.LoginUser(email, password);
        
        if (userId.HasValue)
        {
            Console.WriteLine($"[Login] ✅ Thành công! UserId: {userId.Value}");
            return Ok(new { status = "success", userId = userId.Value, message = "Đăng nhập thành công!" });
        }
        else
        {
            Console.WriteLine($"[Login] ❌ Thất bại: Email hoặc password không đúng");
            return Unauthorized(new { status = "error", message = "Sai email hoặc password" });
        }
    }
}

public class RegisterRequest
{
    public string email { get; set; } = "";
    public string password { get; set; } = "";
}

public class LoginRequest
{
    public string email { get; set; } = "";
    public string password { get; set; } = "";
}

