using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Extensions for throwing <see cref="ArgumentNullException" /> and <see cref="ArgumentException" />.
/// </summary>
public static class ArgumentExceptionExtensions
{
    /// <summary>
    /// <summary>Throws an <see cref="ArgumentNullException" /> if <paramref name="inputValue" /> is null or an <see cref="ArgumentException" /> if it's empty or whitespace.</summary>
    /// </summary>
    /// <param name="inputValue">The <see cref="string" /> to validate.</param>
    /// <param name="nameofInputValue">The name of the <paramref name="inputValue" /> parameter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="inputValue" /> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="inputValue" /> is empty or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? inputValue, [CallerArgumentExpression("inputValue")] string? nameofInputValue = null)
    {
        if (!string.IsNullOrWhiteSpace(inputValue))
            return;

        ArgumentNullException.ThrowIfNull(inputValue, nameofInputValue);
        throw new ArgumentException("Value cannot be empty or whitespace.", nameofInputValue);
    }
}
