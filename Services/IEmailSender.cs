using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanPhanMem.Models;

namespace WebBanPhanMem.Services
{
    public interface IEmailSender
    {
        /// <summary>
        /// Gửi email bất đồng bộ, hỗ trợ gửi kèm file đính kèm.
        /// </summary>
        /// <param name="to">Địa chỉ email người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="body">Nội dung email (hỗ trợ HTML)</param>
        /// <param name="attachments">Danh sách file đính kèm, có thể null hoặc rỗng</param>
        Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment>? attachments = null);
    }
}
