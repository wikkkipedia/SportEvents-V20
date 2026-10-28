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
    private readonly IEventRepository _repository;
    private readonly IClock _clock;
    private readonly IResultRanking _ranking;
    private readonly IAuditLogger _logger;

    /// <summary>Створює сервіс із тестованими залежностями.</summary>
    /// <param name="repository">Зберігання стану.</param>
    /// <param name="clock">Джерело поточного часу.</param>
    /// <param name="ranking">Політика розподілу місць.</param>
    /// <param name="logger">Журнал операцій.</param>
    public EventService(IEventRepository repository, IClock clock,
        IResultRanking ranking, IAuditLogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _ranking = ranking ?? throw new ArgumentNullException(nameof(ranking));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Створює довідниковий запис учасника.</summary>
    /// <exception cref="ArgumentException">Некоректне ім'я, номер або дата.</exception>
    public void AddParticipant(int id, string name, DateOnly birthDate)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(name) || birthDate > DateOnly.FromDateTime(_clock.Now.Date))
        {
            throw new ArgumentException("Неправильні дані учасника.");
        }

        DataSnapshot data = _repository.Load();
        if (data.Participants.Any(p => p.Id == id))
        {
            throw new DomainRuleException("Учасник із таким номером уже є.");
        }

        data.Participants.Add(new Participant(id, name.Trim(), birthDate));
        _repository.Save(data);
        _logger.Write("Information", "UC-01", $"Додано учасника #{id}");
    }

    /// <summary>Створює змагання у стані Planned.</summary>
    /// <returns>Новий ідентифікатор.</returns>
    public int CreateEvent(string title, DateOnly date, int distanceMeters)
    {
        if (string.IsNullOrWhiteSpace(title) || distanceMeters < MinimumDistanceMeters)
        {
            throw new ArgumentException("Вкажіть назву та додатну дистанцію.");
        }

        DataSnapshot data = _repository.Load();
        int id = data.Events.Count == 0 ? 1 : data.Events.Max(e => e.Id) + 1;
        data.Events.Add(new SportsEvent(id, title.Trim(), date, distanceMeters, EventStatus.Planned));
        _repository.Save(data);
        _logger.Write("Information", "UC-02", $"Створено змагання #{id}");
        return id;
    }

    /// <summary>Відкриває реєстрацію на підготовлене змагання.</summary>
    public void OpenEvent(int eventId) => ChangeStatus(eventId, EventStatus.Planned, EventStatus.Open);

    /// <summary>Реєструє на відкрите змагання наявного учасника.</summary>
    public void Enroll(int eventId, int participantId)
    {
        DataSnapshot data = _repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        RequireStatus(competition, EventStatus.Open);
        Participant person = data.Participants.SingleOrDefault(p => p.Id == participantId)
            ?? throw new DomainRuleException("Учасника не знайдено.");
        _ = AgeRules.GetGroup(person.BirthDate, competition.EventDate);
        if (data.Registrations.Any(r => r.EventId == eventId && r.ParticipantId == participantId))
        {
            throw new DomainRuleException("Учасника вже зареєстровано.");
        }

        data.Registrations.Add(new(eventId, participantId, DateOnly.FromDateTime(_clock.Now.Date)));
        _repository.Save(data);
        _logger.Write("Information", "UC-03", $"Реєстрація #{participantId} на #{eventId}");
    }

    /// <summary>Фіксує додатний результат одного зареєстрованого учасника.</summary>
    public void RecordResult(int eventId, int participantId, TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero || elapsed >= TimeSpan.FromDays(1) ||
            elapsed.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            throw new ArgumentException("Час має бути додатним, меншим за добу та з точністю до мілісекунди.");
        }

        DataSnapshot data = _repository.Load();
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
        _repository.Save(data);
        _logger.Write("Information", "UC-04", $"Результат #{participantId} на #{eventId}");
    }

    /// <summary>Завершує змагання лише за наявності результатів.</summary>
    public void FinishEvent(int eventId)
    {
        DataSnapshot data = _repository.Load();
        RequireStatus(GetEvent(data, eventId), EventStatus.Open);
        if (!data.Results.Any(r => r.EventId == eventId))
        {
            throw new DomainRuleException("Спочатку внесіть хоча б один результат.");
        }

        ReplaceStatus(data, eventId, EventStatus.Finished);
        _repository.Save(data);
        _logger.Write("Information", "UC-05", $"Закрито змагання #{eventId}");
    }

    /// <summary>Скасовує змагання, якщо його ще не завершено.</summary>
    public void CancelEvent(int eventId)
    {
        DataSnapshot data = _repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        if (competition.Status is not (EventStatus.Planned or EventStatus.Open))
        {
            throw new DomainRuleException("Це змагання вже не можна скасувати.");
        }

        ReplaceStatus(data, eventId, EventStatus.Cancelled);
        _repository.Save(data);
        _logger.Write("Warning", "UC-05", $"Скасовано змагання #{eventId}");
    }

    /// <summary>Шукає змагання одночасно за назвою та/або датою.</summary>
    public IReadOnlyList<SportsEvent> FindEvents(string? title, DateOnly? date)
    {
        IEnumerable<SportsEvent> events = _repository.Load().Events;
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
    public IReadOnlyList<ProtocolRow> GenerateProtocol(int eventId)
    {
        DataSnapshot data = _repository.Load();
        SportsEvent competition = GetEvent(data, eventId);
        if (competition.Status is EventStatus.Planned or EventStatus.Cancelled)
        {
            throw new DomainRuleException("Протокол доступний лише для відкритих або завершених змагань.");
        }

        return _ranking.Build(data, eventId);
    }

    /// <summary>Кількість учасників у каталозі, для перевірки демонстраційних даних.</summary>
    public int ParticipantCount() => _repository.Load().Participants.Count;

    /// <summary>Створює 60 вигаданих учасників для навчального середовища.</summary>
    public void SeedDemoParticipants()
    {
        if (ParticipantCount() > 0)
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
            DateOnly birthday = new(baseYear + id % DemoYearCycle, 1 + id % MonthsPerYear, DemoBirthDay);
            AddParticipant(id, $"Тестовий учасник {id}", birthday);
        }
    }

    private void ChangeStatus(int id, EventStatus expected, EventStatus next)
    {
        DataSnapshot data = _repository.Load();
        RequireStatus(GetEvent(data, id), expected);
        ReplaceStatus(data, id, next);
        _repository.Save(data);
        _logger.Write("Information", "UC-05", $"Стан змагання #{id}: {next}");
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
