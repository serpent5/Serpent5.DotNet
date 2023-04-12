using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace System.Text.Json;

/// <summary>
/// Extensions for <see cref="JsonSerializerOptions" />.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Adds a <see cref="JsonConverter" /> that trims <see cref="string" />s before reading/writing.
    /// </summary>
    /// <remarks>Empty and whitespace-only strings are read as <c>null</c>.</remarks>
    public static void AddStringTrimmingJsonConverter(this JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializerOptions);

        jsonSerializerOptions.Converters.Add(new StringTrimmingJsonConverter());
    }
}
