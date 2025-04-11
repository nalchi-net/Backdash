// ReSharper disable AccessToDisposedClosure, UnusedVariable

using Backdash;
using Backdash.Core;
using Backdash.Network;
using ConsoleGame;
using GnsSharp;

const int Channel = 0;

Environment.SetEnvironmentVariable("SteamAppId", "480");
Environment.SetEnvironmentVariable("SteamGameId", "480");

if (SteamAPI.InitEx(out string? errMsg) != ESteamAPIInitResult.OK)
{
    Console.WriteLine(errMsg!);
    return 1;
}

FnSteamNetworkingMessagesSessionRequest reqFunc = (ref SteamNetworkingMessagesSessionRequest_t req) =>
{
    ISteamNetworkingMessages.User!.AcceptSessionWithUser(req.IdentityRemote);
};

if (!ISteamNetworkingUtils.User!.SetGlobalCallback_MessagesSessionRequest(reqFunc))
{
    Console.WriteLine("SetGlobalCallback_MessagesSessionRequest failed");
    return 1;
}

// Run callbacks as a seperate task
using CancellationTokenSource cancelTokenSrc = new();
CancellationToken cancelToken = cancelTokenSrc.Token;
Task callbackRunner = Task.Run(async () =>
    {
        while (!cancelToken.IsCancellationRequested)
        {
            SteamAPI.RunCallbacks();
            await Task.Delay(10, cancelToken);
        }
    }, cancelToken);

// Wait for steam auth
ISteamNetworkingUtils.User!.InitRelayNetworkAccess();
while (true)
{
    ESteamNetworkingAvailability avail =
    ISteamNetworkingSockets.User!.GetAuthenticationStatus(out SteamNetAuthenticationStatus_t auth);

    if (avail == ESteamNetworkingAvailability.Current)
        break;
    else if (!(avail == ESteamNetworkingAvailability.Waiting || avail == ESteamNetworkingAvailability.Attempting || avail == ESteamNetworkingAvailability.Retrying))
    {
        Console.WriteLine($"ESteamNetworkingAvailability.{avail} : Re-init...");
        ISteamNetworkingUtils.User!.InitRelayNetworkAccess();
    }

    await Task.Delay(10);
}

var frameDuration = FrameSpan.GetDuration(1);
using CancellationTokenSource cts = new();

// stops the game with ctr+c
Console.CancelKeyPress += (_, eventArgs) =>
{
    if (cts.IsCancellationRequested) return;
    eventArgs.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Stopping...");
};
// port and players
if (args is not [{ } portArg, { } playerCountArg, .. { } endpoints]
    || !int.TryParse(portArg, out var port)
    || !int.TryParse(playerCountArg, out var playerCount)
   )
    throw new InvalidOperationException("Invalid port argument");

// ## Netcode Configuration

// create rollback session builder
var builder = RollbackNetcode
    .WithInputType<GameInput>()
    .WithPort(Channel)
    .WithPlayerCount(playerCount)
    .WithInputDelayFrames(2)
    .WithLogLevel(LogLevel.Information)
    .WithNetworkStats()
    .ConfigureProtocol(options =>
    {
        options.NumberOfSyncRoundtrips = 10;
        // p.LogNetworkStats = true;
        // p.NetworkLatency = TimeSpan.FromMilliseconds(300);
        // p.DelayStrategy = Backdash.Network.DelayStrategy.Constant;
        // options.DisconnectTimeoutEnabled = false;
    });

// parse console arguments checking if it is a spectator
if (endpoints is ["spectate", { } hostArg] && ulong.TryParse(hostArg, out var host))
{
    var identity = new SteamNetworkingIdentity();
    identity.SetSteamID64(host);

    builder
        .WithFileLogWriter($"log_spectator_{port}.log", append: false)
        .ConfigureSpectator(options =>
        {
            options.HostEndPoint = new(identity, Channel);
        });
}
// not a spectator, creating a `remote` game session
else
{
    var players = ParsePlayers(endpoints);
    var localPlayer = players.SingleOrDefault(x => x.IsLocal())
                      ?? throw new InvalidOperationException("No local player defined");
    builder
        // Write logs in a file with player number
        .WithFileLogWriter($"log_player_{port}.log", append: false)
        .WithPlayers(players)
        .ForRemote();
}

var session = builder.Build();

// create the actual game
Game game = new(session, cts);

// set the session callbacks (like save state, load state, network events, etc..)
session.SetHandler(game);

// start background worker, like network IO, async messaging
session.Start(cts.Token);

try
{
    // kinda run a game-loop using a timer
    using PeriodicTimer timer = new(frameDuration);
    do game.Update();
    while (await timer.WaitForNextTickAsync(cts.Token));
}
catch (OperationCanceledException)
{
    // skip
}

// finishing the session
session.Dispose();
await session.WaitToStop();
Console.Clear();

// Stop the callback loop task
try
{
    await cancelTokenSrc.CancelAsync();
    await callbackRunner;
}
catch (TaskCanceledException)
{
    Console.WriteLine("Callback loop task stopped!");
}

SteamAPI.Shutdown();

return 0;

static NetcodePlayer[] ParsePlayers(IEnumerable<string> endpoints)
{
    var players = endpoints.Select(ParsePlayer).ToArray();

    if (!players.Any(x => x.IsLocal()))
        throw new InvalidOperationException("No defined local player");

    return players;
}

static NetcodePlayer ParsePlayer(string address)
{
    if (address.Equals("local", StringComparison.OrdinalIgnoreCase))
        return NetcodePlayer.CreateLocal();

    ulong steamId;

    if (address.StartsWith("s:", StringComparison.OrdinalIgnoreCase))
        if (ulong.TryParse(address[2..], out steamId))
        {
            SteamNetworkingIdentity identity = default;
            identity.SetSteamID64(steamId);
            return NetcodePlayer.CreateSpectator(new SteamEndPoint(identity, Channel));
        }
        else
            throw new InvalidOperationException("Invalid spectator endpoint");

    if (ulong.TryParse(address, out steamId))
    {
        SteamNetworkingIdentity identity = default;
        identity.SetSteamID64(steamId);
        return NetcodePlayer.CreateRemote(new(identity, Channel));
    }

    throw new InvalidOperationException($"Invalid player argument: {address}");
}
