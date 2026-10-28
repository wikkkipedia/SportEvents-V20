namespace SportEvents.Core.Domain;

/// <summary>Етап проведення змагання; перехід між етапами контролює сервіс.</summary>
public enum EventStatus
{
    /// <summary>Підготовка.</summary>
    Planned,

    /// <summary>Реєстрацію відкрито.</summary>
    Open,

    /// <summary>Результати затверджено.</summary>
    Finished,

    /// <summary>Змагання скасовано.</summary>
    Cancelled,
}

/// <summary>Вікова група на дату змагання.</summary>
public enum AgeGroup
{
    /// <summary>Менше ніж 18 років.</summary>
    Junior,

    /// <summary>Від 18 до 39 років.</summary>
    Adult,

    /// <summary>Від 40 років.</summary>
    Masters,
}

/// <summary>Учасник із датою народження; зразкові дані не належать реальним людям.</summary>
/// <param name="Id">Додатний унікальний ідентифікатор.</param>
/// <param name="FullName">Ім'я учасника.</param>
/// <param name="BirthDate">Дата народження.</param>
public sealed record Participant(int Id, string FullName, DateOnly BirthDate);

/// <summary>Картка змагання з керованим станом.</summary>
/// <param name="Id">Унікальний ідентифікатор.</param>
/// <param name="Title">Назва.</param>
/// <param name="EventDate">Дата проведення.</param>
/// <param name="DistanceMeters">Дистанція у метрах.</param>
/// <param name="Status">Стан змагання.</param>
public sealed record SportsEvent(
    int Id, string Title, DateOnly EventDate, int DistanceMeters, EventStatus Status);

/// <summary>Реєстрація конкретного учасника на одне змагання.</summary>
/// <param name="EventId">Ідентифікатор змагання.</param>
/// <param name="ParticipantId">Ідентифікатор учасника.</param>
/// <param name="RegisteredOn">Дата реєстрації.</param>
public sealed record EventRegistration(int EventId, int ParticipantId, DateOnly RegisteredOn);

/// <summary>Зафіксований час проходження дистанції.</summary>
/// <param name="EventId">Ідентифікатор змагання.</param>
/// <param name="ParticipantId">Ідентифікатор учасника.</param>
/// <param name="Elapsed">Тривалість, що повинна бути додатною.</param>
public sealed record FinishResult(int EventId, int ParticipantId, TimeSpan Elapsed);

/// <summary>Порушення правила переходу стану або участі у змаганні.</summary>
public sealed class DomainRuleException : InvalidOperationException
{
    /// <summary>Initializes a new instance of the <see cref="DomainRuleException"/> class.Створює помилку з поясненням порушеного правила.</summary>
    /// <param name="message">Повідомлення без персональних даних.</param>
    public DomainRuleException(string message)
        : base(message)
    {
    }
}
