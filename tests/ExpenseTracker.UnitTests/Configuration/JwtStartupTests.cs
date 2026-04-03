using ExpenseTracker.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ExpenseTracker.UnitTests.Configuration;

[Trait("Category", "Unit")]
public sealed class JwtStartupTests
{
    [Fact]
    public void Resolve_Production_MissingKey_Throws()
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "",
            ["Jwt:DevelopmentFallbackSigningKey"] = "Development-only-Jwt-Key-Minimum32Chars!",
        }).Build();
        Mock<IHostEnvironment> env = new();
        _ = env.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        Action act = () => JwtStartup.Resolve(config, env.Object);

        _ = act.Should().Throw<InvalidOperationException>().WithMessage("*SigningKey*");
    }

    [Fact]
    public void Resolve_Development_UsesFallback_WhenPrimaryMissing()
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "",
            ["Jwt:DevelopmentFallbackSigningKey"] = "Development-only-Jwt-Key-Minimum32Chars!",
        }).Build();
        Mock<IHostEnvironment> env = new();
        _ = env.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        JwtOptions result = JwtStartup.Resolve(config, env.Object);

        _ = result.SigningKey.Should().Be("Development-only-Jwt-Key-Minimum32Chars!");
    }

    [Fact]
    public void Resolve_PrefersExplicitSigningKey_WhenValid()
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "12345678901234567890123456789012",
            ["Jwt:DevelopmentFallbackSigningKey"] = "Development-only-Jwt-Key-Minimum32Chars!",
        }).Build();
        Mock<IHostEnvironment> env = new();
        _ = env.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        JwtOptions result = JwtStartup.Resolve(config, env.Object);

        _ = result.SigningKey.Should().Be("12345678901234567890123456789012");
    }
}
