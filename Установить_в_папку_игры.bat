@echo off
chcp 65001 >nul
title Установка ClassicCounter Wauncher в игру

echo =======================================================
echo    ClassicCounter Launcher - Автоматическая установка
echo =======================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "EXE_SRC="

if exist "%SCRIPT_DIR%wauncher.exe" (
    set "EXE_SRC=%SCRIPT_DIR%wauncher.exe"
) else if exist "%SCRIPT_DIR%Release_Build\wauncher.exe" (
    set "EXE_SRC=%SCRIPT_DIR%Release_Build\wauncher.exe"
) else if exist "%SCRIPT_DIR%..\Release_Build\wauncher.exe" (
    set "EXE_SRC=%SCRIPT_DIR%..\Release_Build\wauncher.exe"
)

if "%EXE_SRC%"=="" (
    echo [!] Ошибка: wauncher.exe не найден рядом со скриптом.
    pause
    exit /b 1
)

echo Поиск установленной игры ClassicCounter...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$desktop = [Environment]::GetFolderPath('Desktop');" ^
    "$candidates = @(" ^
    "    (Join-Path $desktop 'Desktop\classiccounter')," ^
    "    (Join-Path $desktop 'classiccounter')," ^
    "    (Join-Path $desktop 'ClassicCounter')," ^
    "    'C:\Games\ClassicCounter'," ^
    "    'D:\Games\ClassicCounter'" ^
    ");" ^
    "$found = $null;" ^
    "foreach ($c in $candidates) {" ^
    "    if (Test-Path (Join-Path $c 'csgo.exe')) { $found = $c; break; }" ^
    "}" ^
    "if (-not $found) {" ^
    "    Add-Type -AssemblyName System.Windows.Forms;" ^
    "    $fbd = New-Object System.Windows.Forms.FolderBrowserDialog;" ^
    "    $fbd.Description = 'Выберите папку с игрой ClassicCounter (где csgo.exe)';" ^
    "    if ($fbd.ShowDialog() -eq 'OK' -and (Test-Path (Join-Path $fbd.SelectedPath 'csgo.exe'))) {" ^
    "        $found = $fbd.SelectedPath;" ^
    "    }" ^
    "}" ^
    "if ($found) { Set-Content -Path '%TEMP%\cc_game_path.tmp' -Value $found -Encoding UTF8 } else { exit 1 }"

if errorlevel 1 (
    echo [!] Папка с игрой не выбрана или не содержит csgo.exe.
    pause
    exit /b 1
)

set /p GAME_DIR=<"%TEMP%\cc_game_path.tmp"
del "%TEMP%\cc_game_path.tmp" >nul 2>&1

echo [+] Найдена игра: %GAME_DIR%
echo Копирование wauncher.exe в папку игры...
copy /y "%EXE_SRC%" "%GAME_DIR%\wauncher.exe" >nul

set "MUSIC_SRC="
if exist "%SCRIPT_DIR%music" set "MUSIC_SRC=%SCRIPT_DIR%music"
if exist "%SCRIPT_DIR%Release_Build\music" set "MUSIC_SRC=%SCRIPT_DIR%Release_Build\music"
if exist "%SCRIPT_DIR%..\Release_Build\music" set "MUSIC_SRC=%SCRIPT_DIR%..\Release_Build\music"

if not "%MUSIC_SRC%"=="" (
    echo Копирование фоновой музыки...
    if not exist "%GAME_DIR%\music" mkdir "%GAME_DIR%\music" >nul
    xcopy /y /e /i "%MUSIC_SRC%\*" "%GAME_DIR%\music\" >nul
)

echo Создание ярлыка на Рабочем столе...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$desktop = [Environment]::GetFolderPath('Desktop');" ^
    "$ws = New-Object -ComObject WScript.Shell;" ^
    "$s = $ws.CreateShortcut((Join-Path $desktop 'ClassicCounter.lnk'));" ^
    "$s.TargetPath = '%GAME_DIR%\wauncher.exe';" ^
    "$s.WorkingDirectory = '%GAME_DIR%';" ^
    "$s.Description = 'ClassicCounter Launcher';" ^
    "$s.Save();"

echo.
echo =======================================================
echo  Готово! Лаунчер успешно установлен в папку игры.
echo  Ярлык 'ClassicCounter' создан на Рабочем столе.
echo =======================================================
echo.

set /p START_NOW="Запустить лаунчер прямо сейчас? (Y/N): "
if /i "%START_NOW%"=="Y" (
    start "" "%GAME_DIR%\wauncher.exe"
)
