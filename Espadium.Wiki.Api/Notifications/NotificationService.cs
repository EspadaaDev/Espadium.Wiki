using Espadium.Wiki.Application.Abstractions;
using Espadium.Wiki.Application.Abstractions.Notifications;
using Microsoft.Extensions.Hosting;

namespace Espadium.Wiki.Api.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailSender _emailSender;
        private readonly IHostEnvironment _env;
        private const string AppName = "Espadium.Wiki";

        public NotificationService(IEmailSender emailSender, IHostEnvironment env)
        {
            _emailSender = emailSender;
            _env = env;
        }

        public Task SendInviteAsync(string toEmail, string inviteLink, string? toName = null, CancellationToken ct = default)
            => SendTemplateAsync(toEmail, "Приглашение", "InviteTemplate.html", inviteLink, toName, ct);

        public Task SendPasswordResetAsync(string toEmail, string resetLink, string? toName = null, CancellationToken ct = default)
            => SendTemplateAsync(toEmail, "Восстановление пароля", "ResetPasswordTemplate.html", resetLink, toName, ct);

        public Task SendConfirmEmailAsync(string toEmail, string confirmLink, string? toName = null, CancellationToken ct = default)
            => SendTemplateAsync(toEmail, "Подтверждение e-mail", "ConfirmEmailTemplate.html", confirmLink, toName, ct);

        private async Task SendTemplateAsync(string to, string subject, string templateFile, string actionUrl, string? toName, CancellationToken ct)
        {
            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", templateFile);
            var html = await File.ReadAllTextAsync(path, ct);
            html = html.Replace("{{AppName}}", AppName)
                       .Replace("{{UserName}}", string.IsNullOrWhiteSpace(toName) ? string.Empty : ", " + toName)
                       .Replace("{{ActionUrl}}", actionUrl);
            await _emailSender.SendAsync(to, subject, html);
        }
    }
}

