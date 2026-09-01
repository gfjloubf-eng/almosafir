@echo off
chcp 65001 >nul
title AlMosafer — تحديث المنصة من GitHub
cd /d "%~dp0"

echo ====================================================
echo    UPDATE-ALMOSAFER — جلب آخر التحديثات لجهازك
echo    (قاعدة بياناتك لا تُمس — الكود فقط)
echo ====================================================
echo.

rem الخطوة 0: اقتل النسخة الشبحية وعمليات البناء العالقة — هذه كانت تقدم لك كوداً قديماً
echo [0/4] اغلاق اي نسخة قديمة شغالة...
taskkill /F /IM AlMosafer.Web.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 2 /nobreak >nul

rem الخطوة 1: حماية اي تعديلات محلية عندك قبل السحب (لا نخسر شيئاً ابداً)
echo [1/4] فحص التعديلات المحلية...
git status --porcelain > "%TEMP%\alm_status.txt" 2>nul
for /f %%A in ('type "%TEMP%\alm_status.txt" ^| find /c /v ""') do set DIRTY=%%A
if not "%DIRTY%"=="0" (
    echo     لديك تعديلات محلية — ستحفظ جانباً بأمان ثم تعاد بعد التحديث:
    git stash push -m "almosafer-local-backup"
)

rem الخطوة 2: السحب الفعلي من GitHub — قلب العملية كلها
echo [2/4] سحب احدث نسخة من فرع main...
git pull --ff-only origin main
if errorlevel 1 (
    echo.
    echo [X] فشل السحب التلقائي. السبب الغالب: تسجيل الدخول لـ GitHub.
    echo     الاسهل: افتح برنامج GitHub Desktop ^> قائمة Repository ^> زر Pull origin
    echo     ثم شغّل هذا الملف مجدداً.
    pause
    exit /b 1
)

rem ارجاع تعديلاتك المحلية ان وجدت
if not "%DIRTY%"=="0" git stash pop >nul 2>&1

rem الخطوة 3: اعرض لك ماذا وصل بالضبط
echo.
echo [3/4] وصلنا الى الاصدار:
git log -1 --pretty=format:"    %%h — %%s"
echo.
echo.

rem الخطوة 4: بناء تحقق سريع (ان توفر SDK) — يثبت ان كل شيء يترجم سليماً
echo [4/4] فحص بناء سريع...
where dotnet >nul 2>&1
if errorlevel 1 (
    echo     SDK غير مثبت — تخطى الفحص؛ الانظام يبني عند التشغيل بملف RUN على اي حال
) else (
    dotnet build src\AlMosafer.Web --nologo -v q >nul 2>&1
    if errorlevel 1 (
        echo     [X] البناء فشل — شغّل REBUILD.bat ثم اخبرني بالخطأ الاحمر
    ) else (
        echo     البناء سليم. الكود يشتغل.
    )
)

echo.
echo ====================================================
echo    تم التحديث! شغّل RUN-ALMOSAFER.bat واضغط Ctrl+F5
echo    في المتصفح (تحديث قسري لازم — كاش المتصفح يخبيء
echo    الانماط القديمة!)
echo    ملاحظة: عند اول تشغيل سيضيف النظام عمود تذكرة
echo    جديد لقاعدتك تلقائياً — مقصود وامن.
echo ====================================================
pause
