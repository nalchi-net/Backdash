using Backdash.Network;
using GnsSharp;

namespace Backdash.Options;

/// <summary>
///     Configurations for <see cref="INetcodeSession{TInput}" /> in <see cref="SessionMode.Spectator" /> mode.
/// </summary>
public sealed record SpectatorOptions
{
    /// <summary>
    ///     Host endpoint <see cref="SteamNetworkingIdentity" />.
    /// </summary>
    public SteamNetworkingIdentity HostAddress { get; set; }

    /// <summary>
    ///     Host endpoint port
    /// </summary>
    /// <value>Defaults to 0</value>
    public int HostPort { get; set; } = 0;

    /// <summary>
    ///     Host IP endpoint
    /// </summary>
    public SteamEndPoint HostEndPoint
    {
        get => new(HostAddress, HostPort);
        set => (HostAddress, HostPort) = (value.Identity, value.Channel);
    }
}
