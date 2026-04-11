using System.Net;
using System.Net.Mail;
using ABCD.Lib;
using Microsoft.Extensions.Options;

namespace ABCD.Application {
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

            using var client = CreateSmtpClient();
            await client.SendMailAsync(message);
        }

        private SmtpClient CreateSmtpClient() {
            if (!string.IsNullOrWhiteSpace(_emailSettings.PickupDirectory)) {
                // Use drop folder in dev
                return new SmtpClient {
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = _emailSettings.PickupDirectory
                };
            }
            // Use real SMTP in prod
            return new SmtpClient(_emailSettings.Host, _emailSettings.Port) {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };
        }
    }
}
