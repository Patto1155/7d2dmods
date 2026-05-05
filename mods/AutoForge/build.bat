@echo off
dotnet build Source\AutoForge.csproj -c Release
echo.
echo DLL output: %~dp0AutoForge.dll
