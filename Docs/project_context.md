# Overview
- Проект: портирование Pocket Palm Heroes (C++) в единый C# (UWP, Monogame).
- Исходные модули: `pheroes` (игра), `gxlib` (графика/ввод/звук), `iolib` (утилиты), `externals` (zlib/libpng и пр.).
- Целевой проект: `PPH/PPH.csproj` c Monogame и WinSDK 10240.
- В C# уже есть демо-логика, её планируем заменить портированной игрой.
- Ресурсы сейчас в папке `Bin`; планируем переименовать и интегрировать через Content Pipeline.

# Goals
- Провести системный анализ архитектуры C++ проектов и основных компонентов.
- Определить ключевые подсистемы: игровой цикл, состояния, карта, герои, AI, UI, ввод/аудио, ресурсы.
- Сформировать план портирования в Monogame: стейт-машина, контент, ввод (тач/клавиатура), аудио.
- Инкрементально перенести логику в единый C#-проект и заменить демо.
- Обеспечить управление через тачскрин и клавиатуру в UWP.

# Progress
- [Init] Создан `Docs/project_context.md` и зафиксирован первичный обзор.
- Намечены ключевые файлы для анализа: `pheroes/Game/hmm.cpp`, `Game.cpp`, `Map.cpp`; `gxlib/gxl.winapp.cpp`, `gxl.viewmgr.h`, `gxl.input.cpp`; C# `PPH/Game1.cs`, `MainPage.xaml.cs`, `PPH.csproj`.
- Подтверждено: Monogame настроен; папка `Content` содержит `Music.xnb`, `font.xnb`, `Music.wma`.
- Получен путь к исходным ресурсам: `pheroes/bin` (будем интегрировать через Content Pipeline).
 - Проведён обзор архитектуры C++: `iGXApp` (инициализация, цикл, дисплей, ввод, звук), `iViewMgr` (менеджер вьюшек, таймеры, модалки), `iDisplay/iDib` (рендеринг, поверхность), `iInput` (клавиатура/ориентация), `iSoundPlayer` (звук). Игровая логика в `iGame` (состояния, меню, оверленд, бой), взаимодействие с `gApp.ViewMgr()`.
 - Изучен C# UWP проект: `Game1` (цикл Update/Draw, ввод Touch/Keyboard/Accelerometer, аудио через `SoundEffect/Song`), `MainPage.xaml.cs` (запуск Monogame через `XamlGame`), `PPH.csproj` (UWP/MonoGame 3.8, контент XNB). Это будет точкой интеграции портированного движка.
 - Зафиксировано каноническое расположение ресурсов: `pheroes\bin`.
- Начата формализация плана портирования (см. `Docs/porting_plan.md`).
- [Impl] Добавлен каркас `ViewManager` и интерфейс `IView` (базовые функции Update/Draw).
- [Impl] Созданы заглушки экранов: `MenuView`, `OverlandView`, `BattleView`; интегрированы в `Game1` (переключатель `usePPHMode`).
- [Next] Готов вывод базового меню PPH через `SpriteBatch` и `font.xnb`.
 - [Impl] Ввод нормализован: `InputRouter`, `InputState`, `IInputConsumer`; `ViewManager` делегирует ввод активной вьюшке.
 - [Refactor] Удалены демо-файлы `MazeCell/MazeChamber/MazeGrid/Enemy` из проекта.
- [Refactor] Переименован namespace `TiltMaze` → `PPH` во всех C# и XAML, обновлён `Package.appxmanifest` (`EntryPoint=PPH.App`).
- [Refactor] Переписан `Game1.cs`: убран `usePPHMode` и вся TiltMaze-логика; оставлен чистый цикл Monogame, `ViewManager.Push(new MenuView)`.
 - [Impl] Связаны переходы: Menu → Overland (Enter), Overland → Battle (B), Battle → Overland (Esc).
- [Impl] Добавлена `DiagnosticsView`: базовый тест ввода (клавиатура/мышь/тач) и рендера; пункт меню "Diagnostics".
- [Init] Добавлен `Content.mgcb` и `MonoGame.Content.Builder.Task` в проект; создан `Docs/content_pipeline_readme.md`.
 - [Assets] Скопированы исходные ресурсы из `pheroes/bin` в `PPH/Content/AssetsRaw` (структура сохранена).
 - [MGCB] Обновлён `Content.mgcb`: `ContentRoot=AssetsRaw`; добавлены сборки PNG (`iologo.png`, `cell_*`, `ctl_*`, `node_sel.png`) и тестовый шрифт `fonts/ui.spritefont`.
 - [Diagnostics] Расширена `DiagnosticsView`: загрузка `iologo` (Texture2D) из Content Pipeline, проигрывание SFX (`player_hit`) [P] и музыки (`Music`) [M]; отображение статуса контента.
 - [Build] Проект успешно собирается (Debug, UWP); Content Pipeline компилирует указанные ассеты.

# Pending
- Сформировать детальный документ плана портирования (этапы, метрики) и согласовать.
- Подготовить стратегию переноса ресурсов (переименование `pheroes/bin` → `AssetsRaw`/`ContentRaw`, настройка импортёров в Content Pipeline).
 - Интегрировать маршрутизацию ввода к активной вьюшке (тач/мышь/клавиатура).
 - Скелет интеграции: маппинг ввода (`Keyboard/Mouse/Touch`), рендер (`SpriteBatch`), аудио (`SoundEffect/Song`).
 - Проба импорта ключевых ассетов и тест отрисовки базового экрана меню.
- Начать перенос `iGame.Process` и связывание со вьюшками (меню → оверленд → бой).
- Интеграция ассетов: переименовать `pheroes/bin` → `AssetsRaw`, прописать импортёры в `Content.mgcb`, начать добавление ключевых ассетов.
 - Интеграция ассетов: продолжить добавление PNG/шрифтов/иных ресурсов в `Content.mgcb` (тайлы, UI-элементы), проверить доступность в вьюшках.
 - Расширить `DiagnosticsView`: тест отрисовки тайловой сетки, базовая проверка альфа/блендинга, загрузка шрифта `ui` из Pipeline.
 - Расширить тесты диагностики (отрисовка спрайта/тайла, проверка музыки/звук эффекта).
 - Проверить сборку проекта после рефакторинга; устранить возможные ошибки компиляции.
