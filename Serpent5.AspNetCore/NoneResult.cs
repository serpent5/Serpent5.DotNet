using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// An <see cref="ActionResult" /> that, when executed, does nothing.
/// </summary>
[PublicAPI]
public sealed class NoneResult : IActionResult
{
    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext ctx)
        => Task.CompletedTask;
}
