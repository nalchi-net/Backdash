dotnet build -c Release %~dp0\..\..
pushd %~dp0\..\..\bin\Release\net9.0
del *.log
start SpaceWar 9000 1 sync-test
popd
