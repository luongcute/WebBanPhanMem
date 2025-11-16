using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using WebBanPhanMem.Models.ViewModels;
using WebBanPhanMem.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebBanPhanMem.Controllers
{
    [AllowAnonymous]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================= GIỎ HÀNG ========================

        public IActionResult Index()
        {
            return View(GetCartItems());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var cart = GetCartItems();
            var existing = cart.FirstOrDefault(x => x.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });

            SaveCartItems(cart);
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // ======================= THANH TOÁN ========================

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            // 🔐 BẮT BUỘC ĐĂNG NHẬP
            if (!User.Identity!.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thanh toán!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _context.Users.FindAsync(userId);

            // Load product data
            var productIds = cart.Select(x => x.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var orderItems = cart.Select(item =>
            {
                var prod = products.First(p => p.Id == item.ProductId);
                return new OrderItem
                {
                    ProductId = item.ProductId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Product = prod
                };
            }).ToList();

            var vm = new CheckoutVM
            {
                UserId = userId,
                CustomerName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                CartItems = orderItems,
                TotalAmount = cart.Sum(x => x.Total)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            // 🔐 BẮT BUỘC ĐĂNG NHẬP
            if (!User.Identity!.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            // Recalculate total
            model.TotalAmount = cart.Sum(c => c.Total);

            if (!ModelState.IsValid)
            {
                // Load products to re-display View
                var ids = cart.Select(c => c.ProductId).ToList();
                var products = await _context.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

                model.CartItems = cart.Select(c =>
                {
                    var prod = products.First(p => p.Id == c.ProductId);
                    return new OrderItem
                    {
                        ProductId = c.ProductId,
                        Price = c.Price,
                        Quantity = c.Quantity,
                        Product = prod
                    };
                }).ToList();

                return View("Checkout", model);
            }

            var order = new Order
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CustomerName = model.CustomerName,
                CustomerEmail = model.Email,
                CreatedAt = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Pending",
                Status = "Pending",
                TotalAmount = model.TotalAmount,
                TransactionCode = $"HD{DateTime.Now:yyyyMMddHHmmss}",
                Items = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                var prod = await _context.Products.FindAsync(item.ProductId);

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    RequiresLicenseKey = prod?.HasLicenseKey ?? false
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            ClearCart();

            TempData["SuccessMessage"] = "Đặt hàng thành công! Mã đơn hàng: " + order.TransactionCode;
            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? NotFound() : View(order);
        }

        // ======================= TIỆN ÍCH ========================

        private List<CartItem> GetCartItems()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();
        }

        private void SaveCartItems(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove("Cart");
        }

        public IActionResult GetCartCount()
        {
            var cart = GetCartItems();
            return Json(new { count = cart.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                item.Quantity = quantity;

            SaveCartItems(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
                cart.Remove(item);

            SaveCartItems(cart);
            return RedirectToAction("Index");
        }
    }
}
