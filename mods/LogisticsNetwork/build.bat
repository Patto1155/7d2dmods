@echo off
setlocal
pushd "%~dp0"
dotnet build Source\LogisticsNetwork.csproj -c Release
set BUILD_EXIT=%ERRORLEVEL%
echo.
echo DLL output: %~dp0LogisticsNetwork.dll
popd
exit /b %BUILD_EXIT%
