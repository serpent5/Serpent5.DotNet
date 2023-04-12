using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public static class JsonOptionsSetupTests
{
    // ReSharper disable once InconsistentNaming
    public class Minimal_APIs
    {
        [Fact]
        public void Sets_DefaultIgnoreCondition_To_WhenWritingNull()
        {
            var jsonOptionsSetup = new JsonOptionsSetup();
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();

            jsonOptionsSetup.Configure(jsonOptions);

            Assert.Equal(JsonIgnoreCondition.WhenWritingNull, jsonOptions.SerializerOptions.DefaultIgnoreCondition);
        }

        [Fact]
        public void Adds_StringTrimmingJsonConverter()
        {
            var jsonOptionsSetup = new JsonOptionsSetup();
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions();

            jsonOptionsSetup.Configure(jsonOptions);

            Assert.Single(jsonOptions.SerializerOptions.Converters, x => x is StringTrimmingJsonConverter);
        }
    }

#pragma warning disable CA1724 // Type names should not match namespaces
    // ReSharper disable once InconsistentNaming
    public class MVC
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        [Fact]
        public void Sets_DefaultIgnoreCondition_To_WhenWritingNull()
        {
            var jsonOptionsSetup = new JsonOptionsSetup();
            var jsonOptions = new Microsoft.AspNetCore.Mvc.JsonOptions();

            jsonOptionsSetup.Configure(jsonOptions);

            Assert.Equal(JsonIgnoreCondition.WhenWritingNull, jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition);
        }

        [Fact]
        public void Adds_StringTrimmingJsonConverter()
        {
            var jsonOptionsSetup = new JsonOptionsSetup();
            var jsonOptions = new Microsoft.AspNetCore.Mvc.JsonOptions();

            jsonOptionsSetup.Configure(jsonOptions);

            Assert.Single(jsonOptions.JsonSerializerOptions.Converters, x => x is StringTrimmingJsonConverter);
        }
    }
}
