# Almosafir — Deployment Guide (Shared Hosting / cPanel)

This guide is written for shared hosting providers (cPanel, Hostinger, Namecheap, A2 Hosting, etc.). It focuses on environment configuration, verification, and production hardening without changing application logic.

## 1) Upload files
1. Upload the entire project folder contents to your hosting `public_html` (or equivalent) directory.
2. Ensure PHP files remain in the web root (same structure as on localhost): `index.php`, `login.php`, `register.php`, `api/`, `helpers/`, `config/`, etc.
3. Confirm `public/.htaccess` exists.

> Note: Some shared hosts do not allow `.htaccess` from a subfolder. If your host blocks it, you may need to place the rules into the *root* `.htaccess` file instead.

## 2) Create database
1. In your hosting control panel, create a new MySQL database.
2. Create a new DB user with privileges (SELECT/INSERT/UPDATE/DELETE/DDL as needed).

## 3) Import SQL
1. Create/obtain a SQL import process for `db_setup.sql`.
2. Import `db_setup.sql` into your new database.

## 4) Configure db.php
The application uses `config/db.php` and reads DB values from environment variables.

### On shared hosting:
Set these environment variables (exact UI depends on host):
- `DB_HOST`
- `DB_NAME`
- `DB_USER`
- `DB_PASS`
- `DB_PORT`

If your host cannot set environment variables:
- You may use `config/config.example.php` as a reference, but **do not modify db.php schema/logic**.
- Prefer using the host's environment-variable feature.

## 5) Run environment_check.php
1. Visit: `https://your-domain.com/environment_check.php`
2. All checks should be **PASS**.

If you see FAIL for database connectivity:
- verify `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASS`, `DB_PORT`.

## 6) Run install.php
1. Visit: `https://your-domain.com/install.php`
2. It performs read-only verification (no table creation/drop).

## 7) Verify login
1. Go to `login.php`
2. Verify you can log in for both:
   - traveler accounts
   - driver accounts

## 8) Verify trip creation (driver)
1. Log in as a driver
2. Create a trip via `add_trip.php`
3. Confirm the trip appears in listings.

## 9) Verify booking (traveler)
1. Log in as a traveler
2. Book a trip via `book.php?trip_id=...`
3. Confirm a booking + conversation is created.

## 10) Verify chat
1. Open the chat UI and load conversations
2. Send a message
3. Confirm messages are persisted and accessible

## Completion notes
- After successful deployment, consider removing or restricting access to `install.php`.
- Ensure `.env` files are not present on the server.

