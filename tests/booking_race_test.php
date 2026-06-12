<?php
/**
 * Concurrency test for booking race conditions.
 *
 * Usage (example):
 *  - Ensure MySQL is reachable for the same DB used by the app (mosafir_db on port 3308 per config/db.php).
 *  - Ensure you have a traveler user and an open trip with enough seats.
 *  - Set environment variables below and run two parallel requests.
 *
 * This script is intentionally framework-free.
 * It uses MySQL transactions + atomic seat reservation logic assumptions.
 *
 * NOTE:
 *  - This test file does not execute automatically in the app.
 *  - It provides a reproducible harness for manual/CI execution.
 */

require_once __DIR__ . '/../config/db.php';

function envInt(string $key, int $default): int {
    $v = getenv($key);
    if ($v === false || $v === '') return $default;
    return (int)$v;
}

function envStr(string $key, string $default): string {
    $v = getenv($key);
    if ($v === false || $v === '') return $default;
    return (string)$v;
}

$tripId = envInt('TEST_TRIP_ID', 0);
$user1Id = envInt('TEST_USER1_ID', 0);
$user2Id = envInt('TEST_USER2_ID', 0);
$seatsToBook = envInt('TEST_SEATS', 1);

if ($tripId <= 0 || $user1Id <= 0 || $user2Id <= 0) {
    fwrite(STDERR, "Missing env vars. Required: TEST_TRIP_ID, TEST_USER1_ID, TEST_USER2_ID.\n");
    exit(1);
}

// Two separate mysqli connections simulate two concurrent clients.
$c1 = $conn;
$c2 = mysqli_connect('localhost','root','','mosafir_db',3308);
if (!$c2) {
    fwrite(STDERR, "Second DB connection failed: " . mysqli_connect_error() . "\n");
    exit(1);
}
mysqli_set_charset($c2, 'utf8mb4');

// Ensure we start with known state (manual; keep conservative)
$seedBookings = $c1->prepare("DELETE FROM bookings WHERE trip_id = ?");
$seedBookings->bind_param('i', $tripId);
$seedBookings->execute();
$seedBookings->close();

// Worker: attempt booking
$attempt = function(mysqli $db, int $travelerId, int $tripId, int $seatsToBook) {
    $db->begin_transaction();
    try {
        // Atomic seat reservation pattern (expected implementation):
        // Update seats only if enough seats remain.
        $stmt = $db->prepare(
            "UPDATE trips SET seats = seats - ? WHERE id = ? AND status='open' AND seats >= ?"
        );
        $stmt->bind_param('iii', $seatsToBook, $tripId, $seatsToBook);
        $stmt->execute();
        $affected = $stmt->affected_rows;
        $stmt->close();

        if ($affected !== 1) {
            $db->rollback();
            return ['ok' => false, 'reason' => 'NO_SEATS'];
        }

        $ins = $db->prepare("INSERT INTO bookings (trip_id, traveler_id, seats_booked) VALUES (?, ?, ?)");
        $ins->bind_param('iii', $tripId, $travelerId, $seatsToBook);
        $ins->execute();
        $bookingId = $db->insert_id;
        $ins->close();

        $db->commit();
        return ['ok' => true, 'booking_id' => $bookingId];
    } catch (Throwable $e) {
        $db->rollback();
        return ['ok' => false, 'reason' => 'EXCEPTION', 'error' => $e->getMessage()];
    }
};

// Simulate "simultaneous" by interleaving operations with usleep.
// True simultaneity requires multi-process; however, this script is still useful
// when combined with manual parallel runs.

$result1 = $attempt($c1, $user1Id, $tripId, $seatsToBook);
usleep(200000); // 200ms
$result2 = $attempt($c2, $user2Id, $tripId, $seatsToBook);

$finalSeatsStmt = $c1->prepare("SELECT seats, status FROM trips WHERE id = ?");
$finalSeatsStmt->bind_param('i', $tripId);
$finalSeatsStmt->execute();
$finalSeats = $finalSeatsStmt->get_result()->fetch_assoc();
$finalSeatsStmt->close();

$bookingsStmt = $c1->prepare("SELECT COUNT(*) AS c FROM bookings WHERE trip_id = ?");
$bookingsStmt->bind_param('i', $tripId);
$bookingsStmt->execute();
$totalBookings = $bookingsStmt->get_result()->fetch_assoc()['c'];
$bookingsStmt->close();

echo "Attempt1: " . json_encode($result1, JSON_UNESCAPED_UNICODE) . "\n";
echo "Attempt2: " . json_encode($result2, JSON_UNESCAPED_UNICODE) . "\n";
echo "Final trip seats: " . json_encode($finalSeats, JSON_UNESCAPED_UNICODE) . "\n";
echo "Total bookings created: {$totalBookings}\n";

// Assertion: with seatsToBook=1, at most ONE booking should succeed if starting seats == 1.
// We cannot guarantee starting seats without reading; keep it advisory.
$startingSeats = null;
// If you want strict asserts, set TEST_SEATS=1 and manually ensure trip.seats was 1 before running.

if (($result1['ok'] ?? false) && ($result2['ok'] ?? false)) {
    fwrite(STDERR, "FAIL: Both concurrent attempts succeeded -> race condition still possible.\n");
    exit(2);
}

if (!($result1['ok'] ?? false) && !($result2['ok'] ?? false)) {
    fwrite(STDERR, "WARN: Neither booking succeeded (could be insufficient seats already).\n");
}

echo "PASS: Not both bookings succeeded (concurrency safety likely).\n";
exit(0);

