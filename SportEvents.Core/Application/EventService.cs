using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SportEvents.Core.Domain;

namespace SportEvents.Core.Application;

/// <summary>Сценарії обліку змагань, незалежні від консолі й формату зберігання.</summary>
public sealed class EventService
{
    private const int MinimumDistanceMeters = 1;
    private const int DemoParticipantCount = 60;
    private const int DemoAgeGroupCycle = 3;
    private const int DemoYearCycle = 5;
    private const int DemoBirthDay = 15;
    private const int MonthsPerYear = 12;
    private const int JuniorStartYear = 2010;
    private const int AdultStartYear = 1990;
    private const int MastersStartYear = 1965;
    private static readonly Func<ILogger, int, IDisposable?> ParticipantScope =
        LoggerMessage.DefineScope<int>("Participant:{ParticipantId}");

    private static readonly Action<ILogger, int, Exception?> LogParticipantBegin =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(1001, "RegistrationStart"), "Початок реєстрації {ParticipantId}");

    private static readonly Action<ILogger, int, Exception?> LogParticipantInvalid =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(1002, "RegistrationInvalid"), "Некоректні дані учасника {ParticipantId}");

    private static readonly Action<ILogger, int, Exception?> LogParticipantDuplicate =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(1003, "RegistrationDuplicate"), "Учасник {ParticipantId} вже існує");

    private static readonly Action<ILogger, int, Exception?> LogParticipantError =
        LoggerMessage.Define<int>(LogLevel.Error, new EventId(1004, "RegistrationSaveFailed"), "Помилка збереження учасника {ParticipantId}");

    private static readonly Action<ILogger, int, Exception?> LogParticipantSuccess =
        LoggerMessage.Define<int>(LogLevel.Information, new EventId(1005, "RegistrationSucceeded"), "Зареєстровано учасника {ParticipantId}");

    private readonly IEventRepository repository;
    private readonly IClock clock;
    private readonly IResultRanking ranking;
    private readonly IAuditLogger logger;
    private readonly ILogger<EventService> structuredLogger;

    /// <summary>Initializes a new instance of the <see cref="EventService"/> class.Створює сервіс із тестованими залежностями.</summary>
    /// <param name="repository">Зберігання стану.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <param name="ranking">Політика розподілу місць.</param>
    /// <param name="logger">Журнал операцій.</param>
    /// <param name="structuredLogger">Optional typed structured logger with scopes.</param>
    public EventService(IEventRepository repository, IClock clock,
        IResultRanking ranking, IAuditLogger logger,
        ILogger<EventService>? structuredLogger = null)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.ranking = ranking ?? throw new ArgumentNullException(nameof(ranking));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.structuredLogger = structuredLogger ?? NullLogger<EventService>.Instance;
    }

    /// <summary>Створює довідниковий запис учасника.</summary>
    /// <exception cref="ArgumentException">Некоректне ім'я, номер або дата.</exception>
    public void AddParticipant(int id, string name, DateOnly birthDate)
    {
        using IDisposable? scope = ParticipantScope(structuredLogger, id);
        LogParticipantBegin(structuredLogger, id, null);

        if (id <= 0 || string.IsNullOrWhiteSpace(name) || birthDate > DateOnly.FromDateTime(clock.Now.Date))
        {
            LogParticipantInvalid(structuredLogger, id, null);
            throw new ArgumentException("Некоректні дані учасника.");
        }

        DataSnapshot data = repository.Load();
        if (data.Participants.Any(p => p.Id == id))
        {
            LogParticipantDuplicate(structuredLogger, id, null);
            throw new DomainRuleException("Учасник з таким номером уже є.");
        }

        data.Participants.Add(new Participant(id, name.Trim(), birthDate));
        try
        {
            repository.Save(data);
        }
        catch (IOException ex)
        {
            LogParticipantError(structuredLogger, id, ex);
            throw;
        }

        logger.Write("Information", "UC-01", $"Додано учасника #{id}");
        LogParticipantSuccess(structuredLogger, id, null);
    }

    /// <summary>Створює змагання у стані Planned.</summary>
    /// <returns>Новий ідентифікатор.</returns>
    public int CreateEvent(string title, DateOnly date, int distanceMeters)
    {
        if (string.IsNullOrWhiteSpace(title) || distanceMeters < MinimumDistanceMeters)
        {
            throw new ArgumentException("Вкажіть назву та додатну дистанцію.");
        }

        DataSnapshot data = repository.Load();
        int id = data.Events.Count == 0 ? 1 : data.Events.Max(e => e.Id) + 1;
        data.Events.Add(new SportsEvent(id, title.Trim(), date, distanceMeters, EventStatus.Planned));
        repository.Save(data);
        logger.Write("Information", "UC-02", $"Створено змагання #{id}");
        return id;
    }

    /// <summary>Відкриває реєстрацію на підготовлене змагання.</summary>
    public void OpenEvent(int eventId) => this.ChangeStatus(eventId, EventStatus.Planned, EventStatus.Open);

    /// <summary>Реєструє на відкрите змагання наявного учасника.</summary>
    public void Enroll(int eventId, int participantId)
    {
        DataSnapshot data = repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        RequireStatus(competition, EventStatus.Open);
        Participant person = data.Participants.SingleOrDefault(p => p.Id == participantId)
            ?? throw new DomainRuleException("Учасника не знайдено.");
        _ = AgeRules.GetGroup(person.BirthDate, competition.EventDate);
        if (data.Registrations.Any(r => r.EventId == eventId && r.ParticipantId == participantId))
        {
            throw new DomainRuleException("Учасника вже зареєстровано.");
        }

        data.Registrations.Add(new(eventId, participantId, DateOnly.FromDateTime(clock.Now.Date)));
        repository.Save(data);
        logger.Write("Information", "UC-03", $"Реєстрація #{participantId} на #{eventId}");
    }

    /// <summary>Фіксує додатний результат одного зареєстрованого учасника.</summary>
    public void RecordResult(int eventId, int participantId, TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero || elapsed >= TimeSpan.FromDays(1) ||
            elapsed.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            throw new ArgumentException("Час має бути додатним, меншим за добу та з точністю до мілісекунди.");
        }

        DataSnapshot data = repository.Load();
        RequireStatus(GetEvent(data, eventId), EventStatus.Open);
        if (!data.Registrations.Any(r => r.EventId == eventId && r.ParticipantId == participantId))
        {
            throw new DomainRuleException("Спочатку зареєструйте учасника.");
        }

        if (data.Results.Any(r => r.EventId == eventId && r.ParticipantId == participantId))
        {
            throw new DomainRuleException("Результат уже внесено.");
        }

        data.Results.Add(new(eventId, participantId, elapsed));
        repository.Save(data);
        logger.Write("Information", "UC-04", $"Результат #{participantId} на #{eventId}");
    }

    /// <summary>Завершує змагання лише за наявності результатів.</summary>
    public void FinishEvent(int eventId)
    {
        DataSnapshot data = repository.Load();
        RequireStatus(GetEvent(data, eventId), EventStatus.Open);
        if (!data.Results.Any(r => r.EventId == eventId))
        {
            throw new DomainRuleException("Спочатку внесіть хоча б один результат.");
        }

        ReplaceStatus(data, eventId, EventStatus.Finished);
        repository.Save(data);
        logger.Write("Information", "UC-05", $"Закрито змагання #{eventId}");
    }

    /// <summary>Скасовує змагання, якщо його ще не завершено.</summary>
    public void CancelEvent(int eventId)
    {
        DataSnapshot data = repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        if (competition.Status is not (EventStatus.Planned or EventStatus.Open))
        {
            throw new DomainRuleException("Це змагання вже не можна скасувати.");
        }

        ReplaceStatus(data, eventId, EventStatus.Cancelled);
        repository.Save(data);
        logger.Write("Warning", "UC-05", $"Скасовано змагання #{eventId}");
    }

    /// <summary>Шукає змагання одночасно за назвою та/або датою.</summary>
    /// <returns></returns>
    public IReadOnlyList<SportsEvent> FindEvents(string? title, DateOnly? date)
    {
        IEnumerable<SportsEvent> events = repository.Load().Events;
        if (!string.IsNullOrWhiteSpace(title))
        {
            events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (date.HasValue)
        {
            events = events.Where(e => e.EventDate == date.Value);
        }

        return events.OrderBy(e => e.EventDate).ThenBy(e => e.Id).ToList();
    }

    /// <summary>Повертає відсортовані місця для сформованого чи активного змагання.</summary>
    /// <returns></returns>
    public IReadOnlyList<ProtocolRow> GenerateProtocol(int eventId)
    {
        DataSnapshot data = repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        if (competition.Status is EventStatus.Planned or EventStatus.Cancelled)
        {
            throw new DomainRuleException("Протокол доступний лише для відкритих або завершених змагань.");
        }

        return ranking.Build(data, eventId);
    }

    /// <summary>Кількість учасників у каталозі, для перевірки демонстраційних даних.</summary>
    /// <returns></returns>
    public int ParticipantCount() => repository.Load().Participants.Count;

    /// <summary>Створює 60 вигаданих учасників для навчального середовища.</summary>
    public void SeedDemoParticipants()
    {
        if (this.ParticipantCount() > 0)
        {
            return;
        }

        for (int id = 1; id <= DemoParticipantCount; id++)
        {
            int baseYear = (id % DemoAgeGroupCycle) switch
            {
                0 => JuniorStartYear,
                1 => AdultStartYear,
                _ => MastersStartYear,
            };
            DateOnly birthday = new(baseYear + (id % DemoYearCycle), 1 + (id % MonthsPerYear), DemoBirthDay);
            this.AddParticipant(id, $"Тестовий учасник {id}", birthday);
        }
    }

    private void ChangeStatus(int id, EventStatus expected, EventStatus next)
    {
        DataSnapshot data = repository.Load();
        RequireStatus(GetEvent(data, id), expected);
        ReplaceStatus(data, id, next);
        repository.Save(data);
        logger.Write("Information", "UC-05", $"Стан змагання #{id}: {next}");
    }

    private static SportsEvent GetEvent(DataSnapshot data, int id)
        => data.Events.SingleOrDefault(e => e.Id == id)
           ?? throw new DomainRuleException("Змагання не знайдено.");

    private static void RequireStatus(SportsEvent competition, EventStatus expected)
    {
        if (competition.Status != expected)
        {
            throw new DomainRuleException($"Потрібен стан {expected}, а зараз {competition.Status}.");
        }
    }

    private static void ReplaceStatus(DataSnapshot data, int id, EventStatus status)
    {
        int index = data.Events.FindIndex(e => e.Id == id);
        data.Events[index] = data.Events[index] with { Status = status };
    }
}
