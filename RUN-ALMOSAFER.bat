@echo off
chcp 65001 >nul
title AlMosafer - تشغيل المنصة (ASP.NET Core)
cd /d "%~dp0"

echo ====================================================
echo    RUN-ALMOSAFER - تشغيل المنصة على اللابتوب
echo ====================================================
echo.

rem أوقف أي نسخة قديمة شغالة (تمسك الملفات والمنفذ)
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
timeout /t 1 /nobreak >nul

rem اسحب آخر الكود — gc.auto=0 يمنع مشكلة قفل الحزم القديمة
echo [1/2] سحب آخر التحديثات...
git -c gc.auto=0 pull --ff-only >nul 2>&1

echo [2/2] تشغيل السيرفر — جداول قاعدة البيانات تنشأ تلقائيا
echo.
echo       انتظر ظهور هذا السطر بالاسفل:
echo       Now listening on: http://localhost:5163
echo.
echo       ثم افتح المتصفح على: http://localhost:5163
echo       لا تضغط Ctrl+C ولا تغلق النافذة اثناء الاستخدام.
echo ====================================================
dotnet run --project src\AlMosafer.Web --urls "http://localhost:5163"
pause
