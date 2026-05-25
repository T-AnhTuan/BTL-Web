using Microsoft.AspNetCore.Mvc;

namespace QuanLyVatTu.Controllers
{
    public class TaiKhoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
