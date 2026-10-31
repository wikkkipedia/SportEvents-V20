using System.Globalization;
using Microsoft.Extensions.Logging;
using SportEvents.Core.Application;
using SportEvents.Core.Domain;
using SportEvents.Core.Infrastructure;

namespace SportEvents.ConsoleApp;

internal static class Program
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private static readonly Dictionary<string, Action<EventService>> MenuActions = new()
    {
        ["1"] = s => s.AddParticipant(ReadInt("Номер: "), ReadText("Ім'я: "), ReadDate("Дата народження yyyy-MM-dd: ")),
        ["2"] = Create,
        ["3"] = s => s.OpenEvent(ReadInt("Номер змагання: ")),
        ["4"] = s => s.Enroll(ReadInt("Номер змагання: "), ReadInt("Номер учасника: ")),
        ["5"] = Record,
        ["6"] = s => s.FinishEvent(ReadInt("Номер змагання: ")),
        ["7"] = s => s.CancelEvent(ReadInt("Номер змагання: ")),
        ["8"] = Search,
        ["9"] = ShowProtocol,
    };

    private static void Main()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "data");
        IClock clock = new SystemClock();
        IEventRepository repo = new JsonEventRepository(Path.Combine(folder, "events.json"));
        IAuditLogger log = new FileAuditLogger(Path.Combine(folder, "audit.log"), clock);
        // Seed demo records without flooding the user-facing console with 60 entries.
        EventService seedService = new(repo, clock, new RankingPolicy(), log);
        seedService.SeedDemoParticipants();

        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "HH:mm:ss ";
                options.IncludeScopes = true;
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        EventService service = new(repo, clock, new RankingPolicy(), log, factory.CreateLogger<EventService>());
        System.Console.WriteLine($"SportEvents v{typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "unknown"}");
        System.Console.WriteLine("SportEvents | варіант 20 | демонстраційна база: 60 учасників");
        RunMenu(service, log);
    }

    private static void RunMenu(EventService service, IAuditLogger log)
    {
        while (true)
        {
            ShowMenu();
            string? choice = System.Console.ReadLine()?.Trim();
            if (choice is null || choice == "0")
            {
                break;
            }

            ExecuteMenuChoice(choice, service, log);
        }
    }

    private static void ExecuteMenuChoice(string choice, EventService service, IAuditLogger log)
    {
        try
        {
            Perform(choice, service);
        }
        catch (Exception ex) when (ex is ArgumentException or DomainRuleException or FormatException)
        {
            log.Write("Warning", "MENU", ex.GetType().Name);
            System.Console.WriteLine("Помилка: " + ex.Message);
        }
        catch (System.Text.Json.JsonException ex)
        {
            log.Write("Error", "JSON", ex.GetType().Name);
            System.Console.WriteLine("Некоректний JSON-файл: " + ex.Message);
        }
        catch (IOException ex)
        {
            log.Write("Error", "IO", ex.GetType().Name);
            System.Console.WriteLine("Не вдалося прочитати або записати файл: " + ex.Message);
        }

    }

    private static void ShowMenu()
    {
        System.Console.WriteLine("\n1 Новий учасник | 2 Створити змагання | 3 Відкрити реєстрацію");
        System.Console.WriteLine("4 Зареєструвати на старт | 5 Внести час | 6 Завершити");
        System.Console.WriteLine("7 Скасувати | 8 Знайти | 9 Протокол | 0 Вихід");
        System.Console.Write("Ваш вибір: ");
    }

    private static void Perform(string? choice, EventService service)
    {
        if (!MenuActions.TryGetValue(choice ?? string.Empty, out Action<EventService>? action))
        {
            System.Console.WriteLine("Невідомий пункт меню.");
            return;
        }

        action(service);
        System.Console.WriteLine("Операцію виконано.");
    }

    private static void Create(EventService service)
    {
        int id = service.CreateEvent(ReadText("Назва: "), ReadDate("Дата yyyy-MM-dd: "), ReadInt("Дистанція, м: "));
        System.Console.WriteLine("Створено змагання #" + id);
    }

    private static void Record(EventService service)
    {
        int id = ReadInt("Номер змагання: ");
        int person = ReadInt("Номер учасника: ");
        TimeSpan time = TimeSpan.ParseExact(ReadText("Час hh:mm:ss.fff: "), "c", Culture);
        service.RecordResult(id, person, time);
    }

    private static void Search(EventService service)
    {
        string name = ReadText("Назва або частина (Enter — усі): ", false);
        string textDate = ReadText("Дата yyyy-MM-dd (Enter — усі): ", false);
        DateOnly? date = textDate.Length == 0 ? null : DateOnly.ParseExact(textDate, "yyyy-MM-dd", Culture);
        foreach (SportsEvent item in service.FindEvents(name, date))
        {
            System.Console.WriteLine($"#{item.Id} | {item.EventDate:yyyy-MM-dd} | {item.Title} | {item.Status}");
        }
    }

    private static void ShowProtocol(EventService service)
    {
        int id = ReadInt("Номер змагання: ");
        foreach (ProtocolRow row in service.GenerateProtocol(id))
        {
            string elapsed = row.Elapsed.ToString(@"hh\:mm\:ss\.fff", Culture);
            System.Console.WriteLine($"{row.Group,-8} {row.Place,2} | #{row.ParticipantId} | {row.Name} | {elapsed}");
        }
    }

    private static string ReadText(string prompt, bool required = true)
    {
        System.Console.Write(prompt);
        string result = (System.Console.ReadLine() ?? string.Empty).Trim();
        if (required && result.Length == 0)
        {
            throw new ArgumentException("Значення не може бути порожнім.");
        }

        return result;
    }

    private static int ReadInt(string prompt)
        => int.Parse(ReadText(prompt), Culture);

    private static DateOnly ReadDate(string prompt)
        => DateOnly.ParseExact(ReadText(prompt), "yyyy-MM-dd", Culture);
}
