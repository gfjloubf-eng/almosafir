<?php
// Central security helpers (CSRF + session hardening)

if (session_status() === PHP_SESSION_NONE) {
    // Configure cookie params early before session_start() in all calling scripts.
    $isSecure = (!empty($_SERVER['HTTPS']) && strtolower($_SERVER['HTTPS']) !== 'off');

    session_set_cookie_params([
        'lifetime' => 0,
        'path' => '/',
        'domain' => '',
        'secure' => $isSecure,
        'httponly' => true,
        // Use Lax to allow normal navigations while still protecting CSRF better than None.
        'samesite' => 'Lax',
    ]);

    session_start();
}

/**
 * Returns a CSRF token for the current session.
 */
function csrf_token(): string
{
    if (session_status() === PHP_SESSION_NONE) {
        session_start();
    }

    if (empty($_SESSION['csrf_token'])) {
        $_SESSION['csrf_token'] = bin2hex(random_bytes(32));
    }

    return $_SESSION['csrf_token'];
}

/**
 * Verifies CSRF token for POST/PUT/DELETE requests.
 * On failure: sends 403 and terminates.
 */
function verify_csrf_token(): void
{
    if (session_status() === PHP_SESSION_NONE) {
        http_response_code(403);
        exit('CSRF session not initialized');
    }

    // Support BOTH:
    // 1) Traditional form POST where csrf_token arrives in $_POST
    // 2) JSON body requests (e.g., chat.php) where csrf_token arrives in raw php://input
    $incoming = $_POST['csrf_token'] ?? null;

    if ($incoming === null) {
        $raw = file_get_contents('php://input');
        if (is_string($raw) && $raw !== '') {
            $decoded = json_decode($raw, true);
            if (is_array($decoded) && array_key_exists('csrf_token', $decoded)) {
                $incoming = $decoded['csrf_token'];
            } else {
                $incoming = '';
            }
        } else {
            $incoming = '';
        }
    }

    $expected = $_SESSION['csrf_token'] ?? '';

    $valid = is_string($incoming) && is_string($expected) && hash_equals($expected, $incoming);

    if (!$valid) {
        // Invalidate the token on failure to reduce brute-force value.
        unset($_SESSION['csrf_token']);

        http_response_code(403);
        exit('Invalid CSRF token');
    }
}

// Secure session hardening (called after session_start())
function apply_session_hardening(): void
{
    // Regenerate cookie flags are set via session_set_cookie_params() above.
    if (session_status() === PHP_SESSION_ACTIVE) {
        // Ensure session cookie params are strict-ish.
        $isSecure = (!empty($_SERVER['HTTPS']) && strtolower($_SERVER['HTTPS']) !== 'off');
        session_set_cookie_params([
            'lifetime' => 0,
            'path' => '/',
            'domain' => '',
            'secure' => $isSecure,
            'httponly' => true,
            'samesite' => 'Lax',
        ]);
    }
}

