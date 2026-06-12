<?php
require_once "security.php";
require_once "config/db.php";
require_once "helpers/ui.php";


if (!isset($_SESSION['user_id'])) {
    header("Location: login.php");
    exit;
}

$user_id = $_SESSION['user_id'];
$stmt = $conn->prepare("SELECT * FROM users WHERE id = ?");
$stmt->bind_param("i", $user_id);
$stmt->execute();
$user = $stmt->get_result()->fetch_assoc();
$stmt->close();

$role = $user['role'];
if ($role == 'driver') {
    // Driver trips count/earnings
    $trips_stmt = $conn->prepare("SELECT COUNT(*) as total, SUM(seats * price_per_seat) as potential_earnings FROM trips WHERE driver_id = ? AND status = 'open'");
    $trips_stmt->bind_param("i", $user_id);
    $trips_stmt->execute();
    $stats = $trips_stmt->get_result()->fetch_assoc();
    $trips_stmt->close();
} else {
    // Traveler bookings
    $bookings_stmt = $conn->prepare("SELECT COUNT(*) as total_bookings FROM bookings WHERE traveler_id = ?");
    $bookings_stmt->bind_param("i", $user_id);
    $bookings_stmt->execute();
    $stats = $bookings_stmt->get_result()->fetch_assoc();
    $bookings_stmt->close();
}

// Recent ratings
$ratings_sql = "SELECT r.rating, r.comment, u.name FROM ratings r JOIN users u ON u.id = CASE WHEN r.driver_id = ? THEN r.traveler_id ELSE r.driver_id END WHERE (r.driver_id = ? OR r.traveler_id = ?) ORDER BY r.created_at DESC LIMIT 5";
$ratings_stmt = $conn->prepare($ratings_sql);
$ratings_stmt->bind_param("iii", $user_id, $user_id, $user_id);
$ratings_stmt->execute();
$ratings = $ratings_stmt->get_result()->fetch_all(MYSQLI_ASSOC);
$ratings_stmt->close();
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
        <h1>👤 <?= htmlspecialchars($user['name']) ?> (<?= $user['role'] == 'driver' ? 'سائق' : 'مسافر' ?>)</h1>

        
        <div style="display:grid; grid-template-columns:1fr 1fr; gap:30px; margin-bottom:30px;">
            <div class="profile-card">
                <h3>معلوماتي</h3>
                <p><strong>الهاتف:</strong> <?= htmlspecialchars($user['phone']) ?></p>
                <?php if ($user['role'] == 'driver'): ?>
                    <p><strong>لوحة:</strong> <?= htmlspecialchars($user['plate_number']) ?></p>
                    <p><strong>السيارة:</strong> <?= htmlspecialchars($user['vehicle_model'] ?? 'غير محدد') ?> <?= $user['vehicle_year'] ?></p>
                    <?php if ($user['preferences']): ?>
                        <p><strong>التفضيلات:</strong> <?= htmlspecialchars(json_encode($user['preferences'], JSON_UNESCAPED_UNICODE)) ?></p>
                    <?php endif; ?>
                <?php endif; ?>
                <p><strong>التقييم:</strong> <?= number_format($user['rating'],1) ?></p>
            </div>
            <div class="profile-card">
                <h3>إحصائيات</h3>
                <?php if ($user['role'] == 'driver'): ?>
                    <p>رحلات مفتوحة: <?= $stats['total'] ?></p>
                    <p>إيرادات محتملة: <?= number_format($stats['potential_earnings'], 0) ?> ر.ي</p>
                    <p>إجمالي رحلات: <?= $user['total_trips'] ?></p>
                    <p>إجمالي أرباح: <?= number_format($user['total_earnings'], 0) ?> ر.ي</p>
                <?php else: ?>
                    <p>حجوزاتي: <?= $stats['total_bookings'] ?></p>
                <?php endif; ?>
            </div>
        </div>

        <h3>آخر التقييمات</h3>
        <?php if (empty($ratings)): ?>
            <p>لا توجد تقييمات بعد</p>
        <?php else: ?>
            <table class="trips-table">
                <tr><th>التقييم</th><th>التعليق</th><th>من</th></tr>
                <?php foreach ($ratings as $r): ?>
                    <tr><td class="stars"><?= stars($r['rating']) ?></td><td><?= htmlspecialchars($r['comment']) ?></td><td><?= htmlspecialchars($r['name']) ?></td></tr>
                <?php endforeach; ?>
            </table>
        <?php endif; ?>

        <div style="text-align:center; margin-top:30px;">
            <?php if ($user['role'] == 'driver'): ?>
                <a href="add_trip.php" class="book-btn" style="background:linear-gradient(135deg,#4ecdc4,#44a08d);">نشر رحلة</a>
            <?php endif; ?>
            <a href="dashboard.php" class="book-btn">لوحة التحكم</a>
            <a href="index.php" class="book-btn">الرئيسية</a>
            <form method="POST" action="profile.php" style="display:inline;">
                <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
                <button type="submit" name="logout" value="1" style="color:#dc3545; background:transparent; border:none; cursor:pointer; font-size:1em;">خروج</button>
            </form>
        </div>
        
        <?php
            if (isset($_POST['logout'])) {
                verify_csrf_token();
                session_destroy();
                header("Location: login.php");
                exit;
            }
        ?>

    </div>
</body>
</html>
