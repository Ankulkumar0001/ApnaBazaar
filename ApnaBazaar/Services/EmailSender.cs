using ApnaBazaar.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ApnaBazaar.Services
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmail(string toEmail, string subject, string body)
        {
            MailMessage message = new MailMessage();

            message.From = new MailAddress(_settings.Email);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient(_settings.Host, _settings.Port);
            smtp.Credentials = new NetworkCredential(_settings.Email, _settings.Password);
            smtp.EnableSsl = true;

            await smtp.SendMailAsync(message);
        }
    }
}
