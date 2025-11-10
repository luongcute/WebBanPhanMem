using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any()) return RedirectToAction("Index", "Cart");

            var order = new Order
            {
                UserId = User.Identity?.Name ?? "guest",
                CreatedAt = DateTime.Now,
                TotalAmount = cart.Sum(x => x.Total),
                Items = cart.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
