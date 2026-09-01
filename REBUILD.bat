@echo off
chcp 65001 >nul
title AlMosafer — اعادة بناء شاملة من الصفر
cd /d "%~dp0"

echo ====================================================
echo    REBUILD — اعادة بناء المنصة من الصفر
echo    (الكود محفوظ ومختبَر — قاعدة بياناتك وبياناتك لا تُمس)
echo ====================================================
echo.

echo [0/4] اغلاق اي نسخة شغالة تقفل الملفات...
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo [1/4] ازالة مخلفات البناء القديمة بالكامل (bin وobj في كل الطبقات)...
for %%P in (Domain Application Infrastructure Web) do (
    if exist "src\AlMosafer.%%P\bin" rmdir /s /q "src\AlMosafer.%%P\bin"
    if exist "src\AlMosafer.%%P\obj" rmdir /s /q "src\AlMosafer.%%P\obj"
)
echo      تم التنظيف.

echo [2/4] سحب آخر نسخة موقعة من الكود...
git pull --ff-only
if errorlevel 1 (
    echo [X] فشل السحب — غالبا انقطاع نت. اعد تشغيل السكربت حين تعود الشبكة.
    pause
    exit /b 1
)

echo [3/4] بناء نظيف كامل (اول مرة تاخذ دقائق — شرب فنجانك الان :)...
dotnet build AlMosafer.slnx --nologo
if errorlevel 1 (
    echo.
    echo [X] فشل البناء! انسخ لي آخر الاسطر الحمراء كما هي.
    pause
    exit /b 1
)

echo [4/4] تشغيل المنصة على http://localhost:5163
echo     الهجرة الذهبية ستحدث ذاتيا الان — راقب سطر [AlMosafer] Database migrations
echo ----------------------------------------------------
dotnet run --project src\AlMosafer.Web
pause
