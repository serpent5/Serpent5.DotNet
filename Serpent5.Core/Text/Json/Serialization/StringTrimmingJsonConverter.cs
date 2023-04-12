// ReSharper disable once CheckNamespace
namespace System.Text.Json.Serialization;

internal sealed class StringTrimmingJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
    {
        var stringValue = jsonReader.GetString()?.Trim();

        return !string.IsNullOrEmpty(stringValue)
            ? stringValue
            : null;
    }

    public override void Write(Utf8JsonWriter jsonWriter, string? stringValue, JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonWriter);

        if (stringValue is not null)
            jsonWriter.WriteStringValue(stringValue.Trim().AsSpan());
    }
}
