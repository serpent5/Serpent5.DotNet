using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Serpent5.AspNetCore.Builder.Options;
using Serpent5.Core.Text.Json.Serialization;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class JsonOptionsSetupTests
{
    [Fact]
    public void SetsDefaultIgnoreConditionToWhenWritingNull()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition);
    }

    [Fact]
    public void AddsStringTrimmingJsonConverter()
    {
        var jsonOptionsSetup = new JsonOptionsSetup();
        var jsonOptions = new JsonOptions();

        jsonOptionsSetup.Configure(jsonOptions);

        Assert.Single(jsonOptions.JsonSerializerOptions.Converters, x => x is StringTrimmingJsonConverter);
    }
}
