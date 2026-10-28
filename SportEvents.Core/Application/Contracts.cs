using SportEvents.Core.Domain;

namespace SportEvents.Core.Application;

/// <summary>Знімок даних, який можна зберігати у JSON або в тестовому сховищі.</summary>
public sealed class DataSnapshot
{
    /// <summary>Довідник учасників.</summary>
    public List<Participant> Participants { get; set; } = new();
    /// <summary>Перелік змагань.</summary>
    public List<SportsEvent> Events { get; set; } = new();
    /// <summary>Записи реєстрації.</summary>
    public List<EventRegistration> Registrations { get; set; } = new();
    /// <summary>Фінішні результати.</summary>
    public List<FinishResult> Results { get; set; } = new();
}

/// <summary>Контракт сховища незалежно від JSON або пам'яті.</summary>
public interface IEventRepository
{
    /// <summary>Завантажує дані та повертає порожній набір, якщо сховище ще не створене.</summary>
    /// <returns>Знімок даних.</returns>
    DataSnapshot Load();

    /// <summary>Зберігає поточний знімок даних.</summary>
    /// <param name="data">Стан після коректного сценарію.</param>
    void Save(DataSnapshot data);
}

/// <summary>Підмінне джерело поточного часу.</summary>
public interface IClock
{
    /// <summary>Поточний час з часовим поясом.</summary>
    DateTimeOffset Now { get; }
}

/// <summary>Підмінний журнал виконаних сценаріїв.</summary>
public interface IAuditLogger
{
    /// <summary>Записує подію без персональних даних.</summary>
    /// <param name="level">Information, Warning або Error.</param>
    /// <param name="scenario">Код сценарію.</param>
    /// <param name="message">Опис події.</param>
    void Write(string level, string scenario, string message);
}

/// <summary>Правило визначення місць у вікових групах.</summary>
public interface IResultRanking
{
    /// <summary>Створює протокол з підсумків і списку учасників.</summary>
    /// <param name="data">Збережені учасники, старти та фініші.</param>
    /// <param name="eventId">Змагання, для якого формується протокол.</param>
    /// <returns>Список рядків у порядку вікових груп та місць.</returns>
    IReadOnlyList<ProtocolRow> Build(DataSnapshot data, int eventId);
}

/// <summary>Рядок підсумкового протоколу з місцем у групі.</summary>
/// <param name="ParticipantId">Номер учасника.</param>
/// <param name="Name">Відображуване ім'я.</param>
/// <param name="Group">Вікова група.</param>
/// <param name="Elapsed">Тривалість дистанції.</param>
/// <param name="Place">Місце; за однакового часу місце однакове.</param>
public sealed record ProtocolRow(int ParticipantId, string Name, AgeGroup Group, TimeSpan Elapsed, int Place);
