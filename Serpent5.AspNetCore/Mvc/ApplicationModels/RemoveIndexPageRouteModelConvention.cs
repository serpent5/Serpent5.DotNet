using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Serpent5.AspNetCore.Mvc.ApplicationModels;

internal sealed class RemoveIndexPageRouteModelConvention : IPageRouteModelConvention
{
    private static readonly Regex endsWithIndex = new(
        @"^Index$|\/Index$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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
