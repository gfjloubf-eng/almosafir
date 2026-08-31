using AlMosafer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlMosafer.Web.Controllers;

public class LinesController : Controller
{
    private readonly ILineService _lineService;

    public LinesController(ILineService lineService)
    {
        _lineService = lineService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city = null)
    {
        ViewBag.Cities = await _lineService.GetActiveCitiesAsync();
        ViewBag.SelectedCity = city;
        var lines = await _lineService.GetActiveLinesAsync(city);
        return View(lines);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var line = await _lineService.GetLineDetailsAsync(id);
        if (line == null)
        {
            return NotFound();
        }
        return View(line);
    }
}
