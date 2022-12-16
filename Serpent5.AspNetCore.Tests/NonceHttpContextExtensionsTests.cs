using Microsoft.AspNetCore.Http;

namespace Serpent5.AspNetCore.Tests;

public class NonceHttpContextExtensionsTests
{
    [Fact]
    public void TryGetNonce_HasNoNonce()
    {
        var httpContext = new DefaultHttpContext();

        var nonceExists = httpContext.TryGetNonce(out _);

        Assert.False(nonceExists);
    }

    [Fact]
    public void GetNonce_CreatesNonce()
    {
        var httpContext = new DefaultHttpContext();

        var nonceValue = httpContext.GetNonce();

        Assert.NotNull(nonceValue);
        Assert.NotEmpty(nonceValue);
    }

    [Fact]
    public void TryGetNonce_AfterGetNonce_HasValue()
    {
        var httpContext = new DefaultHttpContext();

        httpContext.GetNonce();
        var nonceExists = httpContext.TryGetNonce(out var nonceValue);

        Assert.True(nonceExists);
        Assert.NotNull(nonceValue);
        Assert.NotEmpty(nonceValue);
    }

    [Fact]
    public void GetNonce_AfterGetNonce_HasSameValue()
    {
        var httpContext = new DefaultHttpContext();

        var nonceValue = httpContext.GetNonce();
        var nonceValueAgain = httpContext.GetNonce();

        Assert.Equal(nonceValueAgain, nonceValue);
    }
}
