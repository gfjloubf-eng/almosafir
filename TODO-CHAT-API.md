# TODO - Chat API Layer (v1.2-chat-api)

## Progress
- [x] Create `helpers/chat_auth.php`
- [x] Create `api/conversations.php`
- [x] Create `api/messages.php`
- [x] Create `api/send_message.php`
- [x] Create `api/mark_read.php`
- [x] Enforce strict JSON responses + HTTP status codes
- [ ] Add missing DB index: `messages(conversation_id, is_read)`
  - Expected SQL:
    - `CREATE INDEX IF NOT EXISTS idx_messages_conversation_is_read ON messages (conversation_id, is_read);`
- [ ] Add/execute manual API test cases (curl/postman)
- [ ] Produce final report (already generated as `api/Chat_API_Implementation_Report.md`)

