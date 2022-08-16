using System.Security.Claims;

namespace Serpent5.Core.Tests;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetNameIdentifierGetsValueOfNameIdentifierClaim()
    {
        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "ANY_STRING")
            }));

        Assert.Equal("ANY_STRING", claimsPrincipal.GetNameIdentifier());
    }

    [Fact]
    public void GetNameIdentifierThrowsInvalidOperationExceptionWhenNameIdentifierClaimIsMissing()
    {
        var claimsPrincipal = new ClaimsPrincipal();

        Assert.Throws<InvalidOperationException>(() => claimsPrincipal.GetNameIdentifier());
    }
}
