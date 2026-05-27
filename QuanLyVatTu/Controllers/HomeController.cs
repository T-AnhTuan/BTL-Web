using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
