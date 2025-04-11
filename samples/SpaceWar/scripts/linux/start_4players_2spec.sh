#!/bin/bash
dotnet build -c Release "$(dirname "$0")/../.."
pushd "$(dirname "$0")/../../bin/Release/net9.0" || exit
rm ./*.log
dotnet SpaceWar.dll 9000 4 local 127.0.0.1:9001 127.0.0.1:9002 127.0.0.1:9003 s:127.0.0.1:9100 &
dotnet SpaceWar.dll 9001 4 127.0.0.1:9000 local 127.0.0.1:9002 127.0.0.1:9003 s:127.0.0.1:9101 &
dotnet SpaceWar.dll 9002 4 127.0.0.1:9000 127.0.0.1:9001 local 127.0.0.1:9003 &
dotnet SpaceWar.dll 9003 4 127.0.0.1:9000 127.0.0.1:9001 127.0.0.1:9002 local &
dotnet SpaceWar.dll 9100 4 spectate 127.0.0.1:9000 &
dotnet SpaceWar.dll 9101 4 spectate 127.0.0.1:9001 &
popd || exit
