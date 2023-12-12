// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <summary>
/// Extension methods for removing Razor Pages route templates /Index and */Index.
/// </summary>
public static class RemoveIndexPageRouteModelConventionExtensions
{
    /// <summary>
    /// Adds a <see cref="IPageRouteModelConvention" /> that removes Razor Pages route templates /Index and */Index.
    /// </summary>
    /// <param name="pageConventionCollection"></param>
    /// <returns>The created <see cref="IPageRouteModelConvention" />.</returns>
    public static IPageRouteModelConvention RemoveIndexPageRoutes(this PageConventionCollection pageConventionCollection)
    {
        ArgumentNullException.ThrowIfNull(pageConventionCollection);

        var removeIndexPageRouteModelConvention = new RemoveIndexPageRouteModelConvention();

        pageConventionCollection.Add(removeIndexPageRouteModelConvention);

        return removeIndexPageRouteModelConvention;
    }
}
