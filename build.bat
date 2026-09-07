@echo off
setlocal
cd /d "%~dp0"
dotnet build MissionPlanner.csproj -c Release
exit /b %errorlevel%
