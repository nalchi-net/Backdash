using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Backdash.Network;
using Backdash.Network.Protocol;
using GnsSharp;

namespace Backdash;

/// <summary>
/// Public extensions that are visible to the end user.
/// </summary>
public static class PublicExtensions
{
    /// <summary>
    /// Converts <see cref="SteamNetworkingIdentity"/> to <see cref="SocketAddress"/>.
    /// </summary>
    /// <param name="identity">Steam Networking identity.</param>
    /// <returns></returns>
    public static SocketAddress ToSocketAddress(this in SteamNetworkingIdentity identity)
    {
        unsafe
        {
            var addr = new SocketAddress(AddressFamily.Unspecified, sizeof(SteamNetworkingIdentity));

            var identitySpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in identity, 1));
            identitySpan.CopyTo(addr.Buffer.Span);

            return addr;
        }
    }
}

static class InternalExtensions
{
    public static void EnqueueNext<T>(this Queue<T> queue, in T value)
    {
        var count = queue.Count;
        queue.Enqueue(value);
        for (var i = 0; i < count; i++)
            queue.Enqueue(queue.Dequeue());
    }

    public static PlayerConnectionStatus ToPlayerStatus(this ProtocolStatus status) => status switch
    {
        ProtocolStatus.Syncing => PlayerConnectionStatus.Syncing,
        ProtocolStatus.Running => PlayerConnectionStatus.Connected,
        ProtocolStatus.Disconnected => PlayerConnectionStatus.Disconnected,
        _ => PlayerConnectionStatus.Unknown,
    };

    public static IEnumerable<string> SplitToLines(this string value, int size)
    {
        var chunks = value.Chunk(size);
        foreach (var chars in chunks)
            yield return new(chars);
    }

    public static string BreakToLines(this string value, int size) =>
        string.Join('\n', value.SplitToLines(size));

    public static ref SteamNetworkingIdentity AsSteamNetworkingIdentity(this SocketAddress addr)
    {
        return ref MemoryMarshal.AsRef<SteamNetworkingIdentity>(addr.Buffer.Span);
    }
}
