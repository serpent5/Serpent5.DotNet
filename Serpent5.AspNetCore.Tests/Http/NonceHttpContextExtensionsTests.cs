using Microsoft.AspNetCore.Http;

namespace Serpent5.AspNetCore.Tests.Http;

public class NonceHttpContextExtensionsTests
{
    [Fact]
    public void GetNonce_Creates_Nonce()
    {
        var httpContext = TestFakes.HttpContext();

        var nonceValue = httpContext.GetNonce();

        Assert.NotNull(nonceValue);
        Assert.NotEmpty(nonceValue);
    }

    [Fact]
    public void GetNonce_Creates_Nonce_Once()
    {
        var httpContext = TestFakes.HttpContext();

        var nonceValue = httpContext.GetNonce();
        var nonceValueAgain = httpContext.GetNonce();

        Assert.Equal(nonceValueAgain, nonceValue);
    }

    [Fact]
    public void TryGetNonce_Gets_Created_Nonce()
    {
        var httpContext = TestFakes.HttpContext();

        httpContext.GetNonce();
        var nonceExists = httpContext.TryGetNonce(out var nonceValue);

        Assert.True(nonceExists);
        Assert.NotNull(nonceValue);
        Assert.NotEmpty(nonceValue);
    }

    [Fact]
    public void TryGetNonce_Does_Not_Create_Nonce()
    {
        var httpContext = TestFakes.HttpContext();

        var nonceExists = httpContext.TryGetNonce(out _);

        Assert.False(nonceExists);
    }
}
