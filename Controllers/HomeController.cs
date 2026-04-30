using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using velcro.Models;

namespace velcro.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Tableau de bord";
        return View();
    }

    public IActionResult Board(Guid id)
    {
        ViewData["Title"] = "Board";
        ViewData["FullWidth"] = true;
        ViewBag.BoardId = id;
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
