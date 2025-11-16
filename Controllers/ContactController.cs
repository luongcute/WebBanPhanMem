using Microsoft.AspNetCore.Mvc;
using WebBanPhanMem.Models.ViewModels;
using System.Threading.Tasks;
using WebBanPhanMem.Data; // Cần thiết để sử dụng ApplicationDbContext
using WebBanPhanMem.Models; // Cần thiết để sử dụng ContactMessage
using WebBanPhanMem.Services; // Cần thiết để sử dụng IEmailSender (nếu bạn muốn gửi email)
using Microsoft.AspNetCore.Authorization;

namespace WebBanPhanMem.Controllers
{
    [AllowAnonymous]

    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender; // Thêm Email Sender nếu bạn muốn gửi email

        // ✅ 1. DEPENDENCY INJECTION CHO DBContext và EmailSender
        public ContactController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }


        // GET: /Contact/Index
        public IActionResult Index()
        {
            return View(new ContactVM());
        }

        // POST: /Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactVM vm)
        {
            if (ModelState.IsValid)
            {
                // BƯỚC 1: LƯU DỮ LIỆU VÀO DATABASE
                try
                {
                    var message = new ContactMessage
                    {
                        Name = vm.Name,
                        Email = vm.Email,
                        Subject = vm.Subject,
                        Message = vm.Message,
                        SentDate = DateTime.Now,
                        IsRead = false // Mặc định là chưa đọc khi mới gửi
                    };

                    _context.ContactMessages.Add(message);
                    await _context.SaveChangesAsync(); // <-- BƯỚC LƯU DỮ LIỆU QUAN TRỌNG

                    // BƯỚC 2: (TÙY CHỌN) GỬI EMAIL THÔNG BÁO TỚI ADMIN
                    // Nếu IEmailSender đã được cấu hình đúng:
                    // string adminEmail = "admin@yourdomain.com"; // Thay thế bằng email Admin thực tế
                    // string emailBody = $"Yêu cầu từ: {vm.Name} ({vm.Email})<br/>Nội dung: {vm.Message}";
                    // await _emailSender.SendEmailAsync(adminEmail, $"[LIÊN HỆ MỚI] {vm.Subject}", emailBody);


                    TempData["SuccessMessage"] = "Yêu cầu của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception )
                {
                    // Ghi log lỗi database nếu cần thiết
                    // Bạn có thể inject ILogger để ghi lại lỗi ở đây.
                    TempData["ErrorMessage"] = "Gửi yêu cầu thất bại do lỗi hệ thống (Database Error).";
                    return View("Index", vm);
                }
            }

            // Nếu Validation thất bại
            TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng kiểm tra lại thông tin đã nhập.";
            return View("Index", vm);
        }
    }
}