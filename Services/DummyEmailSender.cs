using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ECommerceApp.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // No-op for development/testing
            return Task.CompletedTask;
        }
    }
}
