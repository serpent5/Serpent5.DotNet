using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.ApplicationModels;

internal sealed class RemoveIndexPageRouteModelConvention : IPageRouteModelConvention
{
    private static readonly Regex endsWithIndex = new(
        @"^Index$|\/Index$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public void Apply(PageRouteModel pageRouteModel)
    {
        ArgumentNullException.ThrowIfNull(pageRouteModel);

        var indexSelectorModel = pageRouteModel.Selectors.SingleOrDefault(
            x => !string.IsNullOrWhiteSpace(x.AttributeRouteModel?.Template) && endsWithIndex.IsMatch(x.AttributeRouteModel.Template));

        if (indexSelectorModel is null)
            return;

        pageRouteModel.Selectors.Remove(indexSelectorModel);
    }
}
