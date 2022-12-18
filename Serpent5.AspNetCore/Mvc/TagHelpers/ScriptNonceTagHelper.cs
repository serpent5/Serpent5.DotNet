using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Serpent5.AspNetCore.Mvc.TagHelpers;

/// <summary>
/// An <see cref="ITagHelper"/> implementation that adds a <c>nonce</c> to &lt;script&gt; elements.
/// </summary>
[PublicAPI]
[HtmlTargetElement("script")]
public sealed class ScriptNonceTagHelper : TagHelper
{
    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/> of the executing view.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override void Process(TagHelperContext ctx, TagHelperOutput tagHelperOutput)
    {
        ArgumentNullException.ThrowIfNull(tagHelperOutput);

        tagHelperOutput.Attributes.Add("nonce", ViewContext.HttpContext.GetNonce());
    }
}
