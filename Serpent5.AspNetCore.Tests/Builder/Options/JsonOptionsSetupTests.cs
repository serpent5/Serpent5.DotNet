using System.Text.Json.Serialization;
using Serpent5.AspNetCore.Builder.Options;
using Serpent5.Core.Text.Json.Serialization;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class JsonOptionsSetupTests
{
    [Fact]
    public void MinimalAPIs_SetsDefaultIgnoreConditionToWhenWritingNull()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, jsonOptions.SerializerOptions.DefaultIgnoreCondition);
    }

    [Fact]
    public void MinimalAPIs_AddsStringTrimmingJsonConverter()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Single(jsonOptions.SerializerOptions.Converters, x => x is StringTrimmingJsonConverter);
    }

    [Fact]
    public void MVC_SetsDefaultIgnoreConditionToWhenWritingNull()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new Microsoft.AspNetCore.Mvc.JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition);
    }

    [Fact]
    public void MVC_AddsStringTrimmingJsonConverter()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new Microsoft.AspNetCore.Mvc.JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Single(jsonOptions.JsonSerializerOptions.Converters, x => x is StringTrimmingJsonConverter);
    }
}
