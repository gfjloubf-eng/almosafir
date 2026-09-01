<?php
declare(strict_types=1);

header('Content-Type: text/html; charset=utf-8');

function is_writable_dir(string $dir): bool
{
    if (!is_dir($dir)) return false;
    if (is_writable($dir)) return true;
    $tmp = rtrim($dir, '/\\') . DIRECTORY_SEPARATOR . '.__bb_ai_write_test_' . bin2hex(random_bytes(4));
    $ok = @file_put_contents($tmp, 'ok') !== false;
    if ($ok) {
        @unlink($tmp);
    }
    return $ok;
}

function out_box(string $title, bool $ok, string $detail = ''): void
{
    $status = $ok ? 'PASS' : 'FAIL';
    $bg = $ok ? '#e9ffe9' : '#ffe9e9';
    $border = $ok ? '#55aa55' : '#cc4444';
    echo "<div style='background:{$bg};border:1px solid {$border};padding:12px;margin:10px 0;border-radius:8px;'>";
    echo "<div style='font-weight:800;'>" . htmlspecialchars($title, ENT_QUOTES, 'UTF-8') . " : {$status}</div>";
    if ($detail !== '') {
        echo "<div style='margin-top:6px; color:#222;'>" . htmlspecialchars($detail, ENT_QUOTES, 'UTF-8') . "</div>";
    }
    echo "</div>";
}

$phpOk = version_compare(PHP_VERSION, '7.4.0', '>=');
out_box('PHP Version (>= 7.4)', $phpOk, PHP_VERSION);

$mysqliLoaded = extension_loaded('mysqli');
out_box('mysqli extension', $mysqliLoaded, $mysqliLoaded ? 'loaded' : 'not loaded');

$sessionOk = session_status() !== PHP_SESSION_DISABLED;
out_box('session support', $sessionOk, $sessionOk ? 'enabled' : 'disabled');

// Required folders
$reqDirs = [
    __DIR__ . DIRECTORY_SEPARATOR . 'logs',
    __DIR__ . DIRECTORY_SEPARATOR . 'assets',
];
foreach ($reqDirs as $d) {
    out_box('Writable directory: ' . basename($d), is_writable_dir($d), $d);
}

// Database connection (read-only verification only)
$dbOk = false;
$dbDetail = '';
try {
    require_once __DIR__ . '/config/db.php';
    /** @var mysqli|null $conn */
    if (isset($conn) && $conn instanceof mysqli) {
        $dbOk = $conn->ping();
        $dbDetail = $dbOk ? 'connected' : 'ping failed';
    } else {
        $dbOk = false;
        $dbDetail = 'db.php did not provide mysqli connection';
    }
} catch (Throwable $e) {
    $dbOk = false;
    $dbDetail = $e->getMessage();
}
out_box('Database connectivity (read-only)', $dbOk, $dbDetail);

// Final status
$allOk = $phpOk && $mysqliLoaded && $sessionOk && $dbOk;
if ($allOk) {
    echo "<div style='padding:14px; background:#f2fbf2; border:1px solid #55aa55; border-radius:10px; margin-top:18px;'>";
    echo "<h2 style='margin:0;'>Installation check: READY</h2>";
    echo "<p style='margin:8px 0 0;'>No database changes were made.</p>";
    echo "</div>";
} else {
    echo "<div style='padding:14px; background:#fff3f3; border:1px solid #cc4444; border-radius:10px; margin-top:18px;'>";
    echo "<h2 style='margin:0;'>Installation check: NOT READY</h2>";
    echo "<p style='margin:8px 0 0;'>Fix the failing checks above. No database changes were made.</p>";
    echo "</div>";
}

?>

<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Install - Almosafir</title>
    <style>
        body{font-family:Arial,Helvetica,sans-serif; margin:20px;}
    </style>
</head>
<body>
    <div style='max-width:1000px;'>
        <a href='index.php'>← Back to app</a>
    </div>
</body>
</html>

