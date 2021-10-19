using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.MailSender
{
    public class MailSender : IMailSender
    {
        private readonly SmtpClient _smtpClient;
        private readonly MailAddress _from;
        
        public MailSender(IConfiguration configuration)
        {
            _smtpClient = new SmtpClient(configuration["SmtpClient:Host"], int.Parse(configuration["SmtpClient:Port"]))
            {
                Credentials = new NetworkCredential(configuration["SmtpClient:Credential:Username"], configuration["SmtpClient:Credential:Password"]),
                EnableSsl = true,
            };

            _from = new MailAddress(configuration["SmtpClient:From:Email"], configuration["SmtpClient:From:Name"]);
        }

        public void Send(string emailTo, string nameTo, string subject, string body, bool isBodyHtml = false)
        {
            var message = new MailMessage(_from, new MailAddress(emailTo, nameTo))
            {
                IsBodyHtml = isBodyHtml,
                Body = body,
                Subject = subject
            };
            
            _smtpClient.SendAsync(message, null);
        }
    }
}