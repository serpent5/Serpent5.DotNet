using System.Security.Claims;

namespace Serpent5.Core.Tests.Security.Claims;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetNameIdentifier_Gets_Value_Of_NameIdentifier_Claim()
    {
        const string fakeNameIdentifier = TestFakes.String;
        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, fakeNameIdentifier)
            }));

        Assert.Equal(fakeNameIdentifier, claimsPrincipal.GetNameIdentifier());
    }

    [Fact]
    public void GetNameIdentifier_Throws_InvalidOperationException_When_NameIdentifier_Claim_Does_Not_Exist()
        => Assert.Throws<InvalidOperationException>(new ClaimsPrincipal().GetNameIdentifier);
}
