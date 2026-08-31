@echo off
chcp 65001 >nul
title AlMosafer - تشغيل موحد
cd /d "%~dp0"

echo ====================================================
echo    RUN-ALMOSAFER — سكربت التشغيل الموحد للمنصة
echo ====================================================
echo.

rem خطوة 0: سحب آخر الكود (بأفضل جهد — لا يوقف السكربت عند فشله)
git pull --ff-only >nul 2>&1

set "MYSQL=C:\xampp\mysql\bin\mysql.exe"
if not exist "%MYSQL%" (
    echo [X] لم اعثر على MySQL في C:\xampp\mysql\bin
    echo     تأكد ان XAMPP مثبت في C:\xampp
    pause
    exit /b 1
)

echo [1/3] تطبيق جداول قاعدة البيانات (خطر الخطوط - Phase 2)...
"%MYSQL%" -u root --port=3306 mosafir_db < "database\Phase2_RouteLines.sql"
if errorlevel 1 (
    echo.
    echo [X] فشل الاتصال بقاعدة البيانات!
    echo     الحل: افتح لوحة XAMPP واضغط Start بجانب MySQL (يصبح اخضر)
    echo     ثم شغّل هذا السكربت من جديد.
    pause
    exit /b 1
)

echo.
echo [2/3] التحقق من الجداول الجديدة وسجل الهجرات:
"%MYSQL%" -u root --port=3306 mosafir_db -e "SHOW TABLES LIKE '%%line%%'; SELECT MigrationId FROM __EFMigrationsHistory;"

echo.
echo [3/3] تشغيل المنصة على http://localhost:5163
echo     اترك هذه النافذة مفتوحة مادمت تستخدم الموقع.
echo     لايقافها لاحقاً: Ctrl+C
echo ----------------------------------------------------
dotnet run --project src\AlMosafer.Web
pause
