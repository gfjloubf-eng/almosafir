using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlMosafer.Web.Models;
using AlMosafer.Application.Interfaces;

namespace AlMosafer.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDbConnectionHealthService _healthService;
    private readonly IReportingService _reportingService;
    private readonly ILineService _lineService;

    public HomeController(ILogger<HomeController> logger, IDbConnectionHealthService healthService, IReportingService reportingService, ILineService lineService)
    {
        _logger = logger;
        _healthService = healthService;
        _reportingService = reportingService;
        _lineService = lineService;
    }

    public async Task<IActionResult> Index()
    {
        var health = await _healthService.CheckConnectionAsync();
        ViewBag.DbConnected = health.CanConnect;
        ViewBag.DbMessage = health.Message;
        ViewBag.DbName = health.DatabaseName;
        ViewBag.TopDrivers = await _reportingService.GetTopDriversAsync(4);
        // P43: اقتراحات المدن الحيّة من شبكة الخطوط المعتمدة (فارغة تلقائياً إن لم تُضف خطوط بعد)
        ViewBag.Cities = await _lineService.GetActiveCitiesAsync();
        // P49/UI: أرقام مصداقية خفيفة لشريط الإحصاءات في الرئيسية
        ViewBag.PublicStats = await _reportingService.GetPublicStatsAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400)]
    public IActionResult Robots()
    {
        // P47 «الباب العالمي»: توجيه الزواحف — المناطق الشخصية بعيدة عن الفهرسة + إشارة لخريطة الموقع
        var baseUri = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var txt = string.Join('\n',
            "User-agent: *",
            "Allow: /",
            "Disallow: /Admin",
            "Disallow: /Account/",
            "Disallow: /Bookings/",
            "Disallow: /Driver/",
            "Disallow: /Traveler/",
            "Disallow: /Conversations",
            "Disallow: /Notifications",
            "",
            $"Sitemap: {baseUri}/sitemap.xml");
        return Content(txt, "text/plain");
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Sitemap()
    {
        // P47: خريطة الصفحات العامة فقط — المساحات الشخصية لا تُفهرَس أصلاً
        var baseUri = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        string[] publicPaths = { "/", "/Trips", "/Lines", "/Trips/InternalLines",
            "/Account/Login", "/Account/RegisterTraveler", "/Account/RegisterDriver" };
        var xml = new System.Text.StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
        foreach (var p in publicPaths)
        {
            xml.Append("  <url><loc>").Append(baseUri).Append(p)
               .Append("</loc><changefreq>daily</changefreq></url>\n");
        }
        xml.Append("</urlset>");
        return Content(xml.ToString(), "application/xml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
