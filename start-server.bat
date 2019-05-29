@echo off
start cmd /c dotnet run -p ./ChatApp.Server/ChatApp.Server.csproj
echo Server running... Press Ctrl + C from the server window to stop
echo You may now run "start-client.bat" to kick off a new client window