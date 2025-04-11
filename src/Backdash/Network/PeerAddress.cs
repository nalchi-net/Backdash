using System.Net;

namespace Backdash.Network;

sealed class PeerAddress(SteamEndPoint endPoint) : IEquatable<PeerAddress>
{
    public SteamEndPoint EndPoint { get; } = endPoint;
    public SocketAddress Address { get; } = endPoint.Serialize();

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is PeerAddress other && Equals(other));

    public bool Equals(PeerAddress? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || EndPoint.Identity == other.EndPoint.Identity;
    }

    public override int GetHashCode() => HashCode.Combine(EndPoint, Address);

    public static implicit operator PeerAddress(SteamEndPoint endPoint) => new(endPoint);
    public static implicit operator SteamEndPoint(PeerAddress peerAddress) => peerAddress.EndPoint;
    public static implicit operator SocketAddress(PeerAddress peerAddress) => peerAddress.Address;

    public static bool operator ==(PeerAddress? left, PeerAddress? right) => Equals(left, right);
    public static bool operator !=(PeerAddress? left, PeerAddress? right) => !Equals(left, right);
}
