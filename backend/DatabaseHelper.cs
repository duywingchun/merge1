using MySql.Data.MySqlClient;

public class DatabaseHelper
{
    private static string connectionString = "Server=localhost;Database=mygarden_db;User=root;Password=123456789;";

    // Đăng ký tài khoản mới
    public static bool RegisterUser(string email, string password)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                // Kiểm tra email đã tồn tại chưa (không phân biệt hoa thường)
                string checkQuery = "SELECT COUNT(*) FROM users WHERE LOWER(email) = LOWER(@email)";
                using (var checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@email", email);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine($"[RegisterUser] Email đã tồn tại: {email}");
                        return false; // Email đã tồn tại
                    }
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                Console.WriteLine($"[RegisterUser] Đã hash password, length: {hashedPassword.Length}");

                // Tạo user mới
                string insertQuery = "INSERT INTO users (email, password_hash, username, created_at) VALUES (@email, @password, @username, NOW())";
                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@username", email.Split('@')[0]); // Dùng phần trước @ làm username
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[RegisterUser] ✅ Đã tạo user: {email}");
                }

                // Tạo farm mặc định cho user
                int userId = GetUserIdByEmail(email);
                Console.WriteLine($"[RegisterUser] UserId sau khi tạo: {userId}");
                if (userId > 0)
                {
                    bool farmCreated = CreateFarm(userId);
                    if (farmCreated)
                    {
                        Console.WriteLine($"[RegisterUser] ✅ Đã tạo farm cho userId: {userId}");
                    }
                    else
                    {
                        Console.WriteLine($"[RegisterUser] ⚠️ Không thể tạo farm cho userId: {userId}");
                    }
                }
                else
                {
                    Console.WriteLine($"[RegisterUser] ⚠️ Không tìm thấy userId sau khi tạo user!");
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi đăng ký: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    // Đăng nhập
    public static int? LoginUser(string email, string password)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                // Sử dụng LOWER() để so sánh email không phân biệt hoa thường
                string query = "SELECT user_id, password_hash FROM users WHERE LOWER(email) = LOWER(@email)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPassword = reader.GetString("password_hash");
                            Console.WriteLine($"[LoginUser] Tìm thấy user, đang verify password...");
                            bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                            Console.WriteLine($"[LoginUser] Password verify result: {passwordMatch}");
                            
                            if (passwordMatch)
                            {
                                int userId = reader.GetInt32("user_id");
                                Console.WriteLine($"[LoginUser] ✅ Password đúng! UserId: {userId}");
                                
                                // Cập nhật last_login
                                reader.Close();
                                UpdateLastLogin(userId);
                                
                                return userId;
                            }
                            else
                            {
                                Console.WriteLine($"[LoginUser] ❌ Password không khớp");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[LoginUser] ❌ Không tìm thấy user với email: {email}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        return null;
    }

    // Tạo farm mặc định
    private static bool CreateFarm(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                // Kiểm tra xem user đã có farm chưa
                string checkQuery = "SELECT COUNT(*) FROM farms WHERE user_id = @userId";
                using (var checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@userId", userId);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        Console.WriteLine($"[CreateFarm] User {userId} đã có farm rồi, bỏ qua tạo mới");
                        return true;
                    }
                }
                
                string query = "INSERT INTO farms (user_id, farm_name, level, coins, gems) VALUES (@userId, 'My Farm', 1, 1000, 50)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"[CreateFarm] ✅ Đã tạo farm cho userId: {userId}");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateFarm] ❌ Lỗi tạo farm: {ex.Message}");
            Console.WriteLine($"[CreateFarm] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    // Lấy thông tin farm
    public static Dictionary<string, object>? GetFarm(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM farms WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Dictionary<string, object>
                            {
                                {"farm_id", reader.GetInt32("farm_id")},
                                {"farm_name", reader.GetString("farm_name")},
                                {"level", reader.GetInt32("level")},
                                {"coins", reader.GetInt32("coins")},
                                {"gems", reader.GetInt32("gems")}
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy farm: {ex.Message}");
        }
        return null;
    }

    // Lấy user_id từ email
    private static int GetUserIdByEmail(string email)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT user_id FROM users WHERE email = @email";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    object? result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
        catch
        {
            return 0;
        }
    }

    // Cập nhật last_login
    private static void UpdateLastLogin(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET last_login = NOW(), is_online = TRUE WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi cập nhật last_login: {ex.Message}");
        }
    }

    // Lưu tin nhắn chat
    public static bool SaveChatMessage(int userId, string username, string message)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO chat_messages (sender_id, message_text, message_type, sent_at) VALUES (@userId, @message, 'global', NOW())";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@message", message);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine($"✅ Đã lưu tin nhắn: userId={userId}, username={username}, message_text='{message}', message_type='global'");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu tin nhắn: {ex.Message}");
            return false;
        }
    }

    // Lấy lịch sử chat
    public static List<Dictionary<string, object>> GetChatHistory(int limit = 20)
    {
        var messages = new List<Dictionary<string, object>>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT cm.message_id, cm.sender_id, u.username, cm.message_text, cm.sent_at
                    FROM chat_messages cm
                    JOIN users u ON cm.sender_id = u.user_id
                    ORDER BY cm.sent_at DESC
                    LIMIT @limit";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@limit", limit);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Lấy username, nếu null thì dùng "User" + sender_id
                            int usernameOrdinal = reader.GetOrdinal("username");
                            string username = reader.IsDBNull(usernameOrdinal) 
                                ? "User" + reader.GetInt32("sender_id") 
                                : reader.GetString("username");
                            
                            string messageText = reader.GetString("message_text");
                            
                            // Debug log
                            Console.WriteLine($"[GetChatHistory] message_id={reader.GetInt32("message_id")}, sender_id={reader.GetInt32("sender_id")}, username='{username}', message_text='{messageText}'");
                            
                            messages.Add(new Dictionary<string, object>
                            {
                                {"message_id", reader.GetInt32("message_id")},
                                {"sender_id", reader.GetInt32("sender_id")},
                                {"username", username},
                                {"message_text", messageText},
                                {"sent_at", reader.GetDateTime("sent_at").ToString("yyyy-MM-dd HH:mm:ss")}
                            });
                        }
                    }
                }
            }
            // Đảo ngược để tin nhắn cũ nhất ở đầu
            messages.Reverse();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy chat history: {ex.Message}");
        }
        return messages;
    }

    // Lấy danh sách users online
    public static List<Dictionary<string, object>> GetOnlineUsers()
    {
        var users = new List<Dictionary<string, object>>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT user_id, username, email FROM users WHERE is_online = TRUE";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new Dictionary<string, object>
                            {
                                {"user_id", reader.GetInt32("user_id")},
                                {"username", reader.GetString("username")},
                                {"email", reader.GetString("email")}
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy online users: {ex.Message}");
        }
        return users;
    }

    // Lấy username từ user_id
    public static string GetUsernameById(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT username FROM users WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    object? result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() ?? "Unknown" : "Unknown";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy username: {ex.Message}");
            return "Unknown";
        }
    }

    // Lưu farm item (cây trồng)
    public static bool SaveFarmItem(int userId, string itemType, string itemName, float positionX, float positionY, int growthStage)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                // Lấy farm_id từ user_id
                int farmId = GetFarmIdByUserId(userId);
                if (farmId <= 0)
                {
                    Console.WriteLine($"Không tìm thấy farm cho userId: {userId}");
                    return false;
                }
                
                string query = @"INSERT INTO farm_items (farm_id, item_type, item_name, position_x, position_y, growth_stage, planted_at) 
                                VALUES (@farmId, @itemType, @itemName, @positionX, @positionY, @growthStage, NOW())
                                ON DUPLICATE KEY UPDATE 
                                growth_stage = @growthStage,
                                position_x = @positionX,
                                position_y = @positionY";
                
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@farmId", farmId);
                    cmd.Parameters.AddWithValue("@itemType", itemType);
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.Parameters.AddWithValue("@positionX", positionX);
                    cmd.Parameters.AddWithValue("@positionY", positionY);
                    cmd.Parameters.AddWithValue("@growthStage", growthStage);
                    cmd.ExecuteNonQuery();
                }
                
                Console.WriteLine($"✅ Đã lưu farm item: userId={userId}, itemType={itemType}, position=({positionX}, {positionY}), stage={growthStage}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu farm item: {ex.Message}");
            return false;
        }
    }

    // Lấy tất cả farm items của user
    public static List<Dictionary<string, object>> GetFarmItems(int userId)
    {
        var items = new List<Dictionary<string, object>>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                int farmId = GetFarmIdByUserId(userId);
                if (farmId <= 0) return items;
                
                string query = "SELECT * FROM farm_items WHERE farm_id = @farmId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@farmId", farmId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Dictionary<string, object>
                            {
                                {"item_id", reader.GetInt32("item_id")},
                                {"item_type", reader.GetString("item_type")},
                                {"item_name", reader.GetString("item_name")},
                                {"position_x", reader.GetFloat("position_x")},
                                {"position_y", reader.GetFloat("position_y")},
                                {"growth_stage", reader.GetInt32("growth_stage")},
                                {"planted_at", reader.GetDateTime("planted_at").ToString("yyyy-MM-dd HH:mm:ss")}
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy farm items: {ex.Message}");
        }
        return items;
    }

    // Xóa farm item
    public static bool DeleteFarmItem(int itemId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM farm_items WHERE item_id = @itemId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@itemId", itemId);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi xóa farm item: {ex.Message}");
            return false;
        }
    }

    // Lấy farm_id từ user_id
    private static int GetFarmIdByUserId(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT farm_id FROM farms WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    object? result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }
        catch
        {
            return 0;
        }
    }

    // Lưu vị trí player
    public static bool SavePlayerPosition(int userId, float positionX, float positionY)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET position_x = @x, position_y = @y, last_saved_at = NOW() WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@x", positionX);
                    cmd.Parameters.AddWithValue("@y", positionY);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine($"✅ Đã lưu vị trí player: userId={userId}, position=({positionX}, {positionY})");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu vị trí player: {ex.Message}");
            return false;
        }
    }

    // Lấy vị trí player
    public static Dictionary<string, object>? GetPlayerPosition(int userId)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT position_x, position_y, last_saved_at FROM users WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int lastSavedAtIndex = reader.GetOrdinal("last_saved_at");
                            return new Dictionary<string, object>
                            {
                                {"position_x", reader.GetFloat("position_x")},
                                {"position_y", reader.GetFloat("position_y")},
                                {"last_saved_at", reader.IsDBNull(lastSavedAtIndex) ? "" : reader.GetDateTime(lastSavedAtIndex).ToString("yyyy-MM-dd HH:mm:ss")}
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy vị trí player: {ex.Message}");
        }
        return null;
    }

    // Cập nhật tài nguyên (coins, gems)
    public static bool UpdateFarmResources(int userId, int coins, int gems)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                int farmId = GetFarmIdByUserId(userId);
                if (farmId <= 0) return false;
                
                string query = "UPDATE farms SET coins = @coins, gems = @gems WHERE farm_id = @farmId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@farmId", farmId);
                    cmd.Parameters.AddWithValue("@coins", coins);
                    cmd.Parameters.AddWithValue("@gems", gems);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine($"✅ Đã cập nhật tài nguyên: userId={userId}, coins={coins}, gems={gems}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi cập nhật tài nguyên: {ex.Message}");
            return false;
        }
    }

    // Lưu inventory seeds (hạt giống)
    public static bool SaveInventorySeeds(int userId, Dictionary<string, int> seeds)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                foreach (var seed in seeds)
                {
                    string query = @"INSERT INTO inventory_seeds (user_id, seed_type, quantity) 
                                    VALUES (@userId, @seedType, @quantity)
                                    ON DUPLICATE KEY UPDATE quantity = @quantity, updated_at = NOW()";
                    
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@seedType", seed.Key);
                        cmd.Parameters.AddWithValue("@quantity", seed.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                Console.WriteLine($"✅ Đã lưu inventory seeds: userId={userId}, count={seeds.Count}");
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lưu inventory seeds: {ex.Message}");
            return false;
        }
    }

    // Lấy inventory seeds
    public static Dictionary<string, int> GetInventorySeeds(int userId)
    {
        var seeds = new Dictionary<string, int>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT seed_type, quantity FROM inventory_seeds WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string seedType = reader.GetString("seed_type");
                            int quantity = reader.GetInt32("quantity");
                            seeds[seedType] = quantity;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi lấy inventory seeds: {ex.Message}");
        }
        return seeds;
    }
}

