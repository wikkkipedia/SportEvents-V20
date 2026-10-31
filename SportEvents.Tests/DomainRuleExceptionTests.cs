using SportEvents.Core.Domain;
using Xunit;

namespace SportEvents.Tests;

public sealed class DomainRuleExceptionTests
{
    [Fact]
    public void CodeAndInnerException_ArePreserved()
    {
        IOException cause = new("Database unavailable");
        DomainRuleException error = new("EVENT_NOT_OPEN", "Event is not open.", cause);
        Assert.Equal("EVENT_NOT_OPEN", error.RuleCode);
        Assert.Same(cause, error.InnerException);
    }

    [Fact]
    public void ExistingConstructor_HasDefaultCode()
    {
        DomainRuleException error = new("Rule violated.");
        Assert.Equal("DOMAIN_RULE", error.RuleCode);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public void EmptyCode_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => new DomainRuleException(" ", "Invalid", null));
    }
}
