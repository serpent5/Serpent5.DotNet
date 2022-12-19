using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class JsonOptionsSetup : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions jsonOptions)
    {
        jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        jsonOptions.JsonSerializerOptions.AddStringTrimmingJsonConverter();
    }
}
