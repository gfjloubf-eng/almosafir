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

// CSRF required
verify_csrf_token();

$conversation_id = (int)($_POST['conversation_id'] ?? 0);
$message = (string)($_POST['message'] ?? '');
$message = trim($message);

if ($conversation_id <= 0) {
    respond(400, ['success' => false, 'error' => 'INVALID_CONVERSATION_ID']);
}

if ($message === '') {
    respond(400, ['success' => false, 'error' => 'MESSAGE_EMPTY']);
}

if (mb_strlen($message, 'UTF-8') > 2000) {
    respond(400, ['success' => false, 'error' => 'MESSAGE_TOO_LONG']);
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

    $conn->begin_transaction();

    $insertSql = "INSERT INTO messages (conversation_id, sender_id, message, is_read, created_at) VALUES (?, ?, ?, 0, NOW())";
    $stmt = $conn->prepare($insertSql);
    if (!$stmt) {
        throw new RuntimeException('DB_PREPARE_FAILED');
    }

    $stmt->bind_param('iis', $conversation_id, $current_user, $message);
    $ok = $stmt->execute();
    if (!$ok) {
        $err = $stmt->error;
        $stmt->close();
        throw new RuntimeException('MESSAGE_INSERT_FAILED: ' . $err);
    }
    $message_id = $conn->insert_id;
    $stmt->close();

    $updateSql = "UPDATE conversations SET last_message_at = NOW() WHERE id = ?";
    $stmt = $conn->prepare($updateSql);
    $stmt->bind_param('i', $conversation_id);
    $stmt->execute();
    $stmt->close();

    $conn->commit();

    respond(200, ['success' => true, 'data' => ['message_id' => (int)$message_id]]);
} catch (Throwable $e) {
    if ($conn && $conn->errno) {
        $conn->rollback();
    }
    respond(500, ['success' => false, 'error' => 'INTERNAL_SERVER_ERROR']);
}

