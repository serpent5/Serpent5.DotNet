// ReSharper disable once CheckNamespace
namespace AngleSharp.Html.Dom;

internal static class HtmlDocumentExtensions
{
    private static readonly IMarkupFormatter markupFormatter = new MinifyMarkupFormatter
    {
        ShouldKeepStandardElements = true
    };

    public static string ToMinifiedHtmlWithScriptNonce(this IHtmlDocument htmlDocument, Func<string> getNonce)
    {
        using var htmlDocumentClone = (IHtmlDocument)htmlDocument.Clone();

        htmlDocumentClone.Scripts.ToList()
            .ForEach(x => x.SetAttribute("nonce", getNonce()));

        return htmlDocumentClone.ToHtml(markupFormatter);
    }
}
