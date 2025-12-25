using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FarmController : ControllerBase
{
    [HttpGet("{userId}")]
    public IActionResult GetFarm(int userId)
    {
        var farm = DatabaseHelper.GetFarm(userId);
        
        if (farm != null)
        {
            return Ok(new { status = "success", farm = farm });
        }
        else
        {
            return NotFound(new { status = "error", message = "Không tìm thấy farm" });
        }
    }

    [HttpPost("items/save")]
    public IActionResult SaveFarmItem([FromBody] FarmItemRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
        }

        bool success = DatabaseHelper.SaveFarmItem(
            request.userId, 
            request.itemType, 
            request.itemName, 
            request.positionX, 
            request.positionY, 
            request.growthStage
        );
        
        if (success)
        {
            return Ok(new { status = "success", message = "Đã lưu farm item" });
        }
        else
        {
            return StatusCode(500, new { status = "error", message = "Lỗi lưu farm item" });
        }
    }

    [HttpGet("items/{userId}")]
    public IActionResult GetFarmItems(int userId)
    {
        try
        {
            var items = DatabaseHelper.GetFarmItems(userId);
            return Ok(new { status = "success", items = items });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy farm items: {ex.Message}");
            return StatusCode(500, new { status = "error", message = "Lỗi lấy danh sách farm items" });
        }
    }

    [HttpDelete("items/{itemId}")]
    public IActionResult DeleteFarmItem(int itemId)
    {
        bool success = DatabaseHelper.DeleteFarmItem(itemId);
        
        if (success)
        {
            return Ok(new { status = "success", message = "Đã xóa farm item" });
        }
        else
        {
            return StatusCode(500, new { status = "error", message = "Lỗi xóa farm item" });
        }
    }

    [HttpPost("position/save")]
    public IActionResult SavePlayerPosition([FromBody] PlayerPositionRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
        }

        try
        {
            bool success = DatabaseHelper.SavePlayerPosition(request.userId, request.positionX, request.positionY);
            
            if (success)
            {
                return Ok(new { status = "success", message = "Đã lưu vị trí player" });
            }
            else
            {
                return StatusCode(500, new { status = "error", message = "Lỗi lưu vị trí player" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in SavePlayerPosition: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { status = "error", message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet("position/{userId}")]
    public IActionResult GetPlayerPosition(int userId)
    {
        var position = DatabaseHelper.GetPlayerPosition(userId);
        
        if (position != null)
        {
            return Ok(new { status = "success", position = position });
        }
        else
        {
            return NotFound(new { status = "error", message = "Không tìm thấy vị trí player" });
        }
    }

    [HttpPost("resources/update")]
    public IActionResult UpdateResources([FromBody] ResourcesRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
        }

        bool success = DatabaseHelper.UpdateFarmResources(request.userId, request.coins, request.gems);
        
        if (success)
        {
            return Ok(new { status = "success", message = "Đã cập nhật tài nguyên" });
        }
        else
        {
            return StatusCode(500, new { status = "error", message = "Lỗi cập nhật tài nguyên" });
        }
    }

    [HttpPost("inventory/save")]
    public IActionResult SaveInventory([FromBody] InventoryRequest? request)
    {
        if (request == null || request.seeds == null)
        {
            return BadRequest(new { status = "error", message = "Request body không hợp lệ" });
        }

        try
        {
            var seedsDict = new Dictionary<string, int>();
            foreach (var seed in request.seeds)
            {
                seedsDict[seed.seedType] = seed.quantity;
            }

            bool success = DatabaseHelper.SaveInventorySeeds(request.userId, seedsDict);
            
            if (success)
            {
                return Ok(new { status = "success", message = "Đã lưu inventory" });
            }
            else
            {
                return StatusCode(500, new { status = "error", message = "Lỗi lưu inventory" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in SaveInventory: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { status = "error", message = $"Lỗi: {ex.Message}" });
        }
    }

    [HttpGet("inventory/{userId}")]
    public IActionResult GetInventory(int userId)
    {
        var seeds = DatabaseHelper.GetInventorySeeds(userId);
        return Ok(new { status = "success", seeds = seeds });
    }
}

public class PlayerPositionRequest
{
    public int userId { get; set; }
    public float positionX { get; set; }
    public float positionY { get; set; }
}

public class ResourcesRequest
{
    public int userId { get; set; }
    public int coins { get; set; }
    public int gems { get; set; }
}

public class FarmItemRequest
{
    public int userId { get; set; }
    public string? itemType { get; set; }
    public string? itemName { get; set; }
    public float positionX { get; set; }
    public float positionY { get; set; }
    public int growthStage { get; set; }
}

public class InventoryRequest
{
    public int userId { get; set; }
    public List<SeedData>? seeds { get; set; }
}

public class SeedData
{
    public string seedType { get; set; } = "";
    public int quantity { get; set; }
}

