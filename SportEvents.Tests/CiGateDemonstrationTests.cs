using Xunit;

namespace SportEvents.Tests;

/// <summary>Перевірка того, що CI блокує некоректний тест і приймає виправлення.</summary>
public sealed class CiGateDemonstrationTests
{
    [Fact]
    public void CiGuard_Demonstration_PassesAfterFix()
    {
        Assert.Equal(2, 1 + 1);
    }
}
