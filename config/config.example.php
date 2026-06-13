<?php
/**
 * config/config.example.php
 *
 * Production deployment example configuration.
 *
 * NOTE:
 * - This project currently uses config/db.php with environment variables.
 * - Keep DB_HOST/DB_NAME/DB_USER/DB_PASS compatible with existing db.php.
 * - You can copy this file to config/config.php (optional) or just set env vars.
 */

return [
    // Database
    'DB_HOST' => 'localhost',
    'DB_NAME' => 'mosafir_db',
    'DB_USER' => 'root',
    'DB_PASSWORD' => '',
    'DB_PORT' => 3308,

    // Application
    'APP_ENV' => 'production',

    // Public base URL (used by future features; safe placeholder)
    'APP_URL' => 'https://example.com',
];

