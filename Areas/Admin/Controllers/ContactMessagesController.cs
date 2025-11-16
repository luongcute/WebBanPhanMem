using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanPhanMem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. INDEX (Xem danh sách)
        // GET: Admin/ContactMessages
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            return View(messages);
        }

        // --- 2. DETAILS (Xem chi tiết & đánh dấu đã đọc)
        // GET: Admin/ContactMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null) return NotFound();

            // Đánh dấu là đã đọc nếu chưa đọc
            if (!message.IsRead)
            {
                message.IsRead = true;
                _context.Update(message);
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // --- 3. DELETE (Xóa tin nhắn)
        // POST: Admin/ContactMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin nhắn cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tin nhắn đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }

        // --- 4. MARK UNREAD (Đánh dấu là chưa đọc)
        // POST: Admin/ContactMessages/MarkUnread/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkUnread(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin nhắn.";
                return RedirectToAction(nameof(Index));
            }

            message.IsRead = false;
            _context.Update(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tin nhắn đã được đánh dấu là chưa đọc.";
            return RedirectToAction(nameof(Index));
        }
    }
}
