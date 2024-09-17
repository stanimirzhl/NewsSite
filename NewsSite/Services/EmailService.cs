using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using NewsSite.EmailSettingsModel;
using Microsoft.AspNetCore.Hosting.Server;
using System.Composition;

namespace NewsSite.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient smtpClient;
        private readonly SmtpSettings smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            this.smtpSettings = smtpSettings.Value;
            this.smtpClient = new SmtpClient(smtpSettings.Value.Server)
            {
                Port = smtpSettings.Value.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpSettings.Value.SenderEmail, smtpSettings.Value.Password),
                EnableSsl = smtpSettings.Value.EnableSsl
            };
        }

        public async Task SendEmailAsync(string toUser, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.SenderEmail, smtpSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toUser);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException($"Error sending email: {ex.Message}", ex);
            }
        }
    }
}
