@echo off
cd ./ChatApp.Server
dotnet restore
dotnet ef database update
cd ../