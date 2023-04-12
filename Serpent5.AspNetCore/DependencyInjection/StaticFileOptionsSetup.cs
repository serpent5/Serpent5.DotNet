using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal partial class StaticFileOptionsSetup : IConfigureOptions<StaticFileOptions>
{
    private readonly IHostEnvironment hostEnvironment;

    public StaticFileOptionsSetup(IHostEnvironment hostEnvironment)
        => this.hostEnvironment = hostEnvironment;

    public void Configure(StaticFileOptions staticFileOptions)
    {
        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider
        {
            // https://webhint.io/docs/user-guide/hints/hint-content-type/
            Mappings =
            {
                [".css"] = "text/css; charset=utf-8",
                [".js"] = "text/javascript; charset=utf-8",
                [".svg"] = "image/svg+xml; charset=utf-8",
                [".txt"] = "text/plain; charset=utf-8",
                [".xml"] = "text/xml; charset=utf-8"
            }
        };

        // Disable direct requests for .html files.
        fileExtensionContentTypeProvider.Mappings.Remove(".html");

        staticFileOptions.ContentTypeProvider = fileExtensionContentTypeProvider;

        if (!hostEnvironment.IsDevelopment())
        {
            staticFileOptions.OnPrepareResponse = static ctx =>
            {
                CacheControlHeaderValue? cacheControlHeaderValue = null;

                // TODO: Configurable "AngularImmutableFilenameRegex".
                // ASP.NET Core uses a "v" query-string parameter to identify immutable responses.
                if (ctx.Context.Request.Query.ContainsKey("v") || AngularImmutableFilenameRegex().IsMatch(ctx.File.Name))
                {
                    cacheControlHeaderValue = new CacheControlHeaderValue
                    {
                        MaxAge = TimeSpan.FromDays(365),
                        Extensions =
                        {
                            new NameValueHeaderValue("immutable")
                        }
                    };
                }

                ctx.Context.Response.GetTypedHeaders().CacheControl = cacheControlHeaderValue
                    ?? new CacheControlHeaderValue
                    {
                        NoCache = true
                    };
            };
        }
    }

    private const string angularImmutableFilenameRegexPattern = @"^.+\.[\da-f]{16}\..+";
    private const RegexOptions angularImmutableFilenameRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant;

#if NET7_0_OR_GREATER
    [GeneratedRegex(angularImmutableFilenameRegexPattern, angularImmutableFilenameRegexOptions)]
    private static partial Regex AngularImmutableFilenameRegex();
#else
    private static Regex AngularImmutableFilenameRegex()
        => new(angularImmutableFilenameRegexPattern, angularImmutableFilenameRegexOptions);
#endif
}
