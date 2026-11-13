@echo off
echo ========================================================
echo   Publishing ClassicCounter Wauncher (Standalone x64)
echo ========================================================
dotnet publish Wauncher/Wauncher.csproj -c Release -r win-x64 --self-contained false -o Publish/
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Publish failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo   [SUCCESS] Published to Publish/wauncher.exe
echo ========================================================
pause
