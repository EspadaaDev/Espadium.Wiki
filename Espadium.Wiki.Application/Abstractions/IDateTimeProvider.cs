namespace Espadium.Wiki.Application.Abstractions
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}

