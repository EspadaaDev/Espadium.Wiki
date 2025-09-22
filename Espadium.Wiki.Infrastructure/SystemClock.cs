using Espadium.Wiki.Application.Abstractions;

namespace Espadium.Wiki.Infrastructure
{
    public class SystemClock : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}

