dotnet build -c Debug
set /P PeerSteamId=Input peer's SteamId:
set /P PlayerIndex=Input your player index (0 or 1):
If /I "%PlayerIndex%"=="0" goto zero
If /I "%PlayerIndex%"=="1" goto one
echo Player Index is invalid!
exit

:zero
start dotnet run --no-build 0 2 local steamid:%PeerSteamId%
exit

:one
start dotnet run --no-build 0 2 steamid:%PeerSteamId% local
exit
