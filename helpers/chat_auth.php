<?php
declare(strict_types=1);

require_once __DIR__ . '/../config/db.php';

/**
 * Authorization helper for chat conversations.
 *
 * Rules:
 * - Return TRUE only if user_id matches driver_id OR traveler_id
 *   for the given conversation.
 */
function can_access_conversation(int $conversation_id, int $user_id): bool
{
    global $conn;

    $sql = "SELECT driver_id, traveler_id FROM conversations WHERE id = ? LIMIT 1";
    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        return false;
    }

    $stmt->bind_param('i', $conversation_id);
    $stmt->execute();

    $res = $stmt->get_result();
    $row = $res ? $res->fetch_assoc() : null;
    $stmt->close();

    if (!$row) {
        return false;
    }

    $driver_id = (int)$row['driver_id'];
    $traveler_id = (int)$row['traveler_id'];

    return $user_id === $driver_id || $user_id === $traveler_id;
}

