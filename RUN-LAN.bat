@echo off
chcp 65001 >nul
title AlMosafer — تشغيل للشبكة (الجوال يشارك)
cd /d "%~dp0"

echo ====================================================
echo    RUN-LAN — المنصة على جهازك + كل الجوالات بالشبكة
echo ====================================================
echo.

taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
timeout /t 1 /nobreak >nul

rem فتح منفذ الجدار الناري (يحتاج «تشغيل كمسؤول» مرة واحدة في العمر)
netsh advfirewall firewall add rule name="AlMosafer-5163" dir=in action=allow protocol=TCP localport=5163 >nul 2>&1

echo عنوانك الذي تكتبه في متصفح الجوال (نفس شبكة الواي فاي):
echo.
for /f "usebackq tokens=2 delims=:" %%a in (`ipconfig ^| findstr /C:"IPv4"`) do (
    for /f "tokens=* delims= " %%b in ("%%a") do echo        http://%%b:5163
)
echo.
echo نصيحة: اكتب العنوان الاول غالباً. عندما يفتح الموقع في الجوال:
echo   Chrome: قائمة النقاط =^> "اضافة الى الشاشة الرئيسية" = يصير تطبيقاً!
echo ====================================================
dotnet run --project src\AlMosafer.Web --urls "http://0.0.0.0:5163"
