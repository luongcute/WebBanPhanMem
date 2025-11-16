using Microsoft.EntityFrameworkCore;
using WebBanPhanMem.Data;
using WebBanPhanMem.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace WebBanPhanMem.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<LicenseService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        // =======================================================
        // CRUD (TRIỂN KHAI HOÀN CHỈNH ILicenseService)
        // =======================================================

        public async Task<List<LicenseKey>> GetAllAsync()
        {
            return await _context.LicenseKeys.ToListAsync();
        }

        public async Task<LicenseKey?> GetByIdAsync(int id)
        {
            return await _context.LicenseKeys.FindAsync(id);
        }

        public async Task CreateAsync(LicenseKey key)
        {
            _context.LicenseKeys.Add(key);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LicenseKey key)
        {
            _context.LicenseKeys.Update(key);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var key = await _context.LicenseKeys.FindAsync(id);
            if (key != null)
            {
                _context.LicenseKeys.Remove(key);
                await _context.SaveChangesAsync();
            }
        }

        // =======================================================
        // GÁN LICENSE
        // =======================================================

        public async Task<List<LicenseKey>> AssignKeysToOrderAsync(int orderId)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null || order.Items == null || !order.Items.Any())
                {
                    _logger.LogWarning($"Order ID {orderId} không tồn tại hoặc không có sản phẩm.");
                    return new List<LicenseKey>();
                }

                var existingKeys = await _context.LicenseKeys
                                .Where(k => k.OrderId == orderId)
                                .ToListAsync();

                if (existingKeys.Any())
                {
                    _logger.LogWarning($"Order ID {orderId} đã có {existingKeys.Count} key được gán trước đó, bỏ qua việc gán lại.");
                    return existingKeys;
                }

                var assignedKeys = new List<LicenseKey>();

                foreach (var item in order.Items.Where(i => i.Product != null && i.Product.HasLicenseKey))
                {
                    var availableKeys = await _context.LicenseKeys
                        .Where(lk => lk.ProductId == item.ProductId && !lk.IsUsed && lk.OrderId == null)
                        .Take(item.Quantity)
                        .ToListAsync();

                    if (availableKeys.Count < item.Quantity)
                    {
                        // Ghi log cảnh báo nhưng vẫn tiếp tục thực thi
                        _logger.LogWarning(
                             $"CẢNH BÁO: Không đủ key cho Product ID {item.ProductId}. Yêu cầu: {item.Quantity}, Chỉ có: {availableKeys.Count} key được gán."
                        );
                    }

                    foreach (var key in availableKeys)
                    {
                        key.OrderId = orderId;
                        key.IsUsed = true;
                        key.ActivatedDate = DateTime.Now;
                        _context.LicenseKeys.Update(key);
                        assignedKeys.Add(key);
                    }
                }

                await _context.SaveChangesAsync();
                transaction.Complete();

                _logger.LogInformation($"Gán thành công {assignedKeys.Count} key cho Order ID {orderId}.");
                return assignedKeys;
            }
        }

        // =======================================================
        // GỬI LICENSE QUA EMAIL (Đã sửa lỗi biến 'order')
        // =======================================================

        public async Task SendKeysByEmailAsync(
            string toEmail,
            int orderId,
            List<LicenseKey> keys,
            List<EmailAttachment>? attachments = null
        )
        {
            if (!keys.Any())
            {
                _logger.LogWarning($"Order ID {orderId} không có key để gửi.");
                return;
            }

            // Khắc phục lỗi: Tải lại Order để kiểm tra số lượng item đã đặt
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            var subject = $"[SOFTSTORE] Khóa Bản Quyền Đơn Hàng #{orderId}";

            var productIds = keys.Select(k => k.ProductId).Distinct();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name);

            var bodyBuilder = new StringBuilder();

            bodyBuilder.Append($"<h2>Khóa Bản Quyền Đơn Hàng #{orderId}</h2>");
            bodyBuilder.Append("<p>Cảm ơn bạn đã mua hàng. Dưới đây là (các) khóa bản quyền của bạn:</p>");

            bodyBuilder.Append("<table style='width:100%; border-collapse: collapse; margin-top: 20px;'>");
            bodyBuilder.Append("<thead><tr style='background-color: #f2f2f2;'>");
            bodyBuilder.Append("<th style='border: 1px solid #ddd; padding: 10px; text-align: left;'>Sản phẩm</th>");
            bodyBuilder.Append("<th style='border: 1px solid #ddd; padding: 10px; text-align: left;'>Key Bản Quyền</th>");
            bodyBuilder.Append("</tr></thead><tbody>");

            foreach (var key in keys)
            {
                var productName = products.GetValueOrDefault(key.ProductId) ?? "Sản phẩm không xác định";
                bodyBuilder.Append("<tr>");
                bodyBuilder.Append($"<td style='border: 1px solid #ddd; padding: 10px;'><strong>{productName}</strong></td>");
                bodyBuilder.Append($"<td style='border: 1px solid #ddd; padding: 10px;'><code style='color: #dc3545; font-weight: bold;'>{key.KeyContent}</code></td>");
                bodyBuilder.Append("</tr>");
            }

            bodyBuilder.Append("</tbody></table>");

            // Thêm cảnh báo nếu biết là key có thể bị thiếu
            if (order != null && order.Items != null)
            {
                int totalRequested = order.Items.Sum(i => i.Quantity);
                if (keys.Count < totalRequested)
                {
                    bodyBuilder.Append($"<p style='color: #ff9800; font-weight: bold; margin-top: 20px;'>⚠️ LƯU Ý QUAN TRỌNG: Số lượng key bạn nhận được ({keys.Count}) ít hơn tổng số lượng bạn đặt ({totalRequested}). Vui lòng liên hệ hỗ trợ ngay lập tức để nhận key còn thiếu.</p>");
                    _logger.LogWarning($"Order ID {orderId}: Số lượng key gửi đi ({keys.Count}) ít hơn số lượng đặt ({totalRequested}).");
                }
            }

            bodyBuilder.Append("<p style='margin-top: 20px;'>Vui lòng giữ key cẩn thận và liên hệ hỗ trợ nếu bạn cần.</p>");


            try
            {
                await _emailSender.SendEmailAsync(
                    toEmail,
                    subject,
                    bodyBuilder.ToString(),
                    attachments
                );

                _logger.LogInformation(
                    $"Đã gửi email key thành công (Order #{orderId}) tới {toEmail}."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Lỗi khi gửi email License Key cho Order ID {orderId} tới {toEmail}."
                );
                throw;
            }
        }
    }
}