using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Serpent5.AspNetCore.Mvc.TagHelpers;

namespace Serpent5.AspNetCore.Tests.Mvc.TagHelpers;

public class ScriptNonceTagHelperTests
{
    [Fact]
    public async Task AddsNonce()
    {
        var scriptNonceTagHelper = new ScriptNonceTagHelper
        {
            ViewContext = new ViewContext
            {
                HttpContext = TestFakes.HttpContext()
            }
        };

        var fakeTagHelperContext = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), string.Empty);
        var fakeTagHelperOutput = new TagHelperOutput(string.Empty, new TagHelperAttributeList(), (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await scriptNonceTagHelper.ProcessAsync(fakeTagHelperContext, fakeTagHelperOutput);

        Assert.True(fakeTagHelperOutput.Attributes.TryGetAttribute("nonce", out var nonceAttributeValue));
        Assert.Equal(scriptNonceTagHelper.ViewContext.HttpContext.GetNonce(), nonceAttributeValue.Value);
    }
}
