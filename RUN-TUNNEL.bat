@echo off
chcp 65001 >nul
title AlMosafer ALL-IN-ONE
cd /d "%~dp0"

echo ====================================================
echo    RUN-TUNNEL - بندقية المسافر الكبيرة
echo    نافذة واحدة = نظام + نفق + رابطك العام
echo ====================================================
echo.

rem ===== 1) السيرف في نافذة جانبية =====
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
timeout /t 1 /nobreak >nul
echo [1/3] تشغيل النظام في نافذة جديدة اسمها AlMosafer-Server
start "AlMosafer-Server" cmd /k "cd /d %~dp0 && dotnet run --project src\AlMosafer.Web --urls http://0.0.0.0:5163"
echo انتظر في النافذة الجانبية هذا السطر: Now listening on http://0.0.0.0:5163
timeout /t 20 /nobreak >nul

rem ===== 2) cloudflared - ينزل مرة واحدة في العمر =====
set CFDIR=%USERPROFILE%\Downloads
if exist "%CFDIR%\cloudflared.exe" goto :havecf
echo [2/3] تنزيل cloudflared لأول مرة 50MB تقريباً دقيقتان
curl -L -o "%CFDIR%\cloudflared.exe" https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe
:havecf

rem ===== 3) النفق وطباعة الرابط =====
echo.
echo ======================================================
echo [3/3] فتح نفق HTTPS العام
echo       بعد لحظات يظهر رابطك بين علامتي خط:
echo       https://xxxxx-yyyy-zzzz.trycloudflare.com
echo.
echo       1. انسخ الرابط الظاهر بالماوس ثم Ctrl+C
echo       2. الصقه في متصفح الجوال - الجوال معه انترنت فقط
echo       3. ثم في جوالك النقاط الثلاث =^> تثبيت التطبيق
echo.
echo       مهم: اترك هذه النافذة + نافذة Server شغالين
echo       لا تضغط Ctrl+C ابدا فيهما
echo ======================================================
echo.
"%CFDIR%\cloudflared.exe" tunnel --url http://localhost:5163
