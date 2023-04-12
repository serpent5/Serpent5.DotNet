using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class ExceptionHandlerOptionsSetup : IConfigureOptions<ExceptionHandlerOptions>
{
    // Instead of handling the exception with special behaviour, allow it to fall-through to UseStatusCodePagesWithReExecute.
    // UseExceptionHandler handles logging and stops propagation of the exception, then UseStatusCodePagesWithReExecute returns an appropriate response.
    public void Configure(ExceptionHandlerOptions exceptionHandlerOptions)
        => exceptionHandlerOptions.ExceptionHandler = _ => Task.CompletedTask;
}
