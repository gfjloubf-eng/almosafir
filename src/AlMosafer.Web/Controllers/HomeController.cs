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

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
