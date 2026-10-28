using System.Globalization;
using SportEvents.Lab06;
using Xunit;

namespace SportEvents.Tests;

public class Lab06CharacterizationTests
{
    public Lab06CharacterizationTests()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static List<SportsResultLine> Lines() => new()
    {
        new SportsResultLine { Code = "A-1", Qty = 2, Price = 150m },
        new SportsResultLine { Code = "B-2", Qty = 1, Price = 400m },
    };

    private static string Handle(SportLegacyProcessor sut,
        List<SportsResultLine>? items, string state = "new",
        string? mail = "a@b.c", string kind = "regular")
    {
        return sut.Handle(new SportRequest
        {
            DocId = 1001,
            ClientId = 7,
            ClientName = "Іван",
            ClientMail = mail,
            ClientKind = kind,
            ClientDone = 0,
            Items = items,
            State = state,
            Currency = "UAH",
            CreatedAt = new DateTime(2026, 3, 1),
            SendMail = false,
        });
    }

    [Theory]
    [InlineData("null", "new", "a@b.c", "ERR: null")]
    [InlineData("empty", "new", "a@b.c", "ERR: empty")]
    [InlineData("two", "draft", "a@b.c", "ERR: state")]
    [InlineData("two", "new", "no-mail", "ERR: mail")]
    public void Handle_InvalidInput_ReturnsExactCode(string kind, string state,
        string mail, string expected)
    {
        List<SportsResultLine>? items = kind switch
        {
            "null" => null,
            "empty" => new(),
            _ => Lines(),
        };
        Assert.Equal(expected, Handle(new SportLegacyProcessor(), items, state, mail));
    }

    [Fact]
    public void Handle_Vip_ReturnsGoldenReport()
    {
        var actual = Handle(new SportLegacyProcessor(), Lines(), kind: "vip");
        var expected = "Документ #1001\nКлієнт: Іван\n"
            + "A-1 x2 = 300.00 UAH\nB-2 x1 = 400.00 UAH\n"
            + "Знижка: 105.00 UAH\nДоставка: 60.00 UAH\nРазом: 655.00 UAH\n";
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("regular", 0, 760)]
    [InlineData("vip", 0, 655)]
    [InlineData("staff", 0, 550)]
    [InlineData("regular", 11, 725)]
    public void Preview_ByKind_ReturnsTotal(string kind, int done, int expected)
    {
        Assert.Equal((decimal)expected, new SportLegacyProcessor().Preview(kind, done, Lines()));
    }

    [Fact]
    public void Preview_FreeShippingThreshold_IsExact()
    {
        var single = new List<SportsResultLine> { new() { Code = "100m", Qty = 1, Price = 1000m } };
        Assert.Equal(1000m, new SportLegacyProcessor().Preview("regular", 0, single));
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        var participant = new SportsParticipant
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = "vip",
            DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };
        string expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж "
            + (DateTime.Now.Year - 2020);
        Assert.Equal(expected, new SportLegacyProcessor().DescribeClient(participant));
    }

    [Fact]
    public void Handle_SendMail_AddsAuditEntry()
    {
        var sut = new SportLegacyProcessor();
        sut.Handle(new SportRequest
        {
            DocId = 1001,
            ClientId = 7,
            ClientName = "Іван",
            ClientMail = "a@b.c",
            ClientKind = "regular",
            ClientDone = 0,
            Items = Lines(),
            State = "new",
            Currency = "UAH",
            CreatedAt = new DateTime(2026, 3, 1),
            SendMail = true,
        });
        Assert.Equal("ok 1001\nmail -> a@b.c\n", sut.DumpLog());
    }
}
