<?php
require_once "security.php";
require_once "config/db.php";
require_once "helpers/ui.php";


if (!isset($_SESSION['user_id']) || $_SESSION['role'] != 'traveler') {
    header("Location: login.php?role=traveler");
    exit;
}

$trip_id = (int)($_GET['trip_id'] ?? 0);
$success = $error = '';

if ($trip_id <= 0) {
    $error = "رحلة غير صالحة";
} elseif ($_SERVER["REQUEST_METHOD"] == "POST") {
    verify_csrf_token();
    $seats_booked = (int)($_POST['seats_booked'] ?? 1);
    $traveler_id = $_SESSION['user_id'];
    
    if ($seats_booked <= 0) {
        $error = "عدد المقاعد يجب أن يكون أكبر من صفر";
} else {
        // Concurrency-safe booking: atomic seat reservation + transaction
        $conn->begin_transaction();
        try {
            // 1) Atomic decrement only if seats are sufficient (prevents race)
            $reserve_stmt = $conn->prepare(
                "UPDATE trips "+
                "SET seats = seats - ? "+
                "WHERE id = ? AND status = 'open' AND seats >= ?"
            );
            $reserve_stmt->bind_param("iii", $seats_booked, $trip_id, $seats_booked);
            $reserve_stmt->execute();
            $affected = $reserve_stmt->affected_rows;
            $reserve_stmt->close();

            if ($affected !== 1) {
                throw new Exception("NO_SEATS");
            }

            // 2) Insert booking record
            $book_stmt = $conn->prepare(
                "INSERT INTO bookings (trip_id, traveler_id, seats_booked) VALUES (?, ?, ?)"
            );
            $book_stmt->bind_param("iii", $trip_id, $traveler_id, $seats_booked);
            $book_stmt->execute();
            $booking_id = $conn->insert_id;
            $book_stmt->close();

            // 3) Create conversation owned by this booking (transaction-safe)
            // Prevent duplicates using UNIQUE(conversations.booking_id)
            // Fetch trip + driver for fast authorization fields.
            $driver_id = null;
            $trip_check_stmt = $conn->prepare("SELECT driver_id FROM trips WHERE id = ? AND status = 'open'");
            $trip_check_stmt->bind_param("i", $trip_id);
            $trip_check_stmt->execute();
            $trip_res = $trip_check_stmt->get_result();
            if (!$trip_res) {
                $trip_check_stmt->close();
                throw new Exception("TRIP_LOOKUP_FAILED");
            }
            $row = $trip_res->fetch_assoc();
            $trip_check_stmt->close();
            if (!$row) {
                throw new Exception("TRIP_NOT_FOUND");
            }
            $driver_id = (int)$row['driver_id'];

            $conv_stmt = $conn->prepare(
                "INSERT INTO conversations (booking_id, trip_id, driver_id, traveler_id, created_at, last_message_at)
                 VALUES (?, ?, ?, ?, NOW(), NULL)
                 ON DUPLICATE KEY UPDATE
                   trip_id = VALUES(trip_id),
                   driver_id = VALUES(driver_id),
                   traveler_id = VALUES(traveler_id),
                   last_message_at = conversations.last_message_at"
            );
            $conv_stmt->bind_param("iiii", $booking_id, $trip_id, $driver_id, $traveler_id);
            if (!$conv_stmt->execute()) {
                $err = $conn->error;
                $conv_stmt->close();
                throw new Exception("CONVERSATION_CREATE_FAILED: " . $err);
            }
            $conv_stmt->close();

            $conn->commit();
            $success = "تم حجز $seats_booked مقعد بنجاح!";
        } catch (Exception $e) {
            $conn->rollback();
            if (($e->getMessage() ?? '') === 'NO_SEATS') {
                $error = "لا توجد مقاعد كافية أو الرحلة غير متاحة";
            } else {
                $error = "حدث خطأ في الحجز";
            }
        }
    }
} else {
    // Fetch trip info
    $trip_stmt = $conn->prepare("SELECT t.*, u.name AS driver_name FROM trips t JOIN users u ON t.driver_id = u.id WHERE t.id = ?");
    $trip_stmt->bind_param("i", $trip_id);
    $trip_stmt->execute();
    $trip = $trip_stmt->get_result()->fetch_assoc();
    $trip_stmt->close();
    
    if (!$trip) {
        $error = "رحلة غير موجودة";
    }
}
?>

<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>حجز رحلة - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <h2>حجز الرحلة</h2>
        <p>مرحباً <?= $_SESSION['user_name'] ?> (مسافر)</p>
        
        <?php if ($error): ?>
            <div class="error"><?= $error ?></div>
            <p><a href="search.php?from_city=<?= urlencode($_GET['from_city'] ?? '') ?>&to_city=<?= urlencode($_GET['to_city'] ?? '') ?>">← رحلات أخرى</a></p>
        <?php elseif ($success): ?>
            <div class="success"><?= $success ?></div>
            <p><a href="index.php">← الرئيسية</a></p>
        <?php elseif (isset($trip)): ?>
            <div style="background:#f8f9fa; padding:20px; border-radius:10px; margin-bottom:20px;">
                <h3>تفاصيل الرحلة</h3>
                <p><strong>السائق:</strong> <?= htmlspecialchars($trip['driver_name']) ?></p>
                <p><strong>التقييم:</strong> <?= stars($trip['driver_rating'] ?? 0) ?> (<?= number_format($trip['driver_rating'] ?? 0,1) ?>)</p>
                <p><strong>من:</strong> <?= htmlspecialchars($trip['from_city'] . ' - ' . $trip['from_location']) ?></p>
                <p><strong>إلى:</strong> <?= htmlspecialchars($trip['to_city']) ?></p>
                <p><strong>الوقت:</strong> <?= date('Y-m-d h:i A', strtotime($trip['trip_time'])) ?></p>
                <p><strong>المقاعد المتاحة:</strong> <span class="seats-available"><?= $trip['seats'] ?></span></p>
            </div>
            
            <form method="POST">
                <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
                <label>عدد المقاعد المطلوبة:</label>
                <input type="number" name="seats_booked" min="1" max="<?= $trip['seats'] ?>" value="1" required>
                <button type="submit">تأكيد الحجز</button>
            </form>
        <?php endif; ?>
        
        <p style="text-align:center; margin-top:30px;">
            <a href="index.php">الرئيسية</a>
            | <a href="login.php?logout=1">خروج</a>
        </p>
    </div>
</body>
</html>

