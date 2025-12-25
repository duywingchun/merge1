-- Script cập nhật database cho các tính năng mới
USE mygarden_db;

-- Thêm cột position vào bảng users (nếu chưa có)
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS position_x FLOAT DEFAULT 0,
ADD COLUMN IF NOT EXISTS position_y FLOAT DEFAULT 0,
ADD COLUMN IF NOT EXISTS last_saved_at TIMESTAMP NULL;

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

