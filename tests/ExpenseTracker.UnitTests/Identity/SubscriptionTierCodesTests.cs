using ExpenseTracker.Infrastructure.Identity;

namespace ExpenseTracker.UnitTests.Identity;

[Trait("Category", "Unit")]
public sealed class SubscriptionTierCodesTests
{
    [Theory]
    [InlineData("basic", true)]
    [InlineData("pro", true)]
    [InlineData("pro_max", true)]
    [InlineData("enterprise", false)]
    [InlineData("", false)]
    public void IsValid_ReturnsExpected(string value, bool expected)
    {
        _ = SubscriptionTierCodes.IsValid(value).Should().Be(expected);
    }

    [Fact]
    public void Source_IsValid_KnownValues()
    {
        _ = SubscriptionTierSourceCodes.IsValid("default").Should().BeTrue();
        _ = SubscriptionTierSourceCodes.IsValid("admin").Should().BeTrue();
        _ = SubscriptionTierSourceCodes.IsValid("unknown").Should().BeFalse();
    }
}
