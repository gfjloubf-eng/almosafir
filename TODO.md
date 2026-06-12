# TODO.md

- [ ] (DB migration) Apply final non-destructive migration SQL for conversations/messages constraints
  - [ ] Add `booking_id` + `last_message_at` to `conversations`
  - [ ] Enforce `UNIQUE(booking_id)`
  - [ ] Add `FOREIGN KEY (booking_id) REFERENCES bookings(id)` with ON DELETE CASCADE
  - [ ] Ensure `trip_id`, `driver_id`, `traveler_id` FKs remain
  - [ ] Ensure `messages.conversation_id` and `messages.sender_id` FKs remain
- [ ] (App migration) Update `book.php` to create conversation after successful booking
  - [ ] Populate `booking_id, trip_id, driver_id, traveler_id`
  - [ ] Prevent duplicates for same booking (idempotent insert)
  - [ ] Keep transaction integrity (conversation creation inside booking transaction)
- [ ] (Chat authorization) Add/adjust endpoints (or future) to authorize by conversation ownership using `booking_id` and cross-check `trip_id/driver_id/traveler_id`
- [ ] (Testing) Run booking race test and a manual authorization test

