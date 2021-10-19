namespace Application.Common.Interfaces.Infrastructure.Services
{
    public interface IMailSender
    {
        void Send(string emailTo, string nameTo, string subject, string body, bool isBodyHtml = false);
    }
}