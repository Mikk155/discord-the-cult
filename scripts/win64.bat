@echo off

cd ..
cd src

echo Compiling TheCult.csproj...
dotnet publish TheCult.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true

if %ERRORLEVEL% NEQ 0 (
    echo ERROR
    pause
    exit /b %ERRORLEVEL%
)

echo Moving release...
xcopy /E /Y bin\Release\net9.0\win-x64\publish\TheCult.exe ..\release\windows\

@echo off

echo Clearing obj...
rmdir /S /Q obj

echo Clearing bin...
rmdir /S /Q bin

echo All done!

pause
