<?php
declare(strict_types=1);

function out_row(string $label, bool $ok, string $detail = ''): void
{
    $status = $ok ? 'PASS' : 'FAIL';
    $color = $ok ? 'green' : 'red';
    $detailEsc = $detail !== '' ? ' - ' . htmlspecialchars($detail, ENT_QUOTES, 'UTF-8') : '';
    echo "<tr><td>" . htmlspecialchars($label, ENT_QUOTES, 'UTF-8') . "</td><td style='color:{$color};font-weight:700;'>{$status}</td><td>{$detailEsc}</td></tr>\n";
}

function is_writable_dir(string $dir): bool
{
    if (!is_dir($dir)) return false;
    if (is_writable($dir)) return true;

    // best-effort: attempt to create temp file
    $tmp = rtrim($dir, '/\\') . DIRECTORY_SEPARATOR . '.__bb_ai_write_test_' . bin2hex(random_bytes(4));
    $ok = @file_put_contents($tmp, 'ok') !== false;
    if ($ok) {
        @unlink($tmp);
    }
    return $ok;
}

function test_db_connect(mysqli $conn): bool
{
    return $conn->ping();
}

header('Content-Type: text/html; charset=utf-8');
?><!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Environment Check - Almosafir</title>
    <style>
        body{font-family:Arial,Helvetica,sans-serif; margin:20px;}
        table{border-collapse:collapse; width:100%; max-width:900px;}
        th,td{border:1px solid #ddd; padding:10px; vertical-align:top;}
        th{background:#f5f5f5; text-align:left;}
    </style>
</head>
<body>
    <h1>Almosafir Environment Checker</h1>
    <p>Results indicate whether this host is ready for production deployment.</p>

    <table>
        <thead>
            <tr>
                <th>Check</th>
                <th>Status</th>
                <th>Details</th>
            </tr>
        </thead>
        <tbody>
<?php
// PHP version
$phpOk = version_compare(PHP_VERSION, '7.4.0', '>=');
out_row('PHP Version (>= 7.4)', $phpOk, PHP_VERSION);

// mysqli extension
$mysqliOk = extension_loaded('mysqli');
out_row('mysqli extension', $mysqliOk, $mysqliOk ? 'loaded' : 'not loaded');

// session support
$sessionOk = session_status() !== PHP_SESSION_DISABLED;
out_row('session support', $sessionOk, $sessionOk ? 'enabled' : 'disabled');

// file upload support
$uploadOk = is_array($_FILES);
out_row('file upload support (via PHP SAPI)', $uploadOk, '$_FILES is ' . ($uploadOk ? 'available' : 'not available (may depend on request)'));

// writable directories
$writableDirs = [
    __DIR__ . DIRECTORY_SEPARATOR . 'logs',
];
$allWritable = true;
foreach ($writableDirs as $dir) {
    $ok = is_writable_dir($dir);
    $allWritable = $allWritable && $ok;
    out_row('Writable directory: ' . $dir, $ok);
}

// database connectivity
$dbOk = false;
$dbDetail = '';
try {
    require_once __DIR__ . '/config/db.php';
    /** @var mysqli|null $conn */
    if (isset($conn) && $conn instanceof mysqli) {
        $dbOk = test_db_connect($conn);
        $dbDetail = $dbOk ? 'connected' : 'ping failed';
    } else {
        $dbOk = false;
        $dbDetail = 'No mysqli connection object';
    }
} catch (Throwable $e) {
    $dbOk = false;
    $dbDetail = $e->getMessage();
}
out_row('database connectivity', $dbOk, $dbDetail);

?>
        </tbody>
    </table>

    <hr />
    <p><strong>Summary:</strong> <?php echo ($phpOk && $mysqliOk && $sessionOk && $allWritable && $dbOk) ? 'PASS' : 'FAIL'; ?></p>
</body>
</html>

