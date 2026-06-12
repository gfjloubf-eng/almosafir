<?php
// Mock Stripe/PayPal integration for BlaBlaCar-style payments
// In production, use real Stripe.js
session_start();
require_once "config/db.php";

if (!isset($_SESSION['user_id']) || $_SESSION['role'] != 'traveler') {
    header("Location: login.php");
    exit;
}

$trip_id = (int)($_GET['trip_id'] ?? 0);
$seats_booked = (int)($_POST['seats_booked'] ?? 1);

if ($trip_id <= 0) {
    die("رحلة غير صالحة");
}

// Fetch trip
$stmt = $conn->prepare("SELECT * FROM trips WHERE id = ? AND status = 'open'");
$stmt->bind_param("i", $trip_id);
$stmt->execute();
$trip = $stmt->get_result()->fetch_assoc();
$stmt->close();

if (!$trip || $trip['seats'] < $seats_booked) {
    die("لا مقاعد متاحة");
}

$total_amount = $trip['price_per_seat'] * $seats_booked;

// Mock payment success (real: Stripe)
$payment_status = 'paid'; // Simulate
$payment_id = 'mock_' . time();

if ($_POST['pay'] ?? false) {
    // Create booking & payment
    $conn->begin_transaction();
    try {
        $booking_stmt = $conn->prepare("INSERT INTO bookings (trip_id, traveler_id, seats_booked) VALUES (?, ?, ?)");
        $booking_stmt->bind_param("iii", $trip_id, $_SESSION['user_id'], $seats_booked);
        $booking_stmt->execute();
        $booking_id = $conn->insert_id;
        $booking_stmt->close();
        
        $payment_stmt = $conn->prepare("INSERT INTO payments (booking_id, amount, status, payment_id) VALUES (?, ?, ?, ?)");
        $payment_stmt->bind_param("idss", $booking_id, $total_amount, $payment_status, $payment_id);
        $payment_stmt->execute();
        $payment_stmt->close();
        
        // Reduce seats
        $update_stmt = $conn->prepare("UPDATE trips SET seats = seats - ? WHERE id = ?");
        $update_stmt->bind_param("ii", $seats_booked, $trip_id);
        $update_stmt->execute();
        $update_stmt->close();
        
        $conn->commit();
        header("Location: complete.php?booking_id=$booking_id");
        exit;
    } catch (Exception $e) {
        $conn->rollback();
        die("خطأ في الدفع");
    }
}
?>
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>الدفع - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <h2>💳 تأكيد الدفع</h2>
        <div style="background:#f8f9fa; padding:30px; border-radius:20px; margin-bottom:30px;">
            <h3>رحلة: <?= htmlspecialchars($trip['from_city'].' → '.$trip['to_city']) ?></h3>
            <p>السائق: <?= htmlspecialchars($trip['driver_name'] ?? 'غير معروف') ?></p>
            <p>المقاعد: <?= $seats_booked ?></p>
            <p><strong>المجموع: <?= number_format($total_amount, 0) ?> ر.ي</strong></p>
        </div>
        
        <form method="POST">
            <input type="hidden" name="seats_booked" value="<?= $seats_booked ?>">
            <button type="submit" name="pay" class="book-btn" style="font-size:1.3em; padding:20px 40px;">💳 دفع الآن (محاكاة)</button>
            <p style="color:#666; margin-top:10px;">* دفع آمن بـ Stripe/PayPal (محاكاة حالياً)</p>
        </form>
        <a href="book.php?trip_id=<?= $trip_id ?>" style="display:block; text-align:center; margin-top:20px;">← تعديل الحجز</a>
    </div>
</body>
</html>
