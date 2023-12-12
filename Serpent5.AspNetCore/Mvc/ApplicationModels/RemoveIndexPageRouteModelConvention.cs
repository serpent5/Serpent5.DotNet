using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.ApplicationModels;

internal sealed partial class RemoveIndexPageRouteModelConvention : IPageRouteModelConvention
{
    /// <inheritdoc />
    public void Apply(PageRouteModel pageRouteModel)
    {
        ArgumentNullException.ThrowIfNull(pageRouteModel);

        var indexSelectorModel = pageRouteModel.Selectors.SingleOrDefault(
            x => !string.IsNullOrWhiteSpace(x.AttributeRouteModel?.Template) && IndexSuffixRegex().IsMatch(x.AttributeRouteModel.Template));

        if (indexSelectorModel is null)
            return;

        pageRouteModel.Selectors.Remove(indexSelectorModel);
    }

    [GeneratedRegex(@"^Index$|\/Index$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IndexSuffixRegex();
}
