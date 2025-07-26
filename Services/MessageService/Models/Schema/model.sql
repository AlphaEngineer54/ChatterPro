CREATE DATABASE IF NOT EXISTS message_db;

USE message_db;

CREATE TABLE Conversation (
    id INT NOT NULL AUTO_INCREMENT,
    date DATE,
    join_code VARCHAR(40) NOT NULL,
    owner_id INT,
    title VARCHAR(255),
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE Message (
    id INT NOT NULL AUTO_INCREMENT,
    content VARCHAR(255),
    conversation_id INT,
    date DATE,
    status VARCHAR(100),
    sender_id INT,
    receiver_id INT,
    PRIMARY KEY (id),
    INDEX Message_ibfk_1 (conversation_id),
    CONSTRAINT Message_ibfk_1 FOREIGN KEY (conversation_id)
    REFERENCES Conversation(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE UserConversation (
    id INT NOT NULL AUTO_INCREMENT,
    conversation_id INT,
    user_id INT,
    PRIMARY KEY (id),
    INDEX conversation_id (conversation_id),
    CONSTRAINT UserConversation_ibfk_1 FOREIGN KEY (conversation_id)
    REFERENCES Conversation(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
