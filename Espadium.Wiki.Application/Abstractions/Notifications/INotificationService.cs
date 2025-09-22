namespace Espadium.Wiki.Application.Abstractions.Notifications
{
    public interface INotificationService
    {
        Task SendInviteAsync(string toEmail, string inviteLink, string? toName = null, CancellationToken ct = default);
        Task SendPasswordResetAsync(string toEmail, string resetLink, string? toName = null, CancellationToken ct = default);
        Task SendConfirmEmailAsync(string toEmail, string confirmLink, string? toName = null, CancellationToken ct = default);
    }
}

