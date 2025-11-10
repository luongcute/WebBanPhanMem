using Microsoft.AspNetCore.Mvc;

namespace WebBanPhanMem.Areas.Admin.Models
{
    public class ReportViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
