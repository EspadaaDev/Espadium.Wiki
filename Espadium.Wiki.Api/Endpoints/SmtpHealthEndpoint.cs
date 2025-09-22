using Espadium.Wiki.Api.Configuration.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

namespace Espadium.Wiki.Api.Endpoints
{
    public static class SmtpHealthEndpoint
    {
        public static IEndpointRouteBuilder MapSmtpHealth(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health/smtp", async (IOptions<EmailOptions> email) =>
            {
                try
                {
                    using var client = new SmtpClient { Timeout = 5000 };
                    await client.ConnectAsync(email.Value.SmtpHost, email.Value.SmtpPort, false);
                    if (!string.IsNullOrEmpty(email.Value.SmtpUser))
                        await client.AuthenticateAsync(email.Value.SmtpUser, email.Value.SmtpPassword);
                    await client.DisconnectAsync(true);
                    return Results.Json(new { status = "ok" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            });
            return app;
        }
    }
}

