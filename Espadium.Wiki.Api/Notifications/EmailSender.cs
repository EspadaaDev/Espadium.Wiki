using Espadium.Wiki.Api.Configuration.Options;
using Espadium.Wiki.Application.Abstractions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Espadium.Wiki.Api.Notifications
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        public EmailSender(IOptions<EmailOptions> options) { _options = options.Value; }

        public async Task SendAsync(string to, string subject, string html)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_options.From));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var client = new SmtpClient { Timeout = 5000 };
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, false);
            if (!string.IsNullOrEmpty(_options.SmtpUser))
                await client.AuthenticateAsync(_options.SmtpUser, _options.SmtpPassword);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}

