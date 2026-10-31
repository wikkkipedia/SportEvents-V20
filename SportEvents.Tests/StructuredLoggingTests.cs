using Microsoft.Extensions.Logging;
using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class StructuredLoggingTests
{
    [Fact]
    public void AddParticipant_Valid_WritesStructuredDebugAndInformationWithScope()
    {
        CaptureLogger logger = new();
        EventService service = Create(new MemoryRepository(), logger);

        service.AddParticipant(42, "Olena", new DateOnly(2000, 1, 1));

        Assert.Contains(logger.Records, x => x.Level == LogLevel.Debug && x.EventId.Id == 1001);
        CapturedLog info = Assert.Single(logger.Records, x => x.Level == LogLevel.Information);
        Assert.Equal(42, info.Fields["ParticipantId"]);
        Assert.Equal("Зареєстровано учасника {ParticipantId}", info.Template);
        Assert.Equal(42, info.Scope["ParticipantId"]);
    }

    [Fact]
    public void AddParticipant_InvalidOrDuplicate_WritesStructuredWarning()
    {
        CaptureLogger logger = new();
        EventService service = Create(new MemoryRepository(), logger);

        Assert.Throws<ArgumentException>(() => service.AddParticipant(-1, "X", new DateOnly(2000, 1, 1)));
        service.AddParticipant(2, "Oleh", new DateOnly(2000, 1, 1));
        Assert.Throws<DomainRuleException>(() => service.AddParticipant(2, "Oleh", new DateOnly(2000, 1, 1)));

        Assert.Equal(2, logger.Records.Count(x => x.Level == LogLevel.Warning));
        Assert.Contains(logger.Records, x => x.EventId.Id == 1003 && Equals(x.Fields["ParticipantId"], 2));
    }

    [Fact]
    public void AddParticipant_RepositoryFailure_PassesExceptionAsException()
    {
        CaptureLogger logger = new();
        EventService service = Create(new RejectingRepository(), logger);

        IOException exception = Assert.Throws<IOException>(() =>
            service.AddParticipant(9, "Olha", new DateOnly(2001, 1, 1)));

        CapturedLog error = Assert.Single(logger.Records, x => x.Level == LogLevel.Error);
        Assert.Same(exception, error.Exception);
        Assert.Equal("Помилка збереження учасника {ParticipantId}", error.Template);
        Assert.Equal(9, error.Fields["ParticipantId"]);
        Assert.Equal(9, error.Scope["ParticipantId"]);
    }

    private static EventService Create(IEventRepository repository, CaptureLogger logger)
        => new(repository, new FixedClock(), new RankingPolicy(), new NullLogger(), logger);

    private sealed class RejectingRepository : IEventRepository
    {
        public DataSnapshot Load() => new();

        public void Save(DataSnapshot data) => throw new IOException("Demonstration storage failure");
    }

    private sealed class CaptureLogger : ILogger<EventService>
    {
        private IDictionary<string, object?>? currentScope;

        public List<CapturedLog> Records { get; } = new();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            currentScope = Pairs(state);
            return new Scope(() => currentScope = null);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Dictionary<string, object?> fields = Pairs(state);
            Records.Add(new CapturedLog(logLevel, eventId, fields,
                currentScope is null ? new Dictionary<string, object?>() : new Dictionary<string, object?>(currentScope),
                fields.TryGetValue("{OriginalFormat}", out object? format) ? format?.ToString() : null,
                exception));
        }

        private static Dictionary<string, object?> Pairs<TState>(TState state)
            => state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(p => p.Key, p => p.Value)
                : new Dictionary<string, object?>();

        private sealed class Scope : IDisposable
        {
            private readonly Action onDispose;

            public Scope(Action onDispose) => this.onDispose = onDispose;

            public void Dispose() => onDispose();
        }
    }

    private sealed record CapturedLog(LogLevel Level, EventId EventId,
        IDictionary<string, object?> Fields, IDictionary<string, object?> Scope,
        string? Template, Exception? Exception);
}
