using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class JsonOptionsSetup : IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>, IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>
{
    public void Configure(Microsoft.AspNetCore.Http.Json.JsonOptions jsonOptions)
        => ConfigureJsonSerializerOptions(jsonOptions.SerializerOptions);

    public void Configure(Microsoft.AspNetCore.Mvc.JsonOptions jsonOptions)
        => ConfigureJsonSerializerOptions(jsonOptions.JsonSerializerOptions);

    private static void ConfigureJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        jsonSerializerOptions.AddStringTrimmingJsonConverter();
    }
}
