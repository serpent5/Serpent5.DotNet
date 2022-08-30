using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Middleware.CacheResponseHeaders;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal class CacheResponseHeadersMiddleware
{
    private readonly RequestDelegate nextMiddleware;

    public CacheResponseHeadersMiddleware(RequestDelegate nextMiddleware)
        => this.nextMiddleware = nextMiddleware;

    [PublicAPI]
    public Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.OnStarting(static httpContextAsObject =>
        {
            var httpContext = (HttpContext)httpContextAsObject;
            var httpResponseHeaders = httpContext.Response.GetTypedHeaders();

            // It's common to set "Cache-Control: no-cache, no-store".
            // According to MDN (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control):
            // - no-store means "Don't cache this, ever".
            // - no-cache means "Check with me before you use the cache".
            // Because this doesn't seem to be a logical combination, let's clear the value and let the "no-store" fallback kick in.
            if (httpResponseHeaders.CacheControl is { NoCache: true, NoStore: true })
                httpResponseHeaders.CacheControl = null;

            // Prefer "ETag" over "Last-Modified".
            if (httpResponseHeaders.ETag is not null)
                httpResponseHeaders.LastModified = null;

            // Immutable responses don't need a version-identifier, simply because they can't change.
            if (httpResponseHeaders.CacheControl?.Extensions.Contains(new NameValueHeaderValue("immutable")) == true)
                httpResponseHeaders.ETag = null;

            // Set a default, fallback "Cache-Control" header.
            if (httpResponseHeaders.CacheControl is null)
            {
                // Resources with a version identifier should be checked against the server first.
                if (httpResponseHeaders.ETag is not null || httpResponseHeaders.LastModified is not null)
                    httpResponseHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                // Otherwise, the resource shouldn't be cached.
                else
                    httpResponseHeaders.CacheControl = new CacheControlHeaderValue { NoStore = true };
            }

            // "Cache-Control" supersedes "Expires" and "Pragma".
            httpContext.Response.Headers.Expires = StringValues.Empty;
            httpContext.Response.Headers.Pragma = StringValues.Empty;

            return Task.CompletedTask;
        }, httpContext);

        return nextMiddleware(httpContext);
    }
}
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
