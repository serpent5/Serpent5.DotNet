using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Serpent5.AspNetCore.Tests.Mvc.ApplicationModels;

public class RemoveIndexPageRouteModelConventionTests
{
    [Theory]
    [InlineData("Index")]
    [InlineData("Example/Index")]
    public void Removes_Selector_When_Route_Template_Is_Or_Ends_With_Index(string routeTemplate)
    {
        var pageConventionCollection = new PageConventionCollection()
            .RemoveIndexPageRoutes();

        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(routeTemplate);

        pageConventionCollection.Apply(fakePageRouteModel);

        Assert.Empty(fakePageRouteModel.Selectors);
    }

    [Theory]
    [InlineData("Example")]
    [InlineData("Another/Example")]
    [InlineData("Another/ExampleIndex")]
    public void Does_Not_Remove_Selector_When_Route_Template_Is_Not_And_Does_Not_End_With_Index(string routeTemplate)
    {
        var pageConventionCollection = new PageConventionCollection()
            .RemoveIndexPageRoutes();

        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(routeTemplate);

        pageConventionCollection.Apply(fakePageRouteModel);

        Assert.Single(fakePageRouteModel.Selectors);
    }

    [Fact]
    public void Removes_Selector_When_Route_Template_Does_Not_Exist()
    {
        var pageConventionCollection = new PageConventionCollection()
            .RemoveIndexPageRoutes();

        var fakePageRouteModel = CreateFakePageRouteModelWithRouteTemplate(null);

        pageConventionCollection.Apply(fakePageRouteModel);

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
