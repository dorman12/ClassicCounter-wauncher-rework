# ClassicCounter Wauncher (Modern Rework)

<p align="center">
  <a href="#russian-version">🇷🇺 Русский</a> &nbsp;|&nbsp; <a href="#english-version">🇬🇧 English</a>
</p>

<a name="russian-version"></a>

<p align="center">
  <img src="Wauncher/Assets/logo.png" alt="ClassicCounter Logo" width="180" />
  <h2 align="center">ClassicCounter Game Launcher &amp; Customizer</h2>
  <p align="center">
    Открытый лаунчер для проекта <b>ClassicCounter</b> на базе <b>.NET 8</b> и <b>Avalonia UI 11</b>.
    <br/>
    Динамический неон, встроенный музыкальный плеер, кастомизация звуков игры и другое.
  </p>
</p>

---

## 🛡️ Безопасность и открытость (Open Source & Clean Code)

> [!IMPORTANT]
> **Данный проект на 100% открыт (Open Source).**
> В исходном коде **полностью отсутствуют** любые скрытые процессы, стиллеры, RAT-модули, обфускация или сторонние трекеры.
> Любой пользователь может открыть исходный код проекта в Visual Studio, VS Code или Rider, проверить каждую строчку и самостоятельно собрать проект из исходников одной командой:
> ```bash
> dotnet build -c Release
> ```

### Прозрачность сетевой активности:
Лаунчер взаимодействует только с официальными открытыми игровыми сервисами:
- **Game Coordinator / Server List**: получение официального списка серверов ClassicCounter и статуса матчей.
- **Steam Web API**: безопасное отображение аватарки и статуса друзей из вашего профиля Steam.
- **Discord Rich Presence**: локальный пайп Discord IPC для отображения красивого статуса в Discord.
- **GitHub API (Releases)**: проверка наличия обновлений лаунчера.

---

## ✨ Ключевые возможности

### 🌐 Мультиязычность / Bilingual Localization (English & Russian)
- **Переключатель языка в настройках**: мгновенное переключение всего интерфейса между **Русским** и **English** в один клик без необходимости перезапуска лаунчера.
- Автоматическое сохранение выбранного языка в конфигурационном файле `wauncher_settings.cfg`.

### 📂 Автоматический поиск игры и установка в 1 клик
- **Умное автоопределение игры**: лаунчер можно запускать из любого места (из архива ZIP, папки «Загрузки» или с Рабочего стола). Он автоматически сканирует систему, мгновенно находит установленную игру `ClassicCounter` (`csgo.exe`) и подключается к ней, не требуя повторного скачивания 20 ГБ игры.
- **Выбор папки в Настройках**: в настройках лаунчера в любой момент можно вручную указать папку с игрой («Выбрать папку») или нажать «Автопоиск».
- **Скрипт «Установить_в_папку_игры.bat»**: удобный скрипт в 1 клик сам находит игру, переносит `wauncher.exe` и музыку в корневую папку игры и создает ярлык на Рабочем столе.

### 🎮 Современный интерфейс (Style Layout)
- **Верхний селектор серверов**: встроенный браузер серверов с регионами (EU, NA, RU), тикрейтом (64 / 128 Tick), текущей картой и онлайном. Опция запуска без сервера (*None*).
- **Центральный патч-ноут**: интерактивная лента обновлений проекта с Markdown-версткой.
- **Правая панель друзей**: список друзей со статусами сети и быстрая кнопка «Кастомизация».
- **Нижняя панель управления**: плашка аккаунта с аватаркой, мини-плеер фоновой музыки и кнопка запуска «Launch Game» с прогресс-баром обновлений.

### 🎨 Гибкая кастомизация и профили тем
- **Визуальный выбор цвета (ColorPicker)**: выбор любого оттенка через интерактивный цветовой спектр/круг для акцентов и базового фона.
- **Динамический неон (Neon Glow Intensity)**: ползунок интенсивности ореола с живым предпросмотром (GLOW PREVIEW).
- **Прозрачность матового стекла (Window Glass Opacity)**.
- **Сохранение в профили**: создание собственных пресетов оформления с быстрым переключением.

### 🎵 Встроенный аудиоплеер фоновой музыки
- **Встроенные треки по умолчанию**: в дистрибутив уже включены 2 легендарных трека из *Bad Piggies* (`Bad Piggies.mp3` и `Ilmari Hakkola - Bad Piggies Theme.mp3`).
- **Автостарт с мягким нарастанием (Fade-In)**: музыка плавно нарастает в течение 3 секунд при открытии лаунчера.
- **Кастомная папка**: возможность выбрать любую свою папку с треками (`.mp3`, `.wav`, `.ogg`, `.flac`).
- **Удобное управление**: предыдущий/следующий трек, пауза, кнопка Mute и ползунок громкости на нижней панели.

### 🎯 Менеджер звука трикшота (Quake Kill Sound)
- Быстрая замена звука при убийстве трикшотом (`csgo\sound\quake\perfect.mp3`).
- Кнопка **«▶ Прослушать»** для тестирования звука прямо в лаунчере.
- Кнопка **«Сбросить»** для мгновенного восстановления оригинального звука Quake из резервной копии.
- Кнопка открытия папки со звуками в проводнике Windows.

---

## 🚀 Быстрый старт (для игроков)

Вам **не нужно** вручную переносить файлы или заново скачивать десятки гигабайт игры:

1. **Скачайте архив** лаунчера со страницы [Releases](https://github.com/dorman12/ClassicCounter-wauncher-rework/releases) (`CC..rework.zip`).
2. **Распакуйте** архив в любое удобное место (на Рабочий стол, в отдельную папку и т.д.).
3. **Запустите `wauncher.exe`**:
   - Лаунчер автоматически попытается найти установленный ClassicCounter (`csgo.exe`).
   - Если игра не определилась автоматически или находится в нестандартной папке:
     - Откройте **«Настройки»** (иконка шестерёнки ⚙️ в меню).
     - В разделе **«Папка с игрой»** нажмите **«Выбрать папку»** и укажите корневую папку с игрой (где лежит `csgo.exe`), либо нажмите **«Автопоиск»**.
     - Путь мгновенно сохранится, и лаунчер сразу готов к игре!
4. Выберите сервер и нажмите **«Launch Game»**!

> [!TIP]
> Также в архиве есть скрипт `Установить_в_папку_игры.bat` — при запуске он сам найдёт корневую папку игры, скопирует туда лаунчер и создаст ярлык на Рабочем столе.

---

## 🛠️ Сборка из исходников

### Требования:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- ОС: Windows 10 / 11 (x64)

### Сборка решения:
```bash
# Клонирование репозитория
git clone https://github.com/dorman12/ClassicCounter-wauncher-rework.git
cd ClassicCounter-wauncher-rework

# Восстановление зависимостей
dotnet restore

# Сборка проекта
dotnet build Wauncher/Wauncher.csproj -c Release
```

### Публикация единого автономного `.exe`:
```bash
dotnet publish Wauncher/Wauncher.csproj -c Release -r win-x64 --self-contained false
```
Готовый исполняемый файл `wauncher.exe` будет создан в:
`Wauncher/bin/Release/net8.0-windows7.0/win-x64/publish/`

---

## 📁 Структура проекта
```
├── Launcher/               # Вспомогательный модуль автообновления
├── Wauncher/               # Основной проект лаунчера (Avalonia UI 11)
│   ├── Assets/             # Иконки, шрифты, стили, встроенная музыка
│   │   └── Music/          # Встроенные по умолчанию треки (Bad Piggies)
│   ├── Services/           # Сервисы (Discord, Музыка, Quake Sound, Сервера, Друзья)
│   ├── ViewModels/         # MVVM модели отображения (Main, Settings, Appearance)
│   └── Views/              # XAML-представления и элементы интерфейса
│       └── Controls/       # AppearancePanel, SettingsPanel, UpdateBar, FriendsList
├── Launcher.sln            # Файл решения Visual Studio
└── README.md               # Документация проекта
```

---

## 📜 Лицензия

Проект распространяется под открытой лицензией [MIT License](LICENSE).
Лаунчер разработан сообществом ClassicCounter.
Launcher rework by **dorman666**.

---
<br/>

<a name="english-version"></a>

# ClassicCounter Wauncher (Modern Rework) — English

<p align="center">
  <a href="#russian-version">🇷🇺 Русский</a> &nbsp;|&nbsp; <a href="#english-version">🇬🇧 English</a>
</p>

<p align="center">
  <img src="Wauncher/Assets/logo.png" alt="ClassicCounter Logo" width="180" />
  <h2 align="center">ClassicCounter Game Launcher &amp; Customizer</h2>
  <p align="center">
    Open-source launcher for the <b>ClassicCounter</b> project built with <b>.NET 8</b> and <b>Avalonia UI 11</b>.
    <br/>
    Dynamic neon glow, integrated music player, game audio customization, and more.
  </p>
</p>

---

## 🛡️ Security & Open Source (Clean Code)

> [!IMPORTANT]
> **This project is 100% Open Source.**
> The source code contains **no hidden background processes, stealers, RATs, obfuscation, or third-party trackers**.
> Any user can inspect the source code in Visual Studio, VS Code, or Rider, review every line, and build the project from source with a single command:
> ```bash
> dotnet build -c Release
> ```

### Network Activity Transparency:
The launcher communicates exclusively with official and open gaming endpoints:
- **Game Coordinator / Server List**: retrieving official ClassicCounter server list and match statuses.
- **Steam Web API**: securely displaying your Steam avatar and friends list status.
- **Discord Rich Presence**: local Discord IPC pipe to show rich status in Discord.
- **GitHub API (Releases)**: checking for launcher updates.

---

## ✨ Key Features

### 🌐 Bilingual Localization (English & Russian)
- **Instant language switch in Settings**: toggle between **English** and **Russian** in one click without restarting the app.
- Automatically persisted in `wauncher_settings.cfg`.

### 📂 Automatic Game Detection & 1-Click Setup
- **Smart Game Detection**: run the launcher from anywhere (ZIP archive, Downloads, or Desktop). It scans the system automatically, locates your installed `ClassicCounter` (`csgo.exe`), and connects immediately without re-downloading 20 GB of game files.
- **Select Game Folder in Settings**: manually pick your game directory ("Select folder") or click "Auto-search" at any time.
- **`Install_to_game_folder.bat` helper script**: convenient 1-click batch script to find the game, move `wauncher.exe` and music to the root game directory, and create a Desktop shortcut.

### 🎮 Modern UI (Style Layout)
- **Top Server Browser**: built-in server selector by region (EU, NA, RU), tickrate (64 / 128 Tick), current map, and player count. Option to play offline (*None*).
- **Patch Notes Center**: interactive update feed formatted with Markdown.
- **Right Friends Sidebar**: friends list with live status and quick "Customization" button.
- **Bottom Control Bar**: account widget with avatar, background music mini-player, and "Launch Game" button with update progress bar.

### 🎨 Flexible Customization & Theme Profiles
- **Visual Color Picker**: choose any color hue via interactive color spectrum/circle for accents and base background.
- **Dynamic Neon (Neon Glow Intensity)**: glow intensity slider with live GLOW PREVIEW.
- **Frosted Glass Opacity (Window Glass Opacity)**.
- **Save to Profiles**: create custom theme presets with quick switching.

### 🎵 Integrated Background Audio Player
- **Default Tracks Included**: comes pre-packaged with 2 legendary Bad Piggies tracks (`Bad Piggies.mp3` and `Ilmari Hakkola - Bad Piggies Theme.mp3`).
- **Smooth Fade-In**: volume gently fades in over 3 seconds when opening the launcher.
- **Custom Music Directory**: choose any folder with `.mp3`, `.wav`, `.ogg`, or `.flac` tracks.
- **Playback Controls**: previous/next, pause, mute button, and volume slider right on the bottom bar.

### 🎯 Quake Trickshot Kill Sound Manager
- Easily customize the trickshot kill sound (`csgo\sound\quake\perfect.mp3`).
- **"▶ Preview"** button to listen to the sound directly inside the launcher.
- **"Reset"** button to instantly restore the original Quake sound from backup.
- Explorer shortcut button to open the sounds folder directly in Windows.

---

## 🚀 Quick Start (For Players)

You **do not** need to manually move files or re-download tens of gigabytes of game data:

1. **Download the archive** from the [Releases](https://github.com/dorman12/ClassicCounter-wauncher-rework/releases) page (`CC..rework.zip`).
2. **Extract** the archive to any folder (Desktop, Games folder, etc.).
3. **Launch `wauncher.exe`**:
   - The launcher will automatically search for your installed ClassicCounter (`csgo.exe`).
   - If the game is not detected automatically or is in a custom folder:
     - Open **Settings** (gear icon ⚙️ in the menu).
     - Under **"Game folder"**, click **"Select folder"** and choose your root game directory (where `csgo.exe` is located), or click **"Auto-search"**.
     - The path is saved automatically — you're all set!
4. Choose a server and click **"Launch Game"**!

> [!TIP]
> The archive also includes `Install_to_game_folder.bat` — running it will automatically locate the game, copy the launcher files there, and create a convenient Desktop shortcut.

---

## 🛠️ Build from Source

### Prerequisites:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- OS: Windows 10 / 11 (x64)

### Building:
```bash
# Clone repository
git clone https://github.com/dorman12/ClassicCounter-wauncher-rework.git
cd ClassicCounter-wauncher-rework

# Restore dependencies
dotnet restore

# Build project
dotnet build Wauncher/Wauncher.csproj -c Release
```

### Publish Single Self-Contained Executable:
```bash
dotnet publish Wauncher/Wauncher.csproj -c Release -r win-x64 --self-contained false
```
The compiled `wauncher.exe` binary will be located in:
`Wauncher/bin/Release/net8.0-windows7.0/win-x64/publish/`

---

## 📁 Project Structure
```
├── Launcher/               # Auto-update helper module
├── Wauncher/               # Main launcher application (Avalonia UI 11)
│   ├── Assets/             # Icons, fonts, styles, built-in music
│   │   └── Music/          # Default tracks (Bad Piggies)
│   ├── Services/           # Services (Discord, Audio, Quake Sound, Servers, Friends)
│   ├── ViewModels/         # MVVM ViewModels (Main, Settings, Appearance)
│   └── Views/              # XAML views and controls
│       └── Controls/       # AppearancePanel, SettingsPanel, UpdateBar, FriendsList
├── Launcher.sln            # Visual Studio Solution
└── README.md               # Project documentation
```

---

## 📜 License

Distributed under the [MIT License](LICENSE).
Developed by the ClassicCounter community.
Launcher rework by **dorman666**.
