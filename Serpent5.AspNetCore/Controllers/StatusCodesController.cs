using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Net.Http.Headers;

using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Serpent5.AspNetCore.Controllers;

[Route("/StatusCodes/{statusCode:int}")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class StatusCodesController(ICompositeViewEngine? viewEngine = null)
    : Controller
{
    private static readonly MediaTypeHeaderValue jsonMediaTypeHeaderValue = new(Application.Json);
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new(Text.Html);

    public IActionResult Index(int statusCode)
    {
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        if (statusCodeReExecuteFeature is not null)
        {
            HttpContext.Request.PathBase = statusCodeReExecuteFeature.OriginalPathBase;
            HttpContext.Request.Path = statusCodeReExecuteFeature.OriginalPath;

            if (statusCodeReExecuteFeature.OriginalQueryString is not null)
                HttpContext.Request.QueryString = new QueryString(statusCodeReExecuteFeature.OriginalQueryString);
        }
        else
        {
            statusCode = StatusCodes.Status404NotFound;
        }

        var mediaTypeHeaderValues = MediaTypeHeaderValue.ParseList(Request.Headers.Accept);

        if (mediaTypeHeaderValues.Any(x => x.IsSubsetOf(jsonMediaTypeHeaderValue)))
            return Problem(statusCode: statusCode);

        if (mediaTypeHeaderValues.Any(x => x.IsSubsetOf(htmlMediaTypeHeaderValue)))
        {
            if (FindView(statusCode, statusCodeReExecuteFeature) is { } viewResult)
                return viewResult;
        }

        return StatusCode(statusCode);
    }

    private ViewResult? FindView(int statusCode, IStatusCodeReExecuteFeature? statusCodeReExecuteFeature)
    {
        if (viewEngine is null)
            return null;

        string[] viewNameCandidates = [
            statusCode.ToString(CultureInfo.InvariantCulture),
            "Default"
        ];

        if (viewNameCandidates.FirstOrDefault(x => viewEngine.FindView(ControllerContext, x, false).Success) is not { } viewName)
            return null;

        var viewResult = View(
            viewName,
            statusCodeReExecuteFeature ?? new StatusCodeReExecuteFeature
            {
                OriginalPathBase = Request.PathBase,
                OriginalPath = Request.Path,
                OriginalQueryString = Request.QueryString.Value
            });

        viewResult.StatusCode = statusCode;

        return viewResult;
    }
}
