<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Almosafir | المسافر</title>
    <link rel="icon" href="assets/images/favicon/favicon-32.png">
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <div class="header">
            <div class="brand" style="display:flex; align-items:center; gap:12px; justify-content:center;">
                <img src="assets/images/logo/logo-main.png" alt="Almosafir" style="height:48px; width:auto;" loading="lazy">
                <h1 style="margin:0;">🚀 المسافر</h1>
            </div>
            <div class="developer">
                تطوير احترافي <strong>عمار عادل المصوعي</strong> | 📞 <span class="phone">712275038</span>
            </div>
        </div>
        
        <?php include 'locations.php'; ?>
        <form action="search.php" method="GET">
            <input list="from_cities" type="text" name="from_city" placeholder="من المحافظة... (تعز)" required>
            <datalist id="from_cities">
                <?php foreach ($yemen_governorates as $city): ?>
                    <option value="<?= $city ?>">
                <?php endforeach; ?>
            </datalist>
            <input list="locations" type="text" name="from_location" placeholder="القرية/المديرية... (الحوبان)">
            <datalist id="locations">
                <?php foreach ($yemen_locations as $loc): ?>
                    <option value="<?= $loc ?>">
                <?php endforeach; ?>
            </datalist>
            <input list="to_cities" type="text" name="to_city" placeholder="الوجهة... (عدن)" required>
            <datalist id="to_cities">
                <?php foreach ($yemen_governorates as $city): ?>
                    <option value="<?= $city ?>">
                <?php endforeach; ?>
            </datalist>
            <button type="submit">🔍 ابحث رحلات اليمن</button>
        </form>

        <div style="text-align:center; margin-top:30px; display:flex; justify-content:center; gap:30px;">
            <a href="register.php" class="book-btn" style="padding:15px 30px; font-size:1.1em;">📝 تسجيل جديد</a>
            <a href="login.php" class="book-btn" style="padding:15px 30px; font-size:1.1em; background:linear-gradient(135deg, #667eea, #764ba2);">🔐 دخول</a>
            <a href="ai_chat.php" class="book-btn" style="padding:15px 30px; font-size:1.1em; background:linear-gradient(135deg, #9b59b6, #8e44ad);">🤖 AI مساعد</a>
        </div>

        <div class="footer">
            <p>نظام حجز رحلات ذكي بتقنية AI • ترتيب الرحلات حسب الوقت + تقييم السائق ⭐</p>
            <p><strong>Developed by Ammar Adel Al-Mas'oudi | 712275038</strong></p>
        </div>
    </div>
</body>
</html>
