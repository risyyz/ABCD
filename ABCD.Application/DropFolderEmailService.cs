using System.Net.Mail;
using ABCD.Lib;
using Microsoft.Extensions.Options;

namespace ABCD.Application {
    public class DropFolderEmailService : IEmailService {
        private readonly EmailSettings _emailSettings;

        public DropFolderEmailService(IOptions<EmailSettings> emailSettings) {
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

            if (string.IsNullOrWhiteSpace(_emailSettings.PickupDirectory))
                throw new InvalidOperationException("PickupDirectory must be set for DropFolderEmailService.");

            using var client = new SmtpClient {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = _emailSettings.PickupDirectory
            };

            await client.SendMailAsync(message);
        }
    }
}
