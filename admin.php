<?php
session_start();
require_once "config/db.php";

if (!isset($_SESSION['user_id'])) {
    die('Access denied');
}

// Trip completion: simulate start/finish
if (isset($_POST['finish_trip'])) {
    $trip_id = (int)$_POST['trip_id'];
    $stmt = $conn->prepare("UPDATE trips SET status = 'finished' WHERE id = ? AND driver_id = ?");
    $stmt->bind_param("ii", $trip_id, $_SESSION['user_id']);
    $stmt->execute();
    $stmt->close();
}

// Get driver trips
$stmt = $conn->prepare("SELECT * FROM trips WHERE driver_id = ? ORDER BY trip_time DESC");
$stmt->bind_param("i", $_SESSION['user_id']);
$stmt->execute();
$trips = $stmt->get_result()->fetch_all(MYSQLI_ASSOC);
?>

<!DOCTYPE html>
<html lang="ar">
<head>
    <title>لوحة السائق</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
<div class="container">
<h2>رحلاتك <?= $_SESSION['user_name'] ?></h2>
<table class="trips-table">
<tr><th>ID</th><th>من</th><th>إلى</th><th>وقت</th><th>مقاعد</th><th>حالة</th><th>إجراء</th></tr>
<?php foreach ($trips as $t): ?>
<tr>
<td><?= $t['id'] ?></td>
<td><?= $t['from_city'].' '.$t['from_location'] ?></td>
<td><?= $t['to_city'] ?></td>
<td><?= $t['trip_time'] ?></td>
<td><?= $t['seats'] ?></td>
<td><?= $t['status'] ?></td>
<td>
<?php if ($t['status'] == 'open'): ?>
<form method="post" style="display:inline;">
<input type="hidden" name="trip_id" value="<?= $t['id'] ?>">
<button type="submit" name="start_trip" class="book-btn" style="background:#007bff;">انطلاق</button>
</form>
<?php endif; ?>
<?php if ($t['status'] == 'started'): ?>
<form method="post" style="display:inline;">
<input type="hidden" name="trip_id" value="<?= $t['id'] ?>">
<button type="submit" name="finish_trip" class="book-btn" style="background:#28a745;">اكتمال</button>
</form>
<?php endif; ?>
</td>
</tr>
<?php endforeach; ?>
</table>
<a href="index.php">الرئيسية</a>
</div>
</body>
</html>

