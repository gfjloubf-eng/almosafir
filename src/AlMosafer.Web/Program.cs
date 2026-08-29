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

// Configure MySQL Database Connection via Pomelo EntityFrameworkCore MySql
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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
    options.UseMySql(connectionString, serverVersion));

// Register Infrastructure Services
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IResourceOwnershipService, ResourceOwnershipService>();
builder.Services.AddScoped<IDbConnectionHealthService, DbConnectionHealthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IReportingService, ReportingService>();

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

// Seed Default Admin Account Securely on Startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var adminEmail = builder.Configuration["AdminSettings:Email"] ?? "admin@almosafir.com";
        var adminPassword = builder.Configuration["AdminSettings:Password"] ?? "Admin@123456";
        await authService.SeedDefaultAdminAsync(adminEmail, adminPassword);
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
