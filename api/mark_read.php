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

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    respond(405, ['success' => false, 'error' => 'METHOD_NOT_ALLOWED']);
}

session_start();
apply_session_hardening();

if (empty($_SESSION['user_id'])) {
    respond(401, ['success' => false, 'error' => 'NOT_AUTHENTICATED']);
}

$current_user = (int)$_SESSION['user_id'];

verify_csrf_token();

$conversation_id = (int)($_POST['conversation_id'] ?? 0);
if ($conversation_id <= 0) {
    respond(400, ['success' => false, 'error' => 'INVALID_CONVERSATION_ID']);
}

try {
    $sql = "SELECT id FROM conversations WHERE id = ? LIMIT 1";
    $stmt = $conn->prepare($sql);
    $stmt->bind_param('i', $conversation_id);
    $stmt->execute();
    $res = $stmt->get_result();
    $exists = $res && $res->fetch_assoc();
    $stmt->close();

    if (!$exists) {
        respond(404, ['success' => false, 'error' => 'CONVERSATION_NOT_FOUND']);
    }

    if (!can_access_conversation($conversation_id, $current_user)) {
        respond(403, ['success' => false, 'error' => 'ACCESS_DENIED']);
    }

    $sql = "UPDATE messages SET is_read = 1 WHERE conversation_id = ? AND sender_id <> ?";
    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        respond(500, ['success' => false, 'error' => 'DB_PREPARE_FAILED']);
    }

    $stmt->bind_param('ii', $conversation_id, $current_user);
    $stmt->execute();

    $affected = $stmt->affected_rows;
    $stmt->close();

    respond(200, ['success' => true, 'data' => ['updated' => (int)$affected]]);
} catch (Throwable $e) {
    respond(500, ['success' => false, 'error' => 'INTERNAL_SERVER_ERROR']);
}

