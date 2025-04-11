using System.Net;
using System.Net.Sockets;
using GnsSharp;

namespace Backdash.Network;

/// <summary>
/// Provides a <see cref="GnsSharp.SteamNetworkingIdentity" /> endpoint.
/// </summary>
public class SteamEndPoint : EndPoint
{
    /// <inheritdoc />
    public override AddressFamily AddressFamily => AddressFamily.Unspecified;

    /// <summary>
    /// Gets the internal <see cref="SteamNetworkingIdentity" />.
    /// </summary>
    public SteamNetworkingIdentity Identity { get; }

    /// <summary>
    /// Gets the channel.
    /// </summary>
    public int Channel { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="SteamEndPoint" /> with the specified <see cref="SteamNetworkingIdentity" /> and channel.
    /// </summary>
    public SteamEndPoint(SteamNetworkingIdentity identity, int channel)
    {
        Identity = identity;
        Channel = channel;
    }

    /// <inheritdoc />
    public override SocketAddress Serialize()
    {
        return Identity.ToSocketAddress();
    }

    /// <inheritdoc />
    public override EndPoint Create(SocketAddress socketAddress) => throw new NotImplementedException();

    /// <inheritdoc />
    public override string ToString() => $"{Identity}:ch{Channel}";
}
