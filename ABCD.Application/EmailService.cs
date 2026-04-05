using System.Net;
using System.Net.Mail;

using ABCD.Lib;

using Microsoft.Extensions.Options;

namespace ABCD.Application {
    public interface IEmailService {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class SmtpEmailService : IEmailService {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings) {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body) {
            using var message = new MailMessage {
                From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port) {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            await client.SendMailAsync(message);
        }
    }
}
