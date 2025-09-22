using Espadium.Wiki.Application.Abstractions;

namespace Espadium.Wiki.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string html)
        {
            return Task.CompletedTask;
        }
    }
}

