-- Script sửa lỗi database (chạy nếu gặp lỗi 500)
USE mygarden_db;

-- Kiểm tra và thêm cột position vào bảng users (nếu chưa có)
-- Lưu ý: MySQL không hỗ trợ IF NOT EXISTS trong ALTER TABLE, nên chạy từng lệnh
-- Nếu cột đã tồn tại sẽ báo lỗi, bỏ qua lỗi đó

-- Thêm cột position_x
ALTER TABLE users ADD COLUMN position_x FLOAT DEFAULT 0;

-- Thêm cột position_y  
ALTER TABLE users ADD COLUMN position_y FLOAT DEFAULT 0;

-- Thêm cột last_saved_at
ALTER TABLE users ADD COLUMN last_saved_at TIMESTAMP NULL;

-- Tạo bảng inventory_seeds (nếu chưa có)
CREATE TABLE IF NOT EXISTS inventory_seeds (
    inventory_id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    seed_type VARCHAR(50) NOT NULL,
    quantity INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_seed (user_id, seed_type)
);

-- Tạo index (bỏ qua lỗi nếu đã tồn tại)
CREATE INDEX idx_inventory_user ON inventory_seeds(user_id);

