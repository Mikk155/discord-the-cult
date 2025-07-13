@echo off

setlocal
set TOKEN="Your bot token"

cd src

dotnet run %TOKEN%

pause
