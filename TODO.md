- [ ] Analyze add_trip.php for defects (single POST handler, single CSRF validation path, single logout handler, avoid mixed logic).
- [ ] Update add_trip.php:
  - [ ] Fix malformed braces / control flow.
  - [ ] Ensure only one POST handler for trip submission (and logout handled in same POST handler but cleanly).
  - [ ] Ensure CSRF validation happens exactly once per POST request, before branching.
  - [ ] Ensure logout logic exists exactly once.
  - [ ] Avoid any GET-time warnings (e.g., $user_name existence).
  - [ ] Keep PHP syntax valid (passes php -l if available).
- [ ] Generate AddTrip Final Integrity Report in final response.

