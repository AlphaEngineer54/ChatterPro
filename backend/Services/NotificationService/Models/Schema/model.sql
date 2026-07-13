CREATE DATABASE IF NOT EXISTS message_db;

USE message_db;

-- Table structure for table `User`
CREATE TABLE Notification (
    id INT NOT NULL AUTO_INCREMENT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message VARCHAR(255),
    userId INT,
    PRIMARY KEY (id)
);
