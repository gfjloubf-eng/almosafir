# System Patterns

- **High-level layout**: MVC-like PHP (pages/*.php handle routes, config/db.php, api/*.php JSON).
- **Data flow**: Session auth -> DB queries -> HTML/JSON responses.
- **Patterns to follow**: Prepared PDO stmts, error logging, Arabic UTF8, responsive CSS.
- **Patterns to avoid**: Raw SQL inserts, inline JS, hard-coded data.
