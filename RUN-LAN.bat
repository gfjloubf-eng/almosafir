@echo off
chcp 65001 >nul
title AlMosafer - تشغيل للجوال (الشبكة)
cd /d "%~dp0"

echo ====================================================
echo    RUN-LAN - المنصة على اللابتوب + الجوال معا
echo ====================================================
echo.

rem أوقف أي نسخة قديمة شغالة (تمسك المنفذ)
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1

rem تحرير المنفذ 5163 من أي عملية شبحية مهما كان اسمها — يحل «address already in use» نهائياً
for /f "tokens=5" %%p in ('netstat -aon ^| findstr ":5163" ^| findstr "LISTENING"') do taskkill /F /PID %%p >nul 2>&1
timeout /t 1 /nobreak >nul

rem فتح المنفذ (مرة واحدة في العمر كمسؤول؛ إن فشل بصمت فالقاعدة موجودة من قبل)
netsh advfirewall firewall add rule name="AlMosafer-5163" dir=in action=allow protocol=TCP localport=5163 >nul 2>&1

echo [1/2] عناوين المنصة على شبكتك — اكتب في الجوال العنوان الذي بجانبه سهم:
echo.
for /f "usebackq tokens=2 delims=:" %%a in (`ipconfig ^| findstr /C:"IPv4"`) do (
    for /f "tokens=* delims= " %%b in ("%%a") do (
        for /f "tokens=1-4 delims=." %%i in ("%%b") do (
            if "%%l"=="1" (
                echo        %%b  [محول افتراضي VMware - تجاهله]
            ) else (
                echo        http://%%b:5163   ^<^<^< استخدم هذا في الجوال
            )
        )
    )
)
echo.
echo [2/2] تشغيل السيرفر... انتظر ظهور السطر:
echo       Now listening on: http://0.0.0.0:5163
echo.
echo       ثم افتح الجوال على العنوان الذي بجانبه السهم اعلاه.
echo       لا تغلق هذه النافذة اثناء الاستخدام.
echo ====================================================
dotnet run --project src\AlMosafer.Web --urls "http://0.0.0.0:5163"
pause
