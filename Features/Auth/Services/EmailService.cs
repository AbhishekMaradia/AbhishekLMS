using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LMS_SoulCode.Features.Auth.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtp = _config.GetSection("SmtpSettings");
            var from = smtp["FromEmail"];
            var username = smtp["UserName"];
            var password = smtp["Password"];
            var host = smtp["Host"];
            var port = int.Parse(smtp["Port"]);
            var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };


                var mail = new MailMessage
                {
                    From = new MailAddress(from, "LMS SoulCode Support"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);
            }
            catch (SmtpException ex)
            {
                throw new Exception($"SMTP failed: {ex.StatusCode} - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Email sending failed: {ex.Message}", ex);
            }
        }



    }
}
