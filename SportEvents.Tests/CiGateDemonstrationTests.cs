using Xunit;

namespace SportEvents.Tests;

/// <summary>Навмисне тимчасове порушення для демонстрації червоного CI на PR.</summary>
public sealed class CiGateDemonstrationTests
{
    [Fact]
    public void CiGuard_Demonstration_WillFailUntilFixed()
    {
        Assert.Equal(2, 1); // Навмисно: червоний CI. У наступному коміті виправити.
    }
}
