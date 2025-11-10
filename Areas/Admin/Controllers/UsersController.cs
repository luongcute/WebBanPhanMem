using Microsoft.AspNetCore.Mvc;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
