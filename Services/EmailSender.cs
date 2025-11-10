using System.Net;
using System.Net.Mail;

namespace WebBanPhanMem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpUser = _config["Email:Username"];
            var smtpPass = _config["Email:Password"];
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };
            var message = new MailMessage(smtpUser, to, subject, body) { IsBodyHtml = true };
            await client.SendMailAsync(message);
        }
    }
}
