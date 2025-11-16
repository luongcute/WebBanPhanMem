using Microsoft.AspNetCore.Mvc;
using WebBanPhanMem.Data;

namespace WebBanPhanMem.Areas.Admin.Components
{
    public class ContactAndOrderStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ContactAndOrderStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pendingContacts = await Task.FromResult(
                _context.ContactMessages.Count(c => c.IsRead == false)
            );

            var pendingOrders = await Task.FromResult(
                _context.Orders.Count(o => o.Status == "Pending")
            );

            return View(new AdminStatsVM
            {
                PendingContacts = pendingContacts,
                PendingOrders = pendingOrders
            });
        }
    }

    public class AdminStatsVM
    {
        public int PendingContacts { get; set; }
        public int PendingOrders { get; set; }
    }
}
