#!/bin/bash
dotnet build -c Release "$(dirname "$0")/../.."
pushd "$(dirname "$0")/../../bin/Release/net9.0" || exit
rm ./*.log
dotnet SpaceWar.dll 9000 2 replay replay.inputs &
popd || exit
