using System.Net;
using System.Net.Sockets;
using Backdash.Core;
using GnsSharp;

namespace Backdash.Network.Client;

/// <summary>
///     <a href="https://partner.steamgames.com/doc/api/ISteamNetworkingMessages">Steam Networking Messages</a> socket interface.
/// </summary>
public sealed class SteamSocket : IPeerSocket
{
    readonly ISteamNetworkingMessages steamNetMsgs;

    /// <summary>
    ///     Gets the channel number to use for Steam Networking Messages.
    /// </summary>
    public int Port { get; }

    /// <inheritdoc cref="Socket.AddressFamily"/>
    public AddressFamily AddressFamily => AddressFamily.Unspecified;

    /// <summary>
    /// Initialized a new <see cref="SteamSocket" />.
    /// </summary>
    /// <param name="channel">Channel number to use for <see cref="ISteamNetworkingMessages.SendMessageToUser" />.</param>
    /// <param name="steamNetMsgs">Steam networking messages interface to use.</param>
    public SteamSocket(int channel, ISteamNetworkingMessages steamNetMsgs)
    {
        Port = channel;
        this.steamNetMsgs = steamNetMsgs;
    }

    /// <summary>
    ///     Receives a datagram into the data buffer, using the specified SocketFlags, and stores the endpoint.
    /// </summary>
    /// <param name="buffer">The buffer for the received data.</param>
    /// <param name="address">
    ///     A <see cref="SocketAddress " /> instance that gets updated with the value of the remote peer
    ///     when this method returns.
    ///     The internal representation of <see cref="SocketAddress.Buffer" /> is <see cref="SteamNetworkingIdentity" />.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<int> ReceiveFromAsync(Memory<byte> buffer, SocketAddress address, CancellationToken cancellationToken)
    {
        return new ValueTask<int>(Task.Run<int>(
            async () =>
            {
                IntPtr[] msgPtrs = new IntPtr[1];

                try
                {
                    while (true)
                    {
                        int msgReceived = steamNetMsgs.ReceiveMessagesOnChannel(Port, msgPtrs);
                        if (msgReceived != 0)
                            break;

                        await Task.Delay(1, cancellationToken);
                    }

                    int payloadSize;
                    unsafe
                    {
                        ref readonly var msg = ref new ReadOnlySpan<SteamNetworkingMessage_t>((void*)msgPtrs[0], 1)[0];
                        payloadSize = msg.Size;

                        ReadOnlySpan<byte> payload = new((void*)msg.Data, msg.Size);
                        payload.CopyTo(buffer.Span);

                        ref var identity = ref address.AsSteamNetworkingIdentity();

                        identity = msg.IdentityPeer;
                    }

                    return payloadSize;
                }
                finally
                {
                    if (msgPtrs[0] != IntPtr.Zero)
                        SteamNetworkingMessage_t.Release(msgPtrs[0]);
                }
            }
            , cancellationToken
        ));
    }

    /// <inheritdoc />
    public ValueTask<SocketReceiveFromResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <summary>
    ///     Sends data to the specified steam networking identity host.
    /// </summary>
    /// <param name="buffer">The buffer for the data to send.</param>
    /// <param name="socketAddress">
    ///     The remote host to which to send the data.
    ///     The internal representation of <see cref="SocketAddress.Buffer" /> must be <see cref="SteamNetworkingIdentity" />.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns></returns>
    public ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, SocketAddress socketAddress, CancellationToken cancellationToken)
    {
        ref SteamNetworkingIdentity identity = ref socketAddress.AsSteamNetworkingIdentity();

        EResult result = steamNetMsgs.SendMessageToUser(identity, buffer.Span, ESteamNetworkingSendType.UnreliableNoNagle, Port);

        if (result != EResult.OK)
            return ValueTask.FromException<int>(new GnsEResultException(result));

        return ValueTask.FromResult<int>(buffer.Length);
    }

    /// <inheritdoc />
    public ValueTask<int> SendToAsync(ReadOnlyMemory<byte> buffer, EndPoint remoteEndPoint, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public void Close() { }
}
