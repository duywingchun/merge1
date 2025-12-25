-- Database schema cho game MyGarden
CREATE DATABASE IF NOT EXISTS mygarden_db;
USE mygarden_db;

-- Bảng users
CREATE TABLE IF NOT EXISTS users (
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    username VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL,
    is_online BOOLEAN DEFAULT FALSE,
    position_x FLOAT DEFAULT 0,
    position_y FLOAT DEFAULT 0,
    last_saved_at TIMESTAMP NULL
);

-- Bảng farms
CREATE TABLE IF NOT EXISTS farms (
    farm_id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    farm_name VARCHAR(100) DEFAULT 'My Farm',
    level INT DEFAULT 1,
    experience INT DEFAULT 0,
    coins INT DEFAULT 1000,
    gems INT DEFAULT 50,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Bảng farm_items (vật phẩm trong nông trại)
CREATE TABLE IF NOT EXISTS farm_items (
    item_id INT PRIMARY KEY AUTO_INCREMENT,
    farm_id INT NOT NULL,
    item_type VARCHAR(50) NOT NULL,
    item_name VARCHAR(100) NOT NULL,
    position_x FLOAT NOT NULL,
    position_y FLOAT NOT NULL,
    growth_stage INT DEFAULT 0,
    planted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (farm_id) REFERENCES farms(farm_id) ON DELETE CASCADE
);

-- Bảng chat_messages
CREATE TABLE IF NOT EXISTS chat_messages (
    message_id INT PRIMARY KEY AUTO_INCREMENT,
    sender_id INT NOT NULL,
    message_text TEXT NOT NULL,
    message_type VARCHAR(20) DEFAULT 'global',
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (sender_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Bảng inventory_seeds (hạt giống của player)
CREATE TABLE IF NOT EXISTS inventory_seeds (
    inventory_id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    seed_type VARCHAR(50) NOT NULL,
    quantity INT DEFAULT 0,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_seed (user_id, seed_type)
);

-- Index để tăng tốc truy vấn
CREATE INDEX idx_user_email ON users(email);
CREATE INDEX idx_farm_user ON farms(user_id);
CREATE INDEX idx_chat_sender ON chat_messages(sender_id);
CREATE INDEX idx_inventory_user ON inventory_seeds(user_id);

