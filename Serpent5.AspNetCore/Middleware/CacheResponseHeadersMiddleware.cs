using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal sealed class CacheResponseHeadersMiddleware(RequestDelegate nextMiddleware)
{
    [UsedImplicitly]
    public Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.OnStarting(
            static httpContextAsObject =>
            {
                var httpContext = (HttpContext)httpContextAsObject;
                var httpResponseHeaders = httpContext.Response.Headers;
                var httpResponseTypedHeaders = httpContext.Response.GetTypedHeaders();

                // It's common to set "Cache-Control: no-cache, no-store".
                // According to MDN (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control):
                // - no-store means "Don't cache this, ever".
                // - no-cache means "Check with me before you use the version you cached".
                // Because this combination appears to be illogical, let's clear the "Cache-Control" header before we continue.
                if (httpResponseTypedHeaders.CacheControl is { NoCache: true, NoStore: true })
                    httpResponseTypedHeaders.CacheControl = null;

                // Immutable responses don't need a version-identifier.
                if (httpResponseTypedHeaders.CacheControl?.Extensions.Contains(new NameValueHeaderValue("immutable")) == true)
                {
                    httpResponseTypedHeaders.ETag = null;
                    httpResponseTypedHeaders.LastModified = null;
                }

                // Prefer "ETag" over "Last-Modified".
                if (httpResponseTypedHeaders.ETag is not null)
                    httpResponseTypedHeaders.LastModified = null;

                // Set a fallback "Cache-Control" header.
                if (httpResponseTypedHeaders.CacheControl is null)
                {
                    // Responses with a version identifier should be checked against the server first.
                    if (httpResponseTypedHeaders.ETag is not null || httpResponseTypedHeaders.LastModified is not null)
                        httpResponseTypedHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                    // Otherwise, the resource shouldn't be stored at all.
                    else
                        httpResponseTypedHeaders.CacheControl = new CacheControlHeaderValue { NoStore = true };
                }

                // "Cache-Control" supersedes "Expires" and "Pragma".
                httpResponseHeaders.Expires = StringValues.Empty;
                httpResponseHeaders.Pragma = StringValues.Empty;

                return Task.CompletedTask;
            },
            httpContext);

        return nextMiddleware(httpContext);
    }
}
