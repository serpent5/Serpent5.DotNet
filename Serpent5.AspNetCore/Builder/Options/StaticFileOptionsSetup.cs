using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class StaticFileOptionsSetup : IConfigureOptions<StaticFileOptions>
{
    public void Configure(StaticFileOptions staticFileOptions)
    {
        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider
        {
            // https://webhint.io/docs/user-guide/hints/hint-content-type/
            Mappings =
            {
                [".css"] = "text/css; charset=utf-8",
                [".js"] = "text/javascript; charset=utf-8",
                [".txt"] = "text/plain; charset=utf-8",
                [".xml"] = "text/xml; charset=utf-8"
            }
        };

        // Disable direct requests for .html files.
        fileExtensionContentTypeProvider.Mappings.Remove(".html");

        staticFileOptions.ContentTypeProvider = fileExtensionContentTypeProvider;
    }
}
