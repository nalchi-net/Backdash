#!/bin/bash
source "$(dirname "$0")/build_server.sh"
pushd "$(dirname "$0")/../../bin/Release/net9.0" || exit
rm ./*.log

dotnet SpaceWar.dll -ServerURl "$LOBBY_SERVER_URL" -Username player -LocalPort 9000 &
dotnet SpaceWar.dll -ServerURl "$LOBBY_SERVER_URL" -Username player -LocalPort 9001 &
dotnet SpaceWar.dll -ServerURl "$LOBBY_SERVER_URL" -Username player -LocalPort 9002 &

popd || exit
source "$(dirname "$0")/start_server.sh"
