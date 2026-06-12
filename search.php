<?php
session_start();
require_once "config/db.php";

// Get search params
$from_city = isset($_GET['from_city']) ? trim($_GET['from_city']) : '';
$from_location = isset($_GET['from_location']) ? trim($_GET['from_location']) : '';
$to_city = isset($_GET['to_city']) ? trim($_GET['to_city']) : '';

if (empty($from_city) || empty($to_city)) {
    $error = "يرجى تحديد المحافظة والوجهة";
} else {
    // Secure prepared query with JOIN for driver rating
$sql = "SELECT t.id, t.from_city, t.from_location, t.to_city, t.trip_time, t.seats, t.price_per_seat, t.description, t.vehicle_info,
                   u.name AS driver_name, u.rating AS driver_rating, u.vehicle_model, u.plate_number
            FROM trips t 
            JOIN users u ON t.driver_id = u.id
            WHERE t.from_city LIKE ? AND t.to_city LIKE ? AND t.status = 'open'";
    
    $like_from = "%$from_city%";
    $like_to = "%$to_city%";
    $max_price = isset($_GET['max_price']) ? (float)$_GET['max_price'] : 0;
    $min_rating = isset($_GET['min_rating']) ? (float)$_GET['min_rating'] : 0;
    
    $types = "ss";
    $params = [$like_from, $like_to];
    
    if ($max_price > 0) {
        $sql .= " AND t.price_per_seat <= ?";
        $types .= "d";
        $params[] = $max_price;
    }
    if ($min_rating > 0) {
        $sql .= " AND u.rating >= ?";
        $types .= "d";
        $params[] = $min_rating;
    }
    $sql .= " ORDER BY t.trip_time ASC, u.rating DESC";
    
    $stmt = $conn->prepare($sql);
    $stmt->bind_param($types, ...$params);
    $stmt->execute();
    $result = $stmt->get_result();
    
    $trips = [];
    while ($row = $result->fetch_assoc()) {
        $trips[] = $row;
    }
    $stmt->close();
}

require_once "helpers/ui.php";
?>
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>نتائج البحث - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <div class="header">
<h1>🔍 نتائج البحث الذكي (BlaBlaCar Style)</h1>
<h2><?= htmlspecialchars($from_city) ?> → <?= htmlspecialchars($to_city) ?></h2>
<p>مرتبة: وقت • تقييم • سعر | <a href="?from_city=<?= urlencode($from_city) ?>&to_city=<?= urlencode($to_city) ?>">مسح الفلاتر</a></p>
<form method="GET" style="margin-bottom:20px;">
<input type="hidden" name="from_city" value="<?= htmlspecialchars($from_city) ?>">
<input type="hidden" name="to_city" value="<?= htmlspecialchars($to_city) ?>">
<label>أقصى سعر: <input type="number" name="max_price" value="<?= $_GET['max_price'] ?? '' ?>" min="0"></label>
<label>أدنى تقييم: <input type="number" step="0.1" name="min_rating" value="<?= $_GET['min_rating'] ?? '' ?>" min="0" max="5"></label>
<button type="submit">فلتر</button>
</form>
</div>
        
        <?php if (isset($error)): ?>
            <div class="error"><?= $error ?></div>
        <?php elseif (empty($trips)): ?>
            <div class="error">لا توجد رحلات متاحة لهذا البحث</div>
        <?php else: ?>
            <table class="trips-table">
                <thead>
                    <tr>
                        <th>السائق</th>
                        <th>التقييم</th>
                        <th>السعر/مقعد</th>
                        <th>السيارة</th>
                        <th>وقت</th>
                        <th>مقاعد</th>
                        <th>الحجز</th>
                    </tr>
                </thead>
                <tbody>
                    <?php foreach ($trips as $trip): ?>
                    <tr>
                        <td><?= htmlspecialchars($trip['driver_name']) ?> <br><small><?= htmlspecialchars($trip['plate_number']) ?></small></td>
                        <td class="stars"><?= stars($trip['driver_rating']) ?> (<?= number_format($trip['driver_rating'], 1) ?>)</td>
                        <td><strong><?= number_format($trip['price_per_seat'], 0) ?> ر.ي</strong></td>
                        <td><?= htmlspecialchars($trip['vehicle_model'] ?? $trip['vehicle_info']) ?></td>
                        <td><?= date('h:i A', strtotime($trip['trip_time'])) ?></td>
                        <td class="seats-available"><?= $trip['seats'] ?></td>
                        <td>
                            <p><?= htmlspecialchars(substr($trip['description'], 0, 50)) ?>...</p>
                            <a href="book.php?trip_id=<?= $trip['id'] ?>" class="book-btn">حجز</a>
                        </td>
                    </tr>
                    <?php endforeach; ?>
                </tbody>
            </table>
        <?php endif; ?>
        
        <p style="text-align:center; margin-top:30px;">
            <a href="index.php">بحث جديد</a> | 
            <a href="register.php">تسجيل</a> | 
            <a href="login.php">دخول</a>
        </p>
    </div>
</body>
</html>
?>