namespace SportEvents.Lab03;

/// <summary>Дані, потрібні для навчального розрахунку спортивного протоколу.</summary>
public sealed record RaceRequest
{
    public int DocId { get; init; }
    public string ClientName { get; init; } = "";
    public string ClientEmail { get; init; } = "";
    public bool IsRegular { get; init; }
    public List<RaceChargeLine> Lines { get; init; } = new();
    public DateOnly CreatedAt { get; init; }
    public string CouponCode { get; init; } = "";
    public string Status { get; init; } = "New";
    public bool PrintToConsole { get; init; }
    public decimal DeliveryPrice { get; init; }
}
