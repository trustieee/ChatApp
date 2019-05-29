[![Build Status](https://dev.azure.com/mariocatch0780/ChatApp/_apis/build/status/mariocatch.ChatApp?branchName=master)](https://dev.azure.com/mariocatch0780/ChatApp/_build/latest?definitionId=1&branchName=master)

# ChatApp
A multi-user authenticated chat app using asp core 3 + signalr

### Requirements
* [Visual Studio 2019 Preview](https://visualstudio.microsoft.com/vs/preview/)
* [Dotnet SDK 3.0.100-preview5-011568](https://dotnet.microsoft.com/download/dotnet-core/3.0)

### Run
```
git clone https://github.com/mariocatch/ChatApp.git ChatApp
cd ./ChatApp/ChatApp.Server
dotnet ef database update
cd ../
dotnet run -p ./ChatApp.Server/ChatApp.Server.csproj
dotnet run -p ./ChatApp.Client/ChatApp.Client.csproj # run this 'n' times for each client you want
```
or
```
git clone https://github.com/mariocatch/ChatApp.git ChatApp
cd ./ChatApp
build-and-stage.bat
start-server.bat
start-client.bat # run this 'n' times for each client you want
```

### Coming Soon / Issues
[You can view the existing issues, which also contains future enhancements here](https://github.com/mariocatch/ChatApp/issues)
