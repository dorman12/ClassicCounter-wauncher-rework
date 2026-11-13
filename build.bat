@echo off
echo ========================================================
echo   Building ClassicCounter Wauncher (Release)
echo ========================================================
dotnet build Wauncher/Wauncher.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed! Check errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [SUCCESS] Build finished successfully!
pause
