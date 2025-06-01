using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Volunteer_website.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent to {toEmail}"); // Thêm log để debug
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}"); // Log lỗi
                throw; // Ném lại ngoại lệ để controller xử lý
            }
        }
    }
}