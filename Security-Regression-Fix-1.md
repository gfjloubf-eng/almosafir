# Regression Fix Report #1

## Issue
`add_trip.php` threw PHP notice/warning:
- Undefined array key: `from_city`, `from_location`, `to_city`, `trip_time`, `seats`

## Root cause
The security patch introduced `verify_csrf_token()` and logout POST handling in a way that allowed the POST block to execute field reads even when the POST request was a logout submission (which does not include the trip fields).

## Fix applied (no unrelated file changes)
Modified only: `add_trip.php`

### Changes
- Inside the `if ($_SERVER["REQUEST_METHOD"] == "POST")` block:
  - Added a guard: if `isset($_POST['logout'])`, skip reading trip fields.
  - Replaced direct `$_POST[...]` reads with safe null-coalescing:
    - `$_POST['from_city'] ?? ''`, etc.
    - `$_POST['seats'] ?? 0`, `$_POST['price_per_seat'] ?? 0`

## Result
- Trip publish POST continues to validate and insert as before.
- Logout POST no longer triggers undefined index warnings.

## Files changed
- `add_trip.php`


