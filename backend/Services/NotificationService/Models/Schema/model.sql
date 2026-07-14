CREATE DATABASE IF NOT EXISTS notification_db;

USE notification_db;

-- Table structure for table `Notification`
CREATE TABLE Notification (
    id INT NOT NULL AUTO_INCREMENT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message VARCHAR(255),
    userId INT,
    PRIMARY KEY (id)
);
