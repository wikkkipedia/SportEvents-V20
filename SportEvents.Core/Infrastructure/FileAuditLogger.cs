using SportEvents.Core.Application;

namespace SportEvents.Core.Infrastructure;

/// <summary>Додає до журналу час, рівень, код сценарію та текст події.</summary>
public sealed class FileAuditLogger : IAuditLogger
{
    private readonly string _path;
    private readonly IClock _clock;

    /// <summary>Створює файловий журнал.</summary>
    /// <param name="path">Місце журналу без персональних даних.</param>
    /// <param name="clock">Джерело часу, яке можна підмінити.</param>
    public FileAuditLogger(string path, IClock clock)
    {
        _path = path;
        _clock = clock;
    }

    /// <inheritdoc />
    public void Write(string level, string scenario, string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        string safe = message.Replace('\n', ' ').Replace('\r', ' ');
        File.AppendAllText(_path, $"{_clock.Now:O} | {level} | {scenario} | {safe}{Environment.NewLine}");
    }
}
