dotnet build -c Debug
start dotnet run --no-build -- 9000 2 local ip:127.0.0.1:9001 s:ip:127.0.0.1:9100
start dotnet run --no-build -- 9001 2 ip:127.0.0.1:9000 local
start dotnet run --no-build -- 9100 2 spectate ip:127.0.0.1:9000
