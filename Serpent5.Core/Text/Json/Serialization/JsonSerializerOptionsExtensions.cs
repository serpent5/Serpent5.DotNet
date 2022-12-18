using JetBrains.Annotations;
using Serpent5.Core.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace System.Text.Json;

/// <summary>
///
/// </summary>
[PublicAPI]
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Trims <see cref="string" />s before reading/writing.
    /// </summary>
    /// <remarks><c>null</c>, empty, or whitespace-only strings are set to <c>null</c> when reading.</remarks>
    public static void AddStringTrimmingJsonConverter(this JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializerOptions);

        jsonSerializerOptions.Converters.Add(new StringTrimmingJsonConverter());
    }
}
