using SportEvents.Core.Application;
using SportEvents.Core.Infrastructure;
using Xunit;

namespace SportEvents.Tests;

public sealed class InfrastructureTests
{
    [Fact] public void JsonRepository_FileAbsent_EmptyData()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        Assert.Empty(new JsonEventRepository(path).Load().Events);
    }

    [Fact] public void JsonRepository_SaveAndLoad_PreservesEvent()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            JsonEventRepository repository = new(path);
            DataSnapshot data = new();
            data.Events.Add(new(1, "Тест", new DateOnly(2026, 10, 24), 1000,
                SportEvents.Core.Domain.EventStatus.Planned));
            repository.Save(data);
            Assert.Equal("Тест", repository.Load().Events.Single().Title);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact] public void JsonRepository_InvalidJson_Throws()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            File.WriteAllText(path, "not json");
            Assert.Throws<System.Text.Json.JsonException>(() => new JsonEventRepository(path).Load());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact] public void FileAuditLogger_Write_RecordsScenarioAndLevel()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log");
        try
        {
            new FileAuditLogger(path, new FixedClock()).Write("Warning", "UC-05", "Невдалий перехід");
            string log = File.ReadAllText(path);
            Assert.Contains("Warning | UC-05", log);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
