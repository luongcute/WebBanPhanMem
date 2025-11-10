using Microsoft.AspNetCore.Mvc;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem { ProductId = product.Id, Name = product.Name, Price = product.Price, Quantity = 1 });

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            cart?.RemoveAll(c => c.ProductId == productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
    }

    // Model tạm cho CartItem
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
