using AlMosafer.Application.Interfaces;
using AlMosafer.Infrastructure.Persistence;
using AlMosafer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// P40 «النبض الحي»: قناة SignalR الدائمة (جزء من إطار ASP.NET Core — بلا أي حزمة إضافية)
builder.Services.AddSignalR();

// Configure MySQL Database Connection via Pomelo EntityFrameworkCore MySql
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Fail fast on unexpanded env placeholders — ASP.NET Core does NOT expand "${VAR}" syntax in JSON config.
// Provide real values via environment variables (e.g. ConnectionStrings__DefaultConnection).
if (connectionString.Contains("${", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Connection string contains unexpanded placeholders ('${...}'). " +
        "Set the environment variable 'ConnectionStrings__DefaultConnection' with the real connection string.");
}

ServerVersion serverVersion;
try
{
    serverVersion = ServerVersion.AutoDetect(connectionString);
}
catch
{
    // Fallback to standard MariaDB / MySQL version for XAMPP
    serverVersion = new MariaDbServerVersion(new Version(10, 4, 28));
}

builder.Services.AddDbContext<AlMosaferDbContext>(options =>
    options.UseMySql(connectionString, serverVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(3, System.TimeSpan.FromSeconds(3), null)));

// Register Infrastructure Services
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IResourceOwnershipService, ResourceOwnershipService>();
builder.Services.AddScoped<IDbConnectionHealthService, DbConnectionHealthService>();
builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// P40: الخدمة الأصلية تُسجَّل باسمها ثم يُغلِّفها مُزخرِف البث اللحظي —
//      بذلك تبقى NotificationService حيادية عن الويب واختباراتها سليمة بلا لمس.
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationService>(sp =>
    new AlMosafer.Web.Services.RealtimeNotificationServiceDecorator(
        sp.GetRequiredService<NotificationService>(),
        sp.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<AlMosafer.Web.Hubs.AppHub>>(),
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AlMosafer.Web.Services.RealtimeNotificationServiceDecorator>>()));
// «النبض الحي» الموجة ٢: MessageService تُسجَّل باسمها ثم يغلّفها مزخرف الدردشة اللحظية —
//      بثّ conv-{id} + user-{id} بعد كل رسالة محفوظة، بلا أي لمس للخدمة الأصلية أو اختباراتها.
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<IMessageService>(sp =>
    new AlMosafer.Web.Services.RealtimeMessageServiceDecorator(
        sp.GetRequiredService<MessageService>(),
        sp.GetRequiredService<AlMosaferDbContext>(),
        sp.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<AlMosafer.Web.Hubs.AppHub>>(),
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AlMosafer.Web.Services.RealtimeMessageServiceDecorator>>()));
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<ILineService, LineService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AlMosafer.AuthToken";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Configure Rate Limiting for Abuse Protection
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("StrictLimiter", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

// بصلاحية المالك (2026-08-31): تطبيق هجرات EF تلقائياً عند الإقلاع —
// يستحدث أي جداول ناقصة (مثل Phase2 RouteLines) دون dotnet-ef أو خطوات يدوية.
// Migrate() ذكي: لا يمس شيئاً إن كانت القاعدة محدّثة؛ وإن كان MySQL مطفأً نسجل تحذيراً ونكمل (مثله مثل بذر الأدمن).
// وتعزيز (نفس اليوم): تحقق صريح من وجود route_lines + سطر الحل الجاهز إن غابت — حلٌّ أكيد لا صامت.
try
{
    using var migrateScope = app.Services.CreateScope();
    var migrateDb = migrateScope.ServiceProvider.GetRequiredService<AlMosaferDbContext>();

    // خطوة الأساس (baseline) — دليلها الميداني من كشف المالك: القاعدة أنشئت قديماً خارج EF
    // (أيام db_setup.sql) فسجل __EFMigrationsHistory فارغ، وMigrate يصطدم بـ«users موجودة»
    // قبل وصوله لهجرات اليوم. المعالجة القياسية: إن وُجد users وسجلٌ يجهله، نوسم الهجرة
    // الأولى كمُطبَّقة (INSERT IGNORE) ثم يكمل Migrate ما بعدها فقط.
    try
    {
        var baselineConnection = migrateDb.Database.GetDbConnection();
        baselineConnection.Open();

        using (var usersProbe = baselineConnection.CreateCommand())
        {
            usersProbe.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'users'";
            var usersTableExists = Convert.ToInt32(usersProbe.ExecuteScalar()) > 0;
            if (usersTableExists)
            {
                using var ensureHistory = baselineConnection.CreateCommand();
                ensureHistory.CommandText =
                    "CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (`MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL, `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL, CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)) CHARACTER SET=utf8mb4;";
                ensureHistory.ExecuteNonQuery();

                using var stampBaseline = baselineConnection.CreateCommand();
                stampBaseline.CommandText =
                    "INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ('20260828154624_InitialCreate', '9.0.0');";
                if (stampBaseline.ExecuteNonQuery() > 0)
                {
                    Console.WriteLine("[AlMosafer] قاعدة قديمة بلا سجل هجرات — وُسمت الهجرة الأولى أساساً مُطبَّقاً (baseline).");
                }
            }
        }
        baselineConnection.Close();
    }
    catch (Exception baselineEx)
    {
        Console.WriteLine($"[Warning] Baseline stamp skipped: {baselineEx.Message}");
    }

    var pendingMigrations = migrateDb.Database.GetPendingMigrations().ToList();
    if (pendingMigrations.Count > 0)
    {
        Console.WriteLine($"[AlMosafer] Applying {pendingMigrations.Count} pending migration(s): {string.Join(", ", pendingMigrations)}");
    }
    migrateDb.Database.Migrate();
    Console.WriteLine("[AlMosafer] Database migrations applied (or already up to date).");

    // فحص أكيد: هل جدول الخطوط موجود فعلاً على القرص؟
    try
    {
        var probeConnection = migrateDb.Database.GetDbConnection();
        probeConnection.Open();
        using var probeCommand = probeConnection.CreateCommand();
        probeCommand.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'route_lines'";
        var routeLinesExists = Convert.ToInt32(probeCommand.ExecuteScalar()) > 0;
        probeConnection.Close();
        Console.WriteLine(routeLinesExists
            ? "[AlMosafer] ✅ جدول route_lines موجود — صفحة شبكة الخطوط ستعمل."
            : "[AlMosafer] ❌ route_lines غير موجود! الحل: C:\\xampp\\mysql\\bin\\mysql.exe -u root mosafir_db < database\\Phase2_RouteLines.sql");
    }
    catch (Exception probeEx)
    {
        Console.WriteLine($"[Warning] Route-lines probe skipped: {probeEx.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Warning] DB migrate skipped: {ex.Message}");
    Console.WriteLine("[AlMosafer] ❌ الهجرات لم تُطبَّق! تأكد أن MySQL شغال (زر Start أخضر في XAMPP) ثم أعد التشغيل؛" +
        " أو نفّذ: C:\\xampp\\mysql\\bin\\mysql.exe -u root mosafir_db < database\\Phase2_RouteLines.sql");
}

// Seed Default Admin Account Securely on Startup
try
{
    // No plaintext default credentials are kept in source code.
    // Provide AdminSettings__Email / AdminSettings__Password via secrets or environment.
    // If not configured, seeding is skipped safely with a clear operational message.
    var adminEmail = builder.Configuration["AdminSettings:Email"];
    var adminPassword = builder.Configuration["AdminSettings:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        Console.WriteLine("[AlMosafer] Admin seed skipped: AdminSettings:Email/Password not configured. " +
            "Set AdminSettings__Email and AdminSettings__Password to provision the initial admin account.");
    }
    else
    {
        using (var scope = app.Services.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await authService.SeedDefaultAdminAsync(adminEmail, adminPassword);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Warning] DB Seed skipped: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapStaticAssets();
// إسناد صريح للملفات الجذرية للـPWA (manifest/sw.js) بأنواع MIME مؤكدة — معلول أي ثغرة تخطيط نوع
var staticContentProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
staticContentProvider.Mappings[".webmanifest"] = "application/manifest+json";
staticContentProvider.Mappings[".svg"] = "image/svg+xml";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticContentProvider });

// P40: نقطة النبض الحي — يحرسها [Authorize] على Hub نفسه (نفس كوكي الجلسة)
app.MapHub<AlMosafer.Web.Hubs.AppHub>("/hubs/app");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
