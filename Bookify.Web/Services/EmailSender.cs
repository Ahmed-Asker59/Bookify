using Bookify.Web.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Bookify.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment  _webHostEnvironment;

        public EmailSender(IWebHostEnvironment webHostEnvironment,
            IOptions<MailSettings> mailSettings)
        {
            _webHostEnvironment = webHostEnvironment;
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage()
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true
            };

            message.To.Add(_webHostEnvironment.IsDevelopment() ? "askarahmad189@gmail.com" : email);
            SmtpClient smtp = new SmtpClient(_mailSettings.Host!)
            {
                Port = _mailSettings.Port,
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
            smtp.Dispose();
        }
    }
}
