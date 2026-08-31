<?php
declare(strict_types=1);

// Simple production logger (file-based). 
// Never expose logs publicly. Keep logs outside public web root or block via .htaccess.

function logger_log(string $level, string $message, array $context = []): void
{
    $level = strtoupper($level);
    if (!in_array($level, ['INFO', 'WARNING', 'ERROR'], true)) {
        $level = 'INFO';
    }

    $baseDir = __DIR__ . '/../logs';
    if (!is_dir($baseDir)) {
        // Best effort: do nothing if logs dir missing
        return;
    }

    $date = date('Y-m-d');
    $file = $baseDir . DIRECTORY_SEPARATOR . 'app_' . $date . '.log';

    $contextStr = '';
    if (!empty($context)) {
        try {
            $contextStr = ' | context=' . json_encode($context, JSON_UNESCAPED_UNICODE);
        } catch (Throwable $e) {
            $contextStr = ' | context=[unserializable]';
        }
    }

    $line = sprintf(
        "%s [%s] %s%s\n",
        date('Y-m-d H:i:s'),
        $level,
        $message,
        $contextStr
    );

    @file_put_contents($file, $line, FILE_APPEND | LOCK_EX);
}

function log_info(string $message, array $context = []): void
{
    logger_log('INFO', $message, $context);
}

function log_warning(string $message, array $context = []): void
{
    logger_log('WARNING', $message, $context);
}

function log_error(string $message, array $context = []): void
{
    logger_log('ERROR', $message, $context);
}

