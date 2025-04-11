dotnet build -c Release %~dp0\..\..
pushd %~dp0\..\..\bin\Release\net9.0
del *.log
start SpaceWar 0 2 local-only
popd
