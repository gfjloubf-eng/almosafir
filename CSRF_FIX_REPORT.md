# CSRF FIX REPORT (Chat JSON Support)

## Files modified
1. `security.php`
   - Updated `verify_csrf_token()` to support both:
     - Traditional form POST (`$_POST['csrf_token']`)
     - JSON requests (`php://input` decoded JSON and read `csrf_token`)

## Validation method (implementation details)
`verify_csrf_token()` now:
1. Checks whether `$_POST['csrf_token']` exists.
2. If not, reads raw request body from `php://input`.
3. Attempts `json_decode(..., true)`.
4. If decoded payload contains `csrf_token`, uses that value as the incoming token.
5. Compares against `$_SESSION['csrf_token']` using `hash_equals()`.
6. On failure:
   - unsets `$_SESSION['csrf_token']`
   - returns **403** with `Invalid CSRF token`

## Backward compatibility verification
### Traditional form POST (existing app flows)
- `login.php` uses `verify_csrf_token()` after a standard HTML form POST.
- `register.php` uses `verify_csrf_token()` after a standard HTML form POST.
- `book.php` uses `verify_csrf_token()` after a standard HTML form POST.
- `add_trip.php` uses `verify_csrf_token()` after a standard HTML form POST.
- `profile.php` uses `verify_csrf_token()` after a standard HTML form POST.

All of these send CSRF tokens via `application/x-www-form-urlencoded` / normal POST, therefore `$_POST['csrf_token']` is present and behavior remains unchanged.

### JSON request body (chat)
- `chat.php` sends CSRF token in JSON body to:
  - `api/send_message.php`
  - `api/mark_read.php`
- With this fix, JSON CSRF tokens are extracted from `php://input` and validated.

## Test results (static + integration expectation)
Because PHP CLI execution is unavailable in the provided terminal environment, tests are reported as static validation of request/response compatibility:
- ✅ JSON CSRF extraction path implemented (fixes root issue).
- ✅ Form POST CSRF path preserved (backward compatibility).
- ✅ HTTP status code on CSRF failure remains **403**.

Expected outcome after deployment:
- `api/send_message.php` passes CSRF when called from `chat.php`.
- `api/mark_read.php` passes CSRF when called from `chat.php`.
- Existing login/register/booking/trip creation CSRF continues to work.

