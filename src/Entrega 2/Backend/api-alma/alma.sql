CREATE DATABASE IF NOT EXISTS almaDB;

use almaDB;


--TABELA DE UPLOADS
CREATE TABLE IF NOT EXISTS uploads (
    id VARCHAR(36) NOT NULL PRIMARY KEY,
    path VARCHAR(500) NOT NULL,
    type VARCHAR(50),
    original_name VARCHAR(255),
    mime_type VARCHAR(100),
    size BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE USUARIOS
CREATE TABLE users (
    id VARCHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role ENUM('admin', 'user') DEFAULT 'user',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_verified TINYINT(1) DEFAULT 0
);

--TABELA DE CONTROLE DE SESSÕES
CREATE TABLE IF NOT EXISTS sessions (
    id CHAR(36) PRIMARY KEY,            -- uuid do registro da sessão
    user_id VARCHAR(36),          -- uuid do usuário
    jti CHAR(36) NOT NULL,              -- jti do token JWT
    ip VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

--TABELA DE AUDITORIA DE MANIPULAÇÕES
CREATE TABLE IF NOT EXISTS audit_logs (
    id CHAR(36) PRIMARY KEY,
    user_id CHAR(36),
    action VARCHAR(100),
    resource VARCHAR(255),
    details TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE TENTATIVAS DE AUTENTICAÇÃO
CREATE TABLE IF NOT EXISTS auth_attempts (
    id CHAR(36) PRIMARY KEY,
    email VARCHAR(255),
    ip VARCHAR(50),
    user_agent VARCHAR(255),
    action VARCHAR(50), -- 'login' ou 'register'
    success TINYINT(1) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE BLOCKLIST DE EMAILS
CREATE TABLE IF NOT EXISTS email_blocklist (
    id INT AUTO_INCREMENT PRIMARY KEY,
    domain VARCHAR(255) UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE RESET DE SENHAS
CREATE TABLE IF NOT EXISTS password_resets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    token VARCHAR(255) NOT NULL,
    expires_at DATETIME NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE ATIVIDADES
CREATE TABLE IF NOT EXISTS activities (
    id VARCHAR(50) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    image_path VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

--TABELA DE ARQUIVOS DE TRANSPARÊNCIA
CREATE TABLE IF NOT EXISTS transparency_files (
    id VARCHAR(50) PRIMARY KEY,
    upload_id INT NOT NULL,                -- referencia a tabela uploads (assume id INT AUTO_INCREMENT)
    title VARCHAR(255) NOT NULL,           -- título descritivo do arquivo
    description TEXT,                       -- descrição opcional (uso interno)
    is_public TINYINT(1) DEFAULT 1,         -- 1 = visível publicamente, 0 = apenas admin
    created_by VARCHAR(50),                 -- id do usuário que fez upload (opcional)
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (upload_id) REFERENCES uploads(id) ON DELETE CASCADE
);

--TABELA DE DOAÇÕES
CREATE TABLE IF NOT EXISTS donations (
    id VARCHAR(50) PRIMARY KEY,
    user_id VARCHAR(36),
    amount DECIMAL(10,2) NOT NULL,
    method ENUM('pix','card','transfer','qr') NOT NULL,
    status ENUM('pending','paid','failed') DEFAULT 'pending',
    transaction_token VARCHAR(200),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

--TABELA DE LOG DE DOAÇÕES
CREATE TABLE IF NOT EXISTS donations_log (
    id VARCHAR(50) PRIMARY KEY,
    donation_id VARCHAR(50),
    user_id VARCHAR(36),
    action VARCHAR(50),
    details TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (donation_id) REFERENCES donations(id),
    FOREIGN KEY (user_id) REFERENCES users(id)
);

--TABELA DE MENSAGENS DE FEEDBACK
CREATE TABLE IF NOT EXISTS feedback_messages (
    id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    subject VARCHAR(255),
    message TEXT NOT NULL,
    attached_file_id INT,         -- opcional: se permitir anexos e integrar com uploads
    status ENUM('new','read','archived') DEFAULT 'new',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

--TABELA DE EVENTOS
CREATE TABLE IF NOT EXISTS events (
    id VARCHAR(50) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    image VARCHAR(255),
    event_date DATE NOT NULL,
    location VARCHAR(255),
    social_link VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NULL
);

CREATE TABLE email_verifications (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    token VARCHAR(255) NOT NULL,
    expires_at DATETIME NOT NULL,
    used TINYINT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
