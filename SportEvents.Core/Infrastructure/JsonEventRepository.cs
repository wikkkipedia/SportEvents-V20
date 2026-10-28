using System.Text.Json;
using SportEvents.Core.Application;

namespace SportEvents.Core.Infrastructure;

/// <summary>Файлове сховище стану у форматі JSON з безпечним записом через тимчасовий файл.</summary>
public sealed class JsonEventRepository : IEventRepository
{
    private readonly string _path;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Налаштовує шлях до файлу з даними.</summary>
    /// <param name="path">Шлях у доступній для запису теці.</param>
    public JsonEventRepository(string path)
    {
        _path = !string.IsNullOrWhiteSpace(path) ? path : throw new ArgumentException("Шлях порожній.");
    }

    /// <inheritdoc />
    public DataSnapshot Load()
    {
        if (!File.Exists(_path))
        {
            return new DataSnapshot();
        }

        string json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<DataSnapshot>(json)
            ?? throw new InvalidDataException("Файл JSON не містить об'єкта даних.");
    }

    /// <inheritdoc />
    public void Save(DataSnapshot data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        string temporary = _path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(data, Options));
        File.Move(temporary, _path, true);
    }
}
