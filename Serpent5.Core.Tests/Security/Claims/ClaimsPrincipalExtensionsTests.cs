using System.Security.Claims;

namespace Serpent5.Core.Tests.Security.Claims;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetNameIdentifier_GetsValueOfNameIdentifierClaim()
    {
        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "anyString")
            }));

        Assert.Equal("anyString", claimsPrincipal.GetNameIdentifier());
    }

    [Fact]
    public void GetNameIdentifier_NameIdentifierClaimIsMissing_ThrowsInvalidOperationException()
    {
        var claimsPrincipal = new ClaimsPrincipal();

        Assert.Throws<InvalidOperationException>(claimsPrincipal.GetNameIdentifier);
    }
}
