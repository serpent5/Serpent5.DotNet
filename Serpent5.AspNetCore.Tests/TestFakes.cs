using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace Serpent5.AspNetCore.Tests;

internal static class TestFakes
{
    public static IMemoryCache MemoryCache()
        => new MemoryCache(Options.Create(new MemoryCacheOptions()));

    public static IWebHostEnvironment WebHostEnvironment()
        => Mock.Of<IWebHostEnvironment>();

    public static HttpContext HttpContext()
        => new DefaultHttpContext();

    public static ControllerContext ControllerContext()
        => new() { HttpContext = HttpContext() };
}
