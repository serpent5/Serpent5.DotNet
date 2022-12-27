using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Builder.Options;

// ReSharper disable once PartialTypeWithSinglePart
internal partial class StaticFileOptionsProductionSetup : IConfigureOptions<StaticFileOptions>
{
    private const string angularImmutableFilenameRegexPattern = @"^.+\.[\da-f]{16}\..+";
    private const RegexOptions angularImmutableFilenameRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;

    public void Configure(StaticFileOptions staticFileOptions)
    {
        staticFileOptions.OnPrepareResponse = static ctx =>
        {
            CacheControlHeaderValue? cacheControlHeaderValue = null;

            // Support immutable "asp-append-version" responses and Angular hashed filenames.
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

#if NET7_0_OR_GREATER
    [GeneratedRegex(angularImmutableFilenameRegexPattern, angularImmutableFilenameRegexOptions)]
    private static partial Regex AngularImmutableFilenameRegex();
#else
    private static Regex AngularImmutableFilenameRegex()
        => new(angularImmutableFilenameRegexPattern, angularImmutableFilenameRegexOptions);
#endif
}
