@echo off
chcp 65001 >nul
title AlMosafer ALL-IN-ONE — النظام + نفق عام بضغطة واحدة
cd /d "%~dp0"

echo ====================================================
echo    RUN-TUNNEL — بندقية المسافر الكبيرة
echo    نافذة واحدة =^> نظام + نفق + رابطك العام
echo ====================================================
echo.

rem ===== 1) إعادة السيرف نظيفاً في نافذة جانبية =====
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
timeout /t 1 /nobreak >nul
echo [1/3] تشغيل النظام في نافذة «AlMosafer-Server»...
start "AlMosafer-Server" cmd /k "cd /d %~dp0 && dotnet run --project src\AlMosafer.Web --urls http://0.0.0.0:5163"
echo         (انتظر في النافذة الجانبية: Now listening on: http://0.0.0.0:5163)
timeout /t 18 /nobreak >nul

rem ===== 2) cloudflared — تنزيل مرة واحدة في العمر =====
set CFDIR=%USERPROFILE%\Downloads
if not exist "%CFDIR%\cloudflared.exe" (
    echo [2/3] تنزيل cloudflared لأول مرة (~50MB دقيقتان)...
    curl -L -o "%CFDIR%\cloudflared.exe" https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe
) else (
    echo [2/3] cloudflared موجود بالفعل — جاهز
)

rem ===== 3) فتح النفق وطباعة الرابط =====
echo.
echo ======================================================
echo [3/3] فتح نفق HTTPS العام...
echo       بعد لحظات يظهر صطرك بين علامتي ^| :
echo       https://xxxxx-yyyy-zzzz.trycloudflare.com
echo.
echo       1. انسخ بماوسك الرابط الظاهر (Ctrl+C)
echo       2. الصقه في متصفح الجوال — الجوال معه إنترنت فقط
echo       3. ثم في جوالك: النقاط الثلاث =^> تثبيت التطبيق
echo.
echo       اترك هذه النافذة + نافذة Server شغالين دائماً!
echo ======================================================
echo.
"%CFDIR%\cloudflared.exe" tunnel --url http://localhost:5163
