dotnet build -c Release %~dp0\..\..
pushd %~dp0\..\..\bin\Release\net9.0
del *.log
start SpaceWar 9000 2 --save-to replay.inputs local 127.0.0.1:9001
start SpaceWar 9001 2 127.0.0.1:9000 local
popd
