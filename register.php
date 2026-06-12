<?php
require_once "security.php";
require_once "config/db.php";


$error = '';
$success = '';

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    verify_csrf_token();
    $name = trim($_POST['name']);
    $phone = trim($_POST['phone']);
    $password = trim($_POST['password']);
    $role = $_POST['role'];
    $plate_number = trim($_POST['plate_number'] ?? '');
    $city = trim($_POST['city'] ?? '');

    if (empty($name) || empty($phone) || empty($password)) {
        $error = "جميع الحقول (الاسم، رقم الهاتف، كلمة المرور) مطلوبة";
    } elseif ($role === 'driver' && empty($plate_number)) {
        $error = "لوحة السيارة مطلوبة للسائقين";
    } else {
        // Check if phone number already exists
        $check_stmt = $conn->prepare("SELECT id FROM users WHERE phone = ?");
        $check_stmt->bind_param("s", $phone);
        $check_stmt->execute();
        $result = $check_stmt->get_result();
        
        if ($result->fetch_assoc()) {
            $error = "رقم الهاتف هذا مسجل بالفعل";
        } else {
            // Hashing password
            $hashed_password = password_hash($password, PASSWORD_DEFAULT);

            // Prepared statement insert
            $stmt = $conn->prepare("INSERT INTO users (name, phone, password, role, plate_number, city) VALUES (?, ?, ?, ?, ?, ?)");
            $stmt->bind_param("ssssss", $name, $phone, $hashed_password, $role, $plate_number, $city);

            if ($stmt->execute()) {
                $success = "تم تسجيل الحساب بنجاح! يمكنك الآن تسجيل الدخول.";
                header("Refresh:2; url=login.php");
            } else {
                $error = "حدث خطأ أثناء التسجيل، يرجى المحاولة لاحقاً.";
            }
            $stmt->close();
        }
        $check_stmt->close();
    }
}
?>

<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>تسجيل حساب جديد - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <h2>إنشاء حساب جديد</h2>
        
        <?php if ($error): ?>
            <div class="error"><?= htmlspecialchars($error) ?></div>
        <?php endif; ?>
        
        <?php if ($success): ?>
            <div class="success"><?= htmlspecialchars($success) ?></div>
        <?php endif; ?>
        
        <form method="POST">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
            <input type="text" name="name" placeholder="الاسم الكامل" required value="<?= isset($_POST['name']) ? htmlspecialchars($_POST['name']) : '' ?>">
            <input type="text" name="phone" placeholder="رقم الهاتف (مثال: 777123456)" required value="<?= isset($_POST['phone']) ? htmlspecialchars($_POST['phone']) : '' ?>">
            <input type="password" name="password" placeholder="كلمة المرور" required>

            
            <label for="role">نوع الحساب:</label>
            <select name="role" id="role" style="padding:15px; border-radius:15px; border:3px solid #e9ecef; font-family:'Cairo', sans-serif; font-size:1.1em;" onchange="toggleDriverFields()">
                <option value="traveler" <?= (isset($_POST['role']) && $_POST['role'] === 'traveler') ? 'selected' : '' ?>>مسافر (حجز رحلات)</option>
                <option value="driver" <?= (isset($_POST['role']) && $_POST['role'] === 'driver') ? 'selected' : '' ?>>سائق (نشر رحلات)</option>
            </select>
            
            <div id="driver_fields" style="display: none; width: 100%; grid-column: span 3; gap: 20px;">
                <div style="display: flex; flex-direction: column; width: 100%; gap: 10px;">
                    <label for="plate">لوحة السيارة:</label>
                    <input type="text" name="plate_number" id="plate" placeholder="رقم اللوحة (مثال: 12345/اليمن)" value="<?= isset($_POST['plate_number']) ? htmlspecialchars($_POST['plate_number']) : '' ?>">
                </div>
            </div>
            
            <?php include 'locations.php'; ?>
            <label for="city">المدينة الحالية/المحافظة:</label>
            <input list="cities" type="text" name="city" id="city" placeholder="اختر أو اكتب المدينة..." value="<?= isset($_POST['city']) ? htmlspecialchars($_POST['city']) : '' ?>">
            <datalist id="cities">
                <?php foreach ($yemen_governorates as $c): ?>
                    <option value="<?= $c ?>">
                <?php endforeach; ?>
            </datalist>
            
            <button type="submit" style="grid-column: span 3; margin-top: 20px;">إنشاء الحساب</button>
        </form>
        
        <p style="text-align:center; margin-top:20px;">
            لديك حساب بالفعل؟ <a href="login.php">تسجيل الدخول</a>
        </p>
    </div>

    <script>
    function toggleDriverFields() {
        const roleSelect = document.getElementById('role');
        const driverFields = document.getElementById('driver_fields');
        if (roleSelect.value === 'driver') {
            driverFields.style.display = 'grid';
            document.getElementById('plate').setAttribute('required', 'required');
        } else {
            driverFields.style.display = 'none';
            document.getElementById('plate').removeAttribute('required');
        }
    }
    // Initialize onload
    window.onload = toggleDriverFields;
    </script>
</body>
</html>