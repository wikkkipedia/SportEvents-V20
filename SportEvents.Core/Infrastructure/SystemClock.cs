using SportEvents.Core.Application;

namespace SportEvents.Core.Infrastructure;

/// <summary>Реальне джерело часу, відокремлене від доменної логіки.</summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset Now => DateTimeOffset.Now;
}
