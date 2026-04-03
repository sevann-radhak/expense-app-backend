using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Endpoints;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.UnitTests.Endpoints;

[Trait("Category", "Unit")]
public sealed class DevBookRequestValidationTests
{
    [Fact]
    public void TryValidate_MissingUserId_ReturnsFalseWithError()
    {
        DefaultHttpContext http = new();
        DevDataOptions options = new() { RequireSharedSecret = false };
        DevBookUserRequest body = new() { UserId = null };

        bool ok = DevBookRequestValidation.TryValidate(http, options, body, out _, out IResult? error);

        _ = ok.Should().BeFalse();
        _ = error.Should().NotBeNull();
    }

    [Fact]
    public void TryValidate_RequireSecretMissingHeader_ReturnsFalseWithError()
    {
        DefaultHttpContext http = new();
        DevDataOptions options = new() { RequireSharedSecret = true, SharedSecret = "expected" };
        DevBookUserRequest body = new() { UserId = "valid-user" };

        bool ok = DevBookRequestValidation.TryValidate(http, options, body, out _, out IResult? error);

        _ = ok.Should().BeFalse();
        _ = error.Should().NotBeNull();
    }

    [Fact]
    public void TryValidate_WrongSecret_ReturnsFalseWithError()
    {
        DefaultHttpContext http = new();
        http.Request.Headers["X-Dev-Data-Secret"] = "wrong";
        DevDataOptions options = new() { RequireSharedSecret = true, SharedSecret = "expected" };
        DevBookUserRequest body = new() { UserId = "valid-user" };

        bool ok = DevBookRequestValidation.TryValidate(http, options, body, out _, out IResult? error);

        _ = ok.Should().BeFalse();
        _ = error.Should().NotBeNull();
    }

    [Fact]
    public void TryValidate_UserIdTooLong_ReturnsFalseWithError()
    {
        DefaultHttpContext http = new();
        DevDataOptions options = new() { RequireSharedSecret = false };
        DevBookUserRequest body = new() { UserId = new string('a', 451) };

        bool ok = DevBookRequestValidation.TryValidate(http, options, body, out _, out IResult? error);

        _ = ok.Should().BeFalse();
        _ = error.Should().NotBeNull();
    }

    [Fact]
    public void TryValidate_ValidSecretAndUserId_ReturnsTrue()
    {
        DefaultHttpContext http = new();
        http.Request.Headers["X-Dev-Data-Secret"] = "expected";
        DevDataOptions options = new() { RequireSharedSecret = true, SharedSecret = "expected" };
        DevBookUserRequest body = new() { UserId = "valid-user" };

        bool ok = DevBookRequestValidation.TryValidate(http, options, body, out string userId, out IResult? error);

        _ = ok.Should().BeTrue();
        _ = error.Should().BeNull();
        _ = userId.Should().Be("valid-user");
    }
}
