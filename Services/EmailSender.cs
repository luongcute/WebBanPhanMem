using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.IO;
using WebBanPhanMem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebBanPhanMem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            List<EmailAttachment>? attachments = null)
        {
            // Lấy cấu hình SMTP, kiểm tra null hoặc rỗng
            var smtpHost = _config["Email:SmtpHost"];
            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new InvalidOperationException("SMTP Host chưa được cấu hình trong appsettings.json");

            var smtpPortStr = _config["Email:SmtpPort"];
            int smtpPort = 587; // default
            if (!string.IsNullOrWhiteSpace(smtpPortStr) && !int.TryParse(smtpPortStr, out smtpPort))
            {
                smtpPort = 587;
            }

            var smtpUser = _config["Email:Username"];
            if (string.IsNullOrWhiteSpace(smtpUser))
                throw new InvalidOperationException("SMTP Username chưa được cấu hình trong appsettings.json");

            var smtpPass = _config["Email:Password"];
            if (string.IsNullOrWhiteSpace(smtpPass))
                throw new InvalidOperationException("SMTP Password chưa được cấu hình trong appsettings.json");

            var fromEmail = _config["Email:From"];
            if (string.IsNullOrWhiteSpace(fromEmail))
                fromEmail = smtpUser; // fallback dùng Username làm địa chỉ gửi

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException("Email From chưa được cấu hình trong appsettings.json");

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var message = new MailMessage()
            {
                From = new MailAddress(fromEmail, "Cửa hàng phần mềm"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            // Gắn file đính kèm nếu có
            if (attachments != null)
            {
                foreach (var a in attachments)
                {
                    var stream = new MemoryStream(a.Content);
                    message.Attachments.Add(new Attachment(stream, a.FileName, a.ContentType));
                }
            }

            await smtp.SendMailAsync(message);
        }
    }
}
