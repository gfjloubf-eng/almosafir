<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');

require_once __DIR__ . '/../security.php';
require_once __DIR__ . '/../config/db.php';
require_once __DIR__ . '/../helpers/chat_auth.php';

function respond(int $status, array $payload): void
{
    http_response_code($status);
    echo json_encode($payload, JSON_UNESCAPED_UNICODE);
    exit;
}

function method_not_allowed(): void
{
    respond(405, ['success' => false, 'error' => 'METHOD_NOT_ALLOWED']);
}

if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    method_not_allowed();
}

session_start();
apply_session_hardening();

if (empty($_SESSION['user_id'])) {
    respond(401, ['success' => false, 'error' => 'NOT_AUTHENTICATED']);
}

$current_user = (int)$_SESSION['user_id'];

try {
    $sql = "
        SELECT 
            c.id AS conversation_id,
            c.trip_id,
            c.booking_id,
            c.last_message_at,
            CASE 
                WHEN c.driver_id = ? THEN u_traveler.name
                ELSE u_driver.name
            END AS other_user_name,
            SUM(CASE WHEN m.is_read = 0 AND m.sender_id <> ? THEN 1 ELSE 0 END) AS unread_count
        FROM conversations c
        JOIN users u_driver ON u_driver.id = c.driver_id
        JOIN users u_traveler ON u_traveler.id = c.traveler_id
        LEFT JOIN messages m ON m.conversation_id = c.id
        WHERE (c.driver_id = ? OR c.traveler_id = ?)
        GROUP BY c.id, c.trip_id, c.booking_id, c.last_message_at
        ORDER BY c.last_message_at DESC, c.id DESC
    ";

    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        respond(500, ['success' => false, 'error' => 'DB_PREPARE_FAILED']);
    }

    $stmt->bind_param('iiii', $current_user, $current_user, $current_user, $current_user);
    $stmt->execute();
    $res = $stmt->get_result();

    $conversations = [];
    while ($row = $res->fetch_assoc()) {
        $conversations[] = [
            'conversation_id' => (int)$row['conversation_id'],
            'trip_id' => (int)$row['trip_id'],
            'booking_id' => $row['booking_id'] === null ? null : (int)$row['booking_id'],
            'other_user_name' => (string)$row['other_user_name'],
            'last_message_at' => $row['last_message_at'],
            'unread_count' => (int)$row['unread_count'],
        ];
    }

    $stmt->close();

    respond(200, ['success' => true, 'data' => ['conversations' => $conversations]]);
} catch (Throwable $e) {
    respond(500, ['success' => false, 'error' => 'INTERNAL_SERVER_ERROR']);
}

