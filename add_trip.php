<?php
require_once "security.php";
require_once "config/db.php";

if (!isset($_SESSION['user_id']) || $_SESSION['role'] != 'driver') {
    header("Location: login.php");
    exit;
}

$success = $error = '';

// Avoid PHP notices on GET (e.g., missing user_name)
$user_name = htmlspecialchars($_SESSION['user_name'] ?? '', ENT_QUOTES, 'UTF-8');

if ($_SERVER["REQUEST_METHOD"] === "POST") {
    // Single CSRF validation path for all POST actions on this page.
    verify_csrf_token();

    // Single logout handler (handled here, not in a separate block).
    if (isset($_POST['logout'])) {
        session_destroy();
        header("Location: login.php");
        exit;
    }

    // Trip submission branch.
    $from_city = trim($_POST['from_city'] ?? '');
    $from_location = trim($_POST['from_location'] ?? '');
    $to_city = trim($_POST['to_city'] ?? '');
    $trip_time = $_POST['trip_time'] ?? '';
    $seats = (int)($_POST['seats'] ?? 0);
    $price_per_seat = (float)($_POST['price_per_seat'] ?? 0);
    $description = trim($_POST['description'] ?? '');
    $vehicle_info = trim($_POST['vehicle_info'] ?? '');
    $driver_id = $_SESSION['user_id'];

    if (empty($from_city) || empty($to_city) || empty($trip_time) || $seats <= 0 || $price_per_seat <= 0) {
        $error = "جميع الحقول مطلوبة (السعر > 0)";
    } else {
        $stmt = $conn->prepare("INSERT INTO trips (driver_id, from_city, from_location, to_city, trip_time, seats, price_per_seat, description, vehicle_info, status) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, 'open')");
        $stmt->bind_param(
            "issssidss",
            $driver_id,
            $from_city,
            $from_location,
            $to_city,
            $trip_time,
            $seats,
            $price_per_seat,
            $description,
            $vehicle_info
        );

        if ($stmt->execute()) {
            $success = "تم نشر الرحلة بنجاح مع السعر " . number_format($price_per_seat, 0) . " ر.ي!";
        } else {
            $error = "حدث خطأ في النشر: " . $conn->error;
        }

        $stmt->close();
    }
}
?>


<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Almosafir | المسافر</title>
    <link rel="icon" href="assets/images/favicon/favicon-32.png">
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <div style="text-align:center; margin-bottom:10px;">
            <img src="assets/images/logo/logo-main.png" alt="Almosafir" style="height:56px; width:auto;" loading="lazy">
        </div>
        <h2>نشر رحلة جديدة</h2>

        <p>مرحباً <?= $user_name ?> (سائق)</p>
        
        <?php if ($error): ?><div class="error"><?= $error ?></div><?php endif; ?>
        <?php if ($success): ?><div class="success"><?= $success ?></div><?php endif; ?>
        
        <?php include 'locations.php'; ?>
        <form method="POST">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
            <input list="from_cities" type="text" name="from_city" placeholder="المحافظة (تعز)" required>

            <datalist id="from_cities">
                <?php foreach ($yemen_governorates as $city): ?>
                    <option value="<?= $city ?>">
                <?php endforeach; ?>
            </datalist>
            <input list="locations" type="text" name="from_location" placeholder="نقطة الانطلاق (الحوبان)" required>
            <datalist id="locations">
                <?php foreach ($yemen_locations as $loc): ?>
                    <option value="<?= $loc ?>">
                <?php endforeach; ?>
            </datalist>
            <input list="to_cities" type="text" name="to_city" placeholder="الوجهة (عدن)" required>
            <datalist id="to_cities">
                <?php foreach ($yemen_governorates as $city): ?>
                    <option value="<?= $city ?>">
                <?php endforeach; ?>
            </datalist>
            <input type="datetime-local" name="trip_time" required>
            <input type="number" name="seats" placeholder="المقاعد المتاحة" min="1" max="20" required>
            <input type="number" step="1000" name="price_per_seat" placeholder="السعر للمقعد (ر.ي)" min="1000" required>
            <textarea name="description" placeholder="وصف الرحلة (مريحة، تكييف...)"></textarea>
            <input type="text" name="vehicle_info" placeholder="معلومات السيارة (Toyota AC 2020)">
            <button type="submit">🚀 نشر رحلة محسنة</button>
        </form>
        
        <a href="index.php" class="book-btn" style="display:inline-block; margin-top:20px;">← الرئيسية</a>
        <form method="POST" action="add_trip.php" style="display:inline;">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
            <button type="submit" name="logout" value="1" style="color:#dc3545; background:transparent; border:none; cursor:pointer; font-size:1em;">تسجيل خروج</button>
        </form>

    </div>
</body>
</html>

