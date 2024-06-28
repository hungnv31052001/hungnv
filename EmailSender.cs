using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using JobWebApplicationvip.Models;

namespace JobWebApplicationvip.Areas.Identity
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (await SendEmailUsingSmtp(email, subject, htmlMessage))
            {
                // Email sent successfully
            }
            else
            {
                // Email sending failed
                throw new Exception("Failed to send email.");
            }
        }

        private async Task<bool> SendEmailUsingSmtp(string email, string subject, string htmlMessage)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_smtpSettings.FromAddress);
                    message.To.Add(email);
                    message.Subject = subject;
                    message.Body = htmlMessage;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        await smtpClient.SendMailAsync(message);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // Log exception here
                return false;
            }
        }
    }
}