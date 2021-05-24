using Core.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Company.Areas.Identity
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailService _emailService;

        public EmailSender(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
