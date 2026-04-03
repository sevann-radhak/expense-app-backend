using ExpenseTracker.Api.Configuration;

namespace ExpenseTracker.UnitTests.Configuration;

[Trait("Category", "Unit")]
public sealed class JwtAuthenticationExtensionsTests
{
    [Theory]
    [InlineData("https://login.microsoftonline.com/tenant/v2.0", true)]
    [InlineData("https://sts.windows.net/tenant/", true)]
    [InlineData("https://contoso.ciamlogin.com/tenant/v2.0", true)]
    [InlineData("https://ExpenseTracker", false)]
    [InlineData(null, false)]
    public void LooksLikeEntraIssuer_ClassifiesMicrosoftIssuers(string? issuer, bool expected)
    {
        bool actual = JwtAuthenticationExtensions.LooksLikeEntraIssuer(issuer);
        Assert.Equal(expected, actual);
    }
}
