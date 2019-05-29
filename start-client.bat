@echo off
start cmd /c dotnet run -p ./ChatApp.Client/ChatApp.Client.csproj
echo Client started... Press Ctrl + C from the client window to stop
echo You can start additional clients with "start-client.bat"