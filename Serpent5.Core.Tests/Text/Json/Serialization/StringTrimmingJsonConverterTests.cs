using System.Text.Json;
using System.Text.Json.Nodes;

namespace Serpent5.Core.Tests.Text.Json.Serialization;

public class StringTrimmingJsonConverterTests
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new();

    static StringTrimmingJsonConverterTests()
        => jsonSerializerOptions.AddStringTrimmingJsonConverter();

    [Fact]
    public void Deserialize_SetsStringValue()
    {
        var model = JsonSerializer.Deserialize<Model>("{ \"Value\": \"anyString\" }", jsonSerializerOptions);

        Assert.NotNull(model);
        Assert.Equal("anyString", model.Value);
    }

    [Fact]
    public void Deserialize_TrimsAndSetsStringValue()
    {
        var model = JsonSerializer.Deserialize<Model>("{ \"Value\": \" anyStringWithWhiteSpace \" }", jsonSerializerOptions);

        Assert.NotNull(model);
        Assert.Equal("anyStringWithWhiteSpace", model.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Deserialize_StringIsEmptyOrWhiteSpace_SetsStringValueToNull(string v)
    {
        var model = JsonSerializer.Deserialize<Model>($"{{ \"Value\": \"{v}\" }}", jsonSerializerOptions);

        Assert.NotNull(model);
        Assert.Null(model.Value);
    }

    [Fact]
    public void Serialize_GetsStringValue()
    {
        var modelJson = JsonSerializer.Serialize(new Model("anyString"), jsonSerializerOptions);
        var modelJsonObject = JsonSerializer.Deserialize<JsonObject>(modelJson);

        Assert.NotNull(modelJsonObject);
        Assert.Equal("anyString", modelJsonObject["Value"]?.GetValue<string>());
    }

    [Fact]
    public void Serialize_GetsAndTrimsStringValue()
    {
        var modelJson = JsonSerializer.Serialize(new Model(" anyString "), jsonSerializerOptions);
        var modelJsonObject = JsonSerializer.Deserialize<JsonObject>(modelJson);

        Assert.NotNull(modelJsonObject);
        Assert.Equal("anyString", modelJsonObject["Value"]?.GetValue<string>());
    }

    [Fact]
    public void Serialize_StringIsNull_GetsStringValueAsNull()
    {
        var modelJson = JsonSerializer.Serialize(new Model(null!), jsonSerializerOptions);
        var modelJsonObject = JsonSerializer.Deserialize<JsonObject>(modelJson);

        Assert.NotNull(modelJsonObject);
        Assert.Null(modelJsonObject["Value"]?.GetValue<string>());
    }

    private sealed record Model(string Value);
}
