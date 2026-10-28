namespace SportEvents.Lab06;

/// <summary>Єдиний об'єкт параметрів для обробки спортивного протоколу.</summary>
public sealed record SportRequest
{
    public int DocId { get; init; }
    public int ClientId { get; init; }
    public string ClientName { get; init; } = "";
    public string? ClientMail { get; init; }
    public string ClientKind { get; init; } = "regular";
    public int ClientDone { get; init; }
    public List<SportsResultLine>? Items { get; init; }
    public string State { get; init; } = "new";
    public string Currency { get; init; } = "UAH";
    public DateTime CreatedAt { get; init; }
    public bool SendMail { get; init; }
}
