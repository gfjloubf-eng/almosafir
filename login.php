<?php
require_once "security.php";
require_once "config/db.php";

$error = '';
$success = '';

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    verify_csrf_token();
    $phone = trim($_POST['phone']);
    $password = $_POST['password'];

    if (empty($phone) || empty($password)) {
        $error = "الرجاء إدخال رقم الهاتف وكلمة المرور";
    } else {
        $stmt = $conn->prepare("SELECT id, name, password, role FROM users WHERE phone = ?");
        $stmt->bind_param("s", $phone);
        $stmt->execute();
        $result = $stmt->get_result();
        
        if ($user = $result->fetch_assoc()) {
                if (password_verify($password, $user['password'])) {
                // Prevent session fixation
                session_regenerate_id(true);
                $_SESSION['user_id'] = $user['id'];
                $_SESSION['user_name'] = $user['name'];
                $_SESSION['role'] = $user['role'];
                
                $success = "تم تسجيل الدخول بنجاح، " . $user['name'];

                
                // Redirect based on role
                if ($user['role'] == 'driver') {
                    header("Refresh:1; url=add_trip.php");
                } else {
                    header("Refresh:1; url=index.php");
                }
            } else {
                $error = "كلمة المرور غير صحيحة";
            }
        } else {
            $error = "رقم الهاتف غير موجود";
        }
        $stmt->close();
    }
}
?>

<?php
// Logout support (GET) - only used when explicitly requested by existing UI.
// Note: CSRF for logout was not requested for this file, so it remains unchanged.
if (isset($_GET['logout'])) {
    session_destroy();
    header("Location: login.php");
    exit;
}
?>

<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>تسجيل الدخول - المسافر</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <div class="container">
        <h2>تسجيل الدخول</h2>
        
        <?php if ($error): ?>
            <div class="error"><?= $error ?></div>
        <?php endif; ?>
        
        <?php if ($success): ?>
            <div class="success"><?= $success ?></div>
        <?php endif; ?>
        
        <form method="POST">
            <input type="hidden" name="csrf_token" value="<?= htmlspecialchars(csrf_token()) ?>">
            <input type="text" name="phone" placeholder="رقم الهاتف" required value="<?= isset($_POST['phone']) ? $_POST['phone'] : '' ?>">
            <input type="password" name="password" placeholder="كلمة المرور" required>
            <button type="submit">دخول</button>
        </form>
        
        <p style="text-align:center; margin-top:20px;">
            ليس لديك حساب؟ <a href="register.php">تسجيل جديد</a>
        </p>
    </div>
</body>
</html>

