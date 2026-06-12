<?php
session_start();
require_once "config/db.php";

if (!isset($_SESSION['user_id'])) {
    header("Location: login.php");
    exit;
}

$user_id = $_SESSION['user_id'];
$role = $_SESSION['role'];

if ($role == 'driver') {
    // Driver dashboard: open trips + earnings
    $trips_sql = "SELECT t.*, b.seats_booked FROM trips t LEFT JOIN bookings b ON t.id = b.trip_id WHERE t.driver_id = ? ORDER BY t.trip_time DESC";
    $trips_stmt = $conn->prepare($trips_sql);
    $trips_stmt->bind_param("i", $user_id);
    $trips_stmt->execute();
    $trips = $trips_stmt->get_result()->fetch_all(MYSQLI_ASSOC);
    $trips_stmt->close();
    
    $earnings_sql = "SELECT SUM(b.seats_booked * t.price_per_seat) as total_earnings FROM bookings b JOIN trips t ON b.trip_id = t.id WHERE t.driver_id = ?";
    $earnings_stmt = $conn->prepare($earnings_sql);
    $earnings_stmt->bind_param("i", $user_id);
    $earnings_stmt->execute();
    $earnings = $earnings_stmt->get_result()->fetch_assoc()['total_earnings'] ?? 0;
    $earnings_stmt->close();
    
} else {
    // Traveler dashboard: my bookings
    $bookings_sql = "SELECT b.*, t.* FROM bookings b JOIN trips t ON b.trip_id = t.id JOIN users u ON t.driver_id = u.id WHERE b.traveler_id = ? ORDER BY b.booking_time DESC";
    $bookings_stmt = $conn->prepare($bookings_sql);
    $bookings_stmt->bind_param("i", $user_id);
    $bookings_stmt->execute();
    $bookings = $bookings_stmt->get_result()->fetch_all(MYSQLI_ASSOC);
    $bookings_stmt->close();
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
        <h1>📊 لوحة التحكم - <?= $_SESSION['user_name'] ?></h1>

        
        <?php if ($role == 'driver'): ?>
            <div style="text-align:center; margin-bottom:30px;">
                <h2>💰 إجمالي الأرباح: <?= number_format($earnings, 0) ?> ر.ي</h2>
            </div>
            <h3>رحلاتي المفتوحة (<?= count($trips) ?>)</h3>
            <?php if (empty($trips)): ?>
                <p>لا توجد رحلات مفتوحة. <a href="add_trip.php">نشر رحلة</a></p>
            <?php else: ?>
                <table class="trips-table">
                    <tr><th>الوجهة</th><th>وقت</th><th>مقاعد</th><th>سعر</th><th>حجوزات</th><th>عمليات</th></tr>
                    <?php foreach ($trips as $trip): ?>
                        <tr>
                            <td><?= htmlspecialchars($trip['from_city'].' → '.$trip['to_city']) ?></td>
                            <td><?= date('Y-m-d h:i', strtotime($trip['trip_time'])) ?></td>
                            <td><?= $trip['seats'] ?></td>
                            <td><?= number_format($trip['price_per_seat'],0) ?> ر.ي</td>
                            <td><?= $trip['seats_booked'] ?? 0 ?></td>
                            <td>
                                <a href="chat.php?trip_id=<?= $trip['id'] ?>" class="book-btn">💬 دردشة</a>
                                <a href="#" onclick="alert('إنهاء قريباً')" class="finish-btn">إنهاء</a>
                            </td>
                        </tr>
                    <?php endforeach; ?>
                </table>
            <?php endif; ?>
            
        <?php else: ?>
            <h3>حجوزاتي (<?= count($bookings) ?>)</h3>
            <?php if (empty($bookings)): ?>
                <p>لا توجد حجوزات. <a href="index.php">ابحث رحلات</a></p>
            <?php else: ?>
                <table class="trips-table">
                    <tr><th>الرحلة</th><th>السائق</th><th>التاريخ</th><th>المقاعد</th><th>السعر الإجمالي</th><th>دردشة</th></tr>
                    <?php foreach ($bookings as $booking): ?>
                        <tr>
                            <td><?= htmlspecialchars($booking['from_city'].' → '.$booking['to_city']) ?></td>
                            <td><?= htmlspecialchars($booking['driver_name'] ?? 'غير معروف') ?></td>
                            <td><?= date('Y-m-d h:i', strtotime($booking['trip_time'])) ?></td>
                            <td><?= $booking['seats_booked'] ?></td>
                            <td><?= number_format($booking['seats_booked'] * $booking['price_per_seat'], 0) ?> ر.ي</td>
                            <td><a href="chat.php?trip_id=<?= $booking['trip_id'] ?>" class="book-btn">💬</a></td>
                        </tr>
                    <?php endforeach; ?>
                </table>
            <?php endif; ?>
        <?php endif; ?>
        
        <div style="text-align:center; margin-top:40px;">
            <a href="profile.php" class="book-btn">الملف الشخصي</a>
            <a href="index.php" class="book-btn">الرئيسية</a>
            <a href="login.php?logout=1" style="color:#dc3545;">خروج</a>
        </div>
    </div>
</body>
</html>
