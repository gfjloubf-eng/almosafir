-- =============================================================
-- ملاحظة (2026-08-31): المنصة تطبق الهجرات تلقائياً عند الإقلاع الآن —
-- هذا الملف لم يعد ضرورياً، ويبقى مرجعاً يدوياً آمناً للتكرار فقط.
-- Phase 2: جداول شبكة الخطوط (RouteLines) — تطبيق يدوي مضمون
-- المطابق لما كان سيولّده: dotnet ef database update
-- الهجرة: 20260831200000_Phase2_RouteLines (EF Core 9.0.0 / Pomelo 9.0.0)
-- يُنفَّذ عبر phpMyAdmin → قاعدة mosafir_db → تبويب SQL
-- آمن للتكرار: IF NOT EXISTS للجداول + NOT EXISTS لسجل الهجرة
-- =============================================================

USE `mosafir_db`;

-- 1) الخطوط الرئيسية
CREATE TABLE IF NOT EXISTS `route_lines` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `name` VARCHAR(150) NOT NULL,
    `city` VARCHAR(100) NOT NULL,
    `description` LONGTEXT NULL,
    `is_active` TINYINT(1) NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    PRIMARY KEY (`Id`)
) CHARACTER SET utf8mb4;

-- 2) مواقف الخط (مرتبة)
CREATE TABLE IF NOT EXISTS `line_stops` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `line_id` INT NOT NULL,
    `name` VARCHAR(150) NOT NULL,
    `order_index` INT NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_line_stops_line_id` (`line_id`),
    CONSTRAINT `FK_line_stops_route_lines_line_id`
        FOREIGN KEY (`line_id`) REFERENCES `route_lines` (`Id`)
        ON DELETE CASCADE
) CHARACTER SET utf8mb4;

-- 3) جدولة مواعيد الخط (اليوم + وقت الانطلاق)
CREATE TABLE IF NOT EXISTS `line_schedules` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `line_id` INT NOT NULL,
    `day_of_week` INT NOT NULL,
    `departure_time` TIME(6) NOT NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_line_schedules_line_id` (`line_id`),
    CONSTRAINT `FK_line_schedules_route_lines_line_id`
        FOREIGN KEY (`line_id`) REFERENCES `route_lines` (`Id`)
        ON DELETE CASCADE
) CHARACTER SET utf8mb4;

-- 4) سجل الهجرة في دفتر EF — يمنع إعادة تطبيقها مستقبلاً عند تشغيل dotnet ef
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260831200000_Phase2_RouteLines', '9.0.0'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260831200000_Phase2_RouteLines'
);
