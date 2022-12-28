namespace Serpent5.AspNetCore.Builder;

/// <summary>
/// Options for the Client UI Behavior.
/// </summary>
// ReSharper disable once InconsistentNaming
public class ClientUIBehaviorOptions
{
    /// <summary>
    /// Gets or sets the Server Address for forwarding requests.
    /// </summary>
    public Uri? ServerAddress { get; set; }
}
