-- Almosafir DB Setup for mosafir_db (Run in phpMyAdmin)
-- Assumes tables exist and adds missing columns

USE `mosafir_db`;

ALTER TABLE `users` 
ADD COLUMN IF NOT EXISTS `photo` VARCHAR(255) DEFAULT NULL AFTER `role`,
ADD COLUMN IF NOT EXISTS `plate_number` VARCHAR(20) DEFAULT NULL AFTER `photo`,
ADD COLUMN IF NOT EXISTS `rating` FLOAT DEFAULT 0 AFTER `plate_number`;

ALTER TABLE `trips` 
ADD COLUMN IF NOT EXISTS `driver_id` INT AFTER `id`,
ADD COLUMN IF NOT EXISTS `from_location` VARCHAR(100) DEFAULT '' AFTER `from_city`,
ADD INDEX `idx_driver` (`driver_id`),
ADD FOREIGN KEY (`driver_id`) REFERENCES `users`(`id`) ON DELETE CASCADE;

CREATE TABLE IF NOT EXISTS `bookings` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `trip_id` INT NOT NULL,
  `traveler_id` INT NOT NULL,
  `seats_booked` INT DEFAULT 1,
  `booking_time` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  KEY `idx_trip` (`trip_id`),
  FOREIGN KEY (`trip_id`) REFERENCES `trips`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`traveler_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS `ratings` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `trip_id` INT,
  `traveler_id` INT,
  `driver_id` INT,
  `rating` INT CHECK (rating BETWEEN 1 AND 5),
  `comment` TEXT,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Enhanced BlaBlaCar-like fields for users
ALTER TABLE `users` 
ADD COLUMN IF NOT EXISTS `vehicle_model` VARCHAR(50) DEFAULT NULL AFTER `plate_number`,
ADD COLUMN IF NOT EXISTS `vehicle_year` YEAR DEFAULT NULL AFTER `vehicle_model`,
ADD COLUMN IF NOT EXISTS `preferences` JSON DEFAULT NULL AFTER `rating`,
ADD COLUMN IF NOT EXISTS `city` VARCHAR(100) DEFAULT NULL AFTER `preferences`,
ADD COLUMN IF NOT EXISTS `total_trips` INT DEFAULT 0 AFTER `city`,
ADD COLUMN IF NOT EXISTS `total_earnings` DECIMAL(10,2) DEFAULT 0.00 AFTER `total_trips`;

-- Enhanced trips: pricing + details
ALTER TABLE `trips` 
ADD COLUMN IF NOT EXISTS `price_per_seat` DECIMAL(6,2) DEFAULT 0.00 AFTER `seats`,
ADD COLUMN IF NOT EXISTS `description` TEXT DEFAULT NULL AFTER `price_per_seat`,
ADD COLUMN IF NOT EXISTS `vehicle_info` VARCHAR(255) DEFAULT NULL AFTER `description`,
ADD COLUMN IF NOT EXISTS `status` VARCHAR(20) DEFAULT 'open' AFTER `vehicle_info`;

-- Payments table (Stripe-like)
CREATE TABLE IF NOT EXISTS `payments` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `booking_id` INT NOT NULL,
  `amount` DECIMAL(10,2) NOT NULL,
  `status` ENUM('pending', 'paid', 'failed') DEFAULT 'pending',
  `payment_id` VARCHAR(100) DEFAULT NULL,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (`booking_id`) REFERENCES `bookings`(`id`) ON DELETE CASCADE
);

-- Conversations/Chat (BlaBlaCar-like)
-- Phase 1: booking-owned conversation via booking_id (non-destructive)
CREATE TABLE IF NOT EXISTS `conversations` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `booking_id` INT NULL,
  `trip_id` INT NOT NULL,
  `driver_id` INT NOT NULL,
  `traveler_id` INT NOT NULL,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `last_message_at` TIMESTAMP NULL,
  FOREIGN KEY (`booking_id`) REFERENCES `bookings`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`trip_id`) REFERENCES `trips`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`driver_id`) REFERENCES `users`(`id`),
  FOREIGN KEY (`traveler_id`) REFERENCES `users`(`id`),
  UNIQUE KEY `uq_conversations_booking_id` (`booking_id`)
);

-- Helpful indexes for authorization + listing
CREATE INDEX IF NOT EXISTS `idx_conversations_booking_id` ON `conversations` (`booking_id`);
CREATE INDEX IF NOT EXISTS `idx_conversations_trip` ON `conversations` (`trip_id`);
CREATE INDEX IF NOT EXISTS `idx_conversations_driver` ON `conversations` (`driver_id`);
CREATE INDEX IF NOT EXISTS `idx_conversations_traveler` ON `conversations` (`traveler_id`);
CREATE INDEX IF NOT EXISTS `idx_conversations_last_message_at` ON `conversations` (`last_message_at`);

CREATE TABLE IF NOT EXISTS `messages` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `conversation_id` INT NOT NULL,
  `sender_id` INT NOT NULL,
  `message` TEXT NOT NULL,
  `is_read` BOOLEAN NOT NULL DEFAULT FALSE,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (`conversation_id`) REFERENCES `conversations`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`sender_id`) REFERENCES `users`(`id`)
);

CREATE INDEX IF NOT EXISTS `idx_messages_conversation_created_at` ON `messages` (`conversation_id`, `created_at`);
CREATE INDEX IF NOT EXISTS `idx_messages_sender_created_at` ON `messages` (`sender_id`, `created_at`);


-- Notifications
CREATE TABLE IF NOT EXISTS `notifications` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `user_id` INT NOT NULL,
  `title` VARCHAR(255) NOT NULL,
  `message` TEXT,
  `type` ENUM('booking', 'message', 'trip_update') DEFAULT 'booking',
  `read` BOOLEAN DEFAULT FALSE,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
);

-- Update sample data with new fields (password: 123456)
UPDATE `users` SET 
`plate_number` = '123-اليمن', `vehicle_model` = 'Toyota Corolla', `vehicle_year` = 2020, 
`preferences` = JSON_OBJECT('music', 'yes', 'smoking', 'no', 'ac', 'yes'),
`total_trips` = 15, `total_earnings` = 2500.00 WHERE id = 1;

UPDATE `users` SET 
`plate_number` = '456-اليمن', `vehicle_model` = 'Hyundai Accent', `vehicle_year` = 2018 WHERE id = 2;

UPDATE `users` SET 
`plate_number` = '789-اليمن', `vehicle_model` = 'Nissan Sunny', `vehicle_year` = 2022, 
`total_trips` = 28, `total_earnings` = 4200.00 WHERE id = 3;

INSERT IGNORE INTO `trips` (`driver_id`, `from_city`, `from_location`, `to_city`, `trip_time`, `seats`, `price_per_seat`, `description`, `vehicle_info`, `status`) VALUES 
(1, 'تعز', 'الحوبان', 'عدن', '2024-10-06 08:00:00', 5, 15000.00, 'رحلة مريحة مع تكييف وموسيقى هادئة', 'Toyota Corolla 2020 - AC', 'open'),
(2, 'تعز', 'الحوبان', 'عدن', '2024-10-06 08:00:00', 4, 12000.00, 'رحلة سريعة واقتصادية', 'Hyundai Accent', 'open'),
(3, 'تعز', 'الحوبان', 'عدن', '2024-10-06 10:00:00', 6, 18000.00, 'سيارة فاخرة مع وجبات خفيفة', 'Nissan Sunny 2022 - Premium', 'open');

