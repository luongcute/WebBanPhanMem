using Microsoft.AspNetCore.Mvc;

namespace WebBanPhanMem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
