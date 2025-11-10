using Microsoft.AspNetCore.Mvc;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    public class LicensesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
