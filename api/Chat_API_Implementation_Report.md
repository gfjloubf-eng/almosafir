# Chat API Implementation Report (v1.2-chat-api)

## Files Created
- `helpers/chat_auth.php`
- `api/conversations.php`
- `api/messages.php`
- `api/send_message.php`
- `api/mark_read.php`
- `api/Chat_API_Implementation_Report.md`

## Files Modified
- None

## Authorization Flow
- `helpers/chat_auth.php`
  - Implements `can_access_conversation(int $conversation_id, int $user_id): bool`
  - Uses prepared statement:
    - `SELECT driver_id, traveler_id FROM conversations WHERE id = ?`
  - Returns `TRUE` only if:
    - `user_id === driver_id` OR `user_id === traveler_id`

- `api/*` endpoints:
  1. `session_start()` and `apply_session_hardening()`
  2. Verify logged-in user via `$_SESSION['user_id']`
     - Missing user → **401** `NOT_AUTHENTICATED`
  3. For conversation-scoped endpoints, verify conversation exists → **404** `CONVERSATION_NOT_FOUND`
  4. Enforce ownership with `can_access_conversation()`
     - Not allowed → **403** `ACCESS_DENIED`

## API Endpoints
### `api/conversations.php` (GET)
- Returns all conversations for the current user.
- Driver views: `driver_id = current_user`
- Traveler views: `traveler_id = current_user`
- Ordering: `last_message_at DESC`
- Output fields:
  - `conversation_id`
  - `trip_id`
  - `booking_id`
  - `other_user_name`
  - `last_message_at`
  - `unread_count`

### `api/messages.php` (GET)
- Input (query): `conversation_id`
- Authorization:
  - Ensures conversation exists and `can_access_conversation()` passes
- Loads latest 50 messages:
  - Ordering: `created_at ASC`
  - Limit: 50
- Output fields:
  - `id`, `sender_id`, `message`, `is_read`, `created_at`

### `api/send_message.php` (POST)
- Requires:
  - Session
  - CSRF validation via `verify_csrf_token()`
- Input (POST): `conversation_id`, `message`
- Validations:
  - `message` required and `trim(message) != ''`
  - max length: 2000 chars (UTF-8 aware)
  - conversation exists
- Behavior:
  - Inserts into `messages`
  - Updates `conversations.last_message_at = NOW()`
- Output:
  - `{ success: true, data: { message_id } }`

### `api/mark_read.php` (POST)
- Requires:
  - Session
  - CSRF validation via `verify_csrf_token()`
- Input (POST): `conversation_id`
- Behavior:
  - Marks messages as read ONLY when `sender_id != current_user`
  - Updates `messages.is_read = 1`
- Output:
  - `{ success: true, data: { updated } }`

## Security Validation
For every implemented endpoint:
- `session_start()`
- Authenticated user check (`$_SESSION['user_id']`)
- Prepared statements only
- Authorization checks via `can_access_conversation()`
- JSON-only responses
- No HTML output
- Proper HTTP status codes enforced:
  - **200** success
  - **400** validation errors
  - **401** not authenticated
  - **403** unauthorized
  - **404** conversation not found
  - **405** method not allowed
  - **500** internal server error

## Database Indexes
### Required by spec
- Add missing index:
  - `messages(conversation_id, is_read)`

### Current status
- This environment prevented reliable inspection via MySQL CLI.
- The remaining index addition is therefore still pending in DB migration/schema.

### Expected SQL (do not remove existing indexes)
```sql
CREATE INDEX IF NOT EXISTS idx_messages_conversation_is_read
ON messages (conversation_id, is_read);
```

## Testing Checklist
- [ ] Driver sees only conversations where `driver_id = current_user`
- [ ] Traveler sees only conversations where `traveler_id = current_user`
- [ ] Unauthorized conversation access returns **403** `ACCESS_DENIED`
- [ ] `api/messages.php` returns **404** when conversation id does not exist
- [ ] `api/send_message.php`:
  - [ ] rejects empty message (**400** `MESSAGE_EMPTY`)
  - [ ] rejects too-long message (**400** `MESSAGE_TOO_LONG`)
  - [ ] requires CSRF (fails with **403** from `verify_csrf_token()`)
  - [ ] updates `conversations.last_message_at`
- [ ] `api/mark_read.php`:
  - [ ] marks only other-party messages read
- [ ] All responses are JSON and status codes match the spec


