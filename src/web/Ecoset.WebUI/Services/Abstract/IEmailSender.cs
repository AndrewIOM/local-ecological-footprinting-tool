using System.Threading.Tasks;

namespace Ecoset.WebUI.Services.Abstract
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
