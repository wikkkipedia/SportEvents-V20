namespace SportEvents.Core.Application;

/// <summary>Типізований результат успішної операції або помилки вводу.</summary>
/// <typeparam name="T">Тип коректного значення.</typeparam>
public sealed record OperationResult<T>
{
    /// <summary>Gets a value indicating whether чи вдалося виконати операцію.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets результат за успішної операції, інакше default.</summary>
    public T? Value { get; }

    /// <summary>Gets пояснення користувачеві за неуспіху.</summary>
    public string? Error { get; }

    private OperationResult(bool success, T? value, string? error)
        => (IsSuccess, Value, Error) = (success, value, error);

    /// <summary>Успішний результат.</summary>
    /// <returns></returns>
    public static OperationResult<T> Ok(T value) => new(true, value, null);

    /// <summary>Помилка вводу користувача.</summary>
    /// <returns></returns>
    public static OperationResult<T> Fail(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, default, error);
    }
}
