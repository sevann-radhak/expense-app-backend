using ExpenseTracker.Infrastructure.Services;

namespace ExpenseTracker.UnitTests.Services;

[Trait("Category", "Unit")]
public sealed class DevBookDataServiceTests
{
    [Fact]
    public void ValidateUserId_Empty_ThrowsArgumentException()
    {
        Action act = () => DevBookDataService.ValidateUserId(string.Empty);
        _ = act.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Fact]
    public void ValidateUserId_Exceeds450Chars_ThrowsArgumentException()
    {
        string longId = new('a', 451);
        Action act = () => DevBookDataService.ValidateUserId(longId);
        _ = act.Should().Throw<ArgumentException>().WithMessage("*450*");
    }

    [Fact]
    public void ValidateUserId_Valid_DoesNotThrow()
    {
        Action act = () => DevBookDataService.ValidateUserId("user-123");
        _ = act.Should().NotThrow();
    }
}
