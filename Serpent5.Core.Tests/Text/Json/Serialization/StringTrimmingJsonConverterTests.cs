using System.Text.Json;
using System.Text.Json.Nodes;

namespace Serpent5.Core.Tests.Text.Json.Serialization;

public class StringTrimmingJsonConverterTests
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new();

    static StringTrimmingJsonConverterTests()
        => jsonSerializerOptions.AddStringTrimmingJsonConverter();

    [Fact]
    public void Deserialize_Reads_Value()
    {
        var testModel = JsonSerializer.Deserialize<TestModel>("{ \"Value\": \"anyString\" }", jsonSerializerOptions);

        Assert.NotNull(testModel);
        Assert.Equal("anyString", testModel.Value);
    }

    [Fact]
    public void Deserialize_Reads_And_Trims_Value()
    {
        var testModel = JsonSerializer.Deserialize<TestModel>("{ \"Value\": \" anyStringWithWhiteSpace \" }", jsonSerializerOptions);

        Assert.NotNull(testModel);
        Assert.Equal("anyStringWithWhiteSpace", testModel.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Deserialize_Reads_String_Value_As_Null_When_Value_Is_Empty_Or_WhiteSpace(string v)
    {
        var testModel = JsonSerializer.Deserialize<TestModel>($"{{ \"Value\": \"{v}\" }}", jsonSerializerOptions);

        Assert.NotNull(testModel);
        Assert.Null(testModel.Value);
    }

    [Fact]
    public void Serialize_Writes_Value()
    {
        var testModelJson = JsonSerializer.Serialize(new TestModel("anyString"), jsonSerializerOptions);
        var testModelJsonObject = JsonSerializer.Deserialize<JsonObject>(testModelJson);

        Assert.NotNull(testModelJsonObject);
        Assert.Equal("anyString", testModelJsonObject["Value"]?.GetValue<string>());
    }

    [Fact]
    public void Serialize_Trims_And_Writes_Value()
    {
        var testModelJson = JsonSerializer.Serialize(new TestModel(" anyString "), jsonSerializerOptions);
        var testModelJsonObject = JsonSerializer.Deserialize<JsonObject>(testModelJson);

        Assert.NotNull(testModelJsonObject);
        Assert.Equal("anyString", testModelJsonObject["Value"]?.GetValue<string>());
    }

    [Fact]
    public void Serialize_Writes_Null_Value_As_Null()
    {
        var testModelJson = JsonSerializer.Serialize(new TestModel(null!), jsonSerializerOptions);
        var testModelJsonObject = JsonSerializer.Deserialize<JsonObject>(testModelJson);

        Assert.NotNull(testModelJsonObject);
        Assert.Null(testModelJsonObject["Value"]?.GetValue<string>());
    }

    private sealed record TestModel(string Value);
}
