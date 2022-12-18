using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serpent5.AspNetCore.Mvc.ApplicationModels;

namespace Serpent5.AspNetCore.Tests.Mvc.ApplicationModels;

public class RemoveIndexPageRouteModelConventionTests
{
    [Theory]
    [InlineData("Index")]
    [InlineData("Example/Index")]
    public void RouteTemplateIsOrEndsWithIndex_RemovesSelector(string routeTemplate)
    {
        var removeIndexPageRouteModelConvention = new RemoveIndexPageRouteModelConvention();
        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(routeTemplate);

        removeIndexPageRouteModelConvention.Apply(fakePageRouteModel);

        Assert.Empty(fakePageRouteModel.Selectors);
    }

    [Theory]
    [InlineData("Example")]
    [InlineData("Another/Example")]
    [InlineData("Another/ExampleIndex")]
    public void RouteTemplateIsNotOrDoesNotEndWithIndex_DoesNotRemoveSelector(string routeTemplate)
    {
        var removeIndexPageRouteModelConvention = new RemoveIndexPageRouteModelConvention();
        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(routeTemplate);

        removeIndexPageRouteModelConvention.Apply(fakePageRouteModel);

        Assert.Single(fakePageRouteModel.Selectors);
    }

    [Fact]
    public void RouteTemplateIsMissing_RemovesSelector()
    {
        var removeIndexPageRouteModelConvention = new RemoveIndexPageRouteModelConvention();
        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(null);

        removeIndexPageRouteModelConvention.Apply(fakePageRouteModel);

        Assert.Single(fakePageRouteModel.Selectors);
    }

    private static PageRouteModel CreateFakePageRouteModelWithRouteTemplate(string? routeTemplate)
        => new(string.Empty, string.Empty)
        {
            Selectors =
            {
                new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = routeTemplate
                    }
                }
            }
        };
}
