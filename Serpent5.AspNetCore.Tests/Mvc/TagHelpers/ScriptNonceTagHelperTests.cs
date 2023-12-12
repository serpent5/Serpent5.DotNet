using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Serpent5.AspNetCore.Mvc.TagHelpers;

namespace Serpent5.AspNetCore.Tests.Mvc.TagHelpers;

public class ScriptNonceTagHelperTests
{
    [Fact]
    public async Task Sets_Nonce_Attribute()
    {
        var fakeHttpContext = TestFakes.HttpContext();
        var nonceValue = fakeHttpContext.GetNonce();

        var scriptNonceTagHelper = new ScriptNonceTagHelper
        {
            ViewContext = new ViewContext
            {
                HttpContext = fakeHttpContext
            }
        };

        var fakeTagHelperOutput = CreateFakeTagHelperOutput();

        await scriptNonceTagHelper.ProcessAsync(CreateFakeTagHelperContext(), fakeTagHelperOutput);

        Assert.True(fakeTagHelperOutput.Attributes.TryGetAttribute("nonce", out var nonceAttributeValue));
        Assert.Equal(nonceValue, nonceAttributeValue.Value);
    }

    private static TagHelperContext CreateFakeTagHelperContext()
        => new([], new Dictionary<object, object>(), string.Empty);

    private static TagHelperOutput CreateFakeTagHelperOutput()
        => new(
            string.Empty,
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
}
