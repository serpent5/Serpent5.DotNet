using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Serpent5.AspNetCore.Authentication;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides options for <see cref="DistributedCacheTicketStore" />.
/// </summary>
[PublicAPI]
public sealed class DistributedCacheTicketStoreOptions
{
    /// <summary>
    /// <para>Gets or sets how long <see cref="AuthenticationTicket" /> remains valid.</para>
    /// Defaults to 12 hours (see <see href="https://pages.nist.gov/800-63-3/sp800-63b.html#433-reauthentication" />).
    /// </summary>
    public TimeSpan Expires { get; set; } = TimeSpan.FromHours(12);

    /// <summary>
    /// <para>Gets or sets how long <see cref="AuthenticationTicket" /> can be inactive before being removed, up until <see cref="Expires" /> elapses.</para>
    /// Defaults to 15 minutes (see <see href="https://pages.nist.gov/800-63-3/sp800-63b.html#433-reauthentication" />).
    /// </summary>
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(15);
}
