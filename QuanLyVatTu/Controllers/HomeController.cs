using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

namespace QuanLyVatTu.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
