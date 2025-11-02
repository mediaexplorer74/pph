# Overview
Palm Heroes (PPH) UWP/MonoGame порт. Ресурсы оригинальной игры упакованы в `Data` (исходники) и собраны в рантайм-пакеты (`game.gfx`, `game.sfx`, `fonts.dat`). Карты рантайма — `Maps/*.phm` с заголовком `GMAP`. Локальное хранилище UWP (`ApplicationData.Current.LocalFolder`) используется для сохранений и конфигурации.

# Goals
- Читать заголовки EMAP/GMAP и показывать сводку.
- Деплоить (распаковывать) игровые ресурсы и карты в `LocalFolder` при старте.
- Хранить `PalmHeroes.cfg` и папку `Save` в `LocalFolder`.

# Progress
- Подтверждены пути и форматы C++: `Maps/*.phm` (`GMAP`), редактор `EMAP`; загрузчики GFX/SFX/LOC.
- Проект успешно собирается (AppxBundle отключен).
- Добавлены в проект как Content: `Maps/**` и `PalmHeroes.cfg` (копирование в выходной каталог).
- Реализован `RuntimeAssets.DeployAsync`: создаёт `Data`, `Maps`, `Save` в `LocalFolder`; копирует `fonts.dat`, `game.gfx`, `game.pix`, `game.sfx`, `objects.dat`; копирует `Two_by_the_river_English.phm`, `Tutorial_English.phm`; размещает `PalmHeroes.cfg`.
- Обновлён `App.OnLaunched` на `async` и вызов `RuntimeAssets.DeployAsync` до инициализации UI.
- Добавлен `RuntimeAssets.cs` в сборку (`PPH.csproj`), устранены ошибки CS0103.
- Обновлён `HmmReader.ReadHeaderAsync`: приоритет чтения из `LocalFolder` с фоллбеком в пакет (`ms-appx:///`).
- Упрощён `HmmReader.ReadHeaderAsync`: теперь читает ТОЛЬКО из `LocalFolder`; фоллбек на пакет (`ms-appx:///`) удалён после введения кэша.
- Явно включён `Data/objects.dat` в проект как Content (устранён пропуск при копировании).
- В `RuntimeAssets.DeployAsync` добавлено создание папки `Cache` для будущей распаковки.
- Создан документ `Docs/resource_unpacking.md` с планом потокового доступа и физической распаковки.
- Сборка проверена через MSBuild: `MSBuild.exe PPH.sln /t:Restore` и `msbuild PPH.sln /t:Build /p:Configuration=Debug /p:Platform=x64 /p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=false`.
 - Стадии портирования (сводка):
   - 1) Базовый каркас — выполнено: `ViewManager`, `IView`, цикл `Update/Draw`, `InputRouter`, базовые `Menu/Overland/Battle` заглушки.
   - 2) Контент и ресурсы — в процессе: деплой паков/карт в `LocalFolder`, подготовка механизма распаковки/стриминга.
   - 3) Игровая логика — частично: заготовка `GameProcess`, требуется перенос `iGame.*` стейт-машины.
   - 4) UI и взаимодействия — не начато: попапы/диалоги, тултипы.
   - 5) Бой и AI — не начато.
   - 6) Сохранения и совместимость — база готова: `Save` и `PalmHeroes.cfg` в `LocalFolder`.
   - 7) Тестирование и полировка — не начато.
 - Перенесён каркас `iGame::Process`: добавлены флаги `_goToMainMenu`, `_started`, методы `MainMenu/StartNewGame/ExitGame/Update`, интеграция в `Game1.Update`.
 - Добавлены классы `MapInfo` и `MapInfoReader` (чтение заголовка карты через `HmmReader`).
 - Создан скелет `GameWorld` (инициализация из `MapInfoReader`), пропертя ширины/высоты и имени карты.
- Обновлён `OverlandView`: принимает путь карты и `GameWorld`, выводит метаданные либо лениво читает заголовок.
  - Обновлён `GameProcess.StartNewGame`: синхронно инициализирует `GameWorld`, передаёт в `OverlandView`, использует `_started` в `Update` (устранено предупреждение CS0414 о неиспользуемом поле).
  - Расширены `MapInfo` и `HmmReader`: добавлены поля `CurrentDay`, `GameMode`, `Difficulty`, `PlayerCount`, `CurrentPlayerId`, список `Players` с базовыми метаданными игрока для `GMAP`.
  - Исправлена инициализация списка игроков в `HmmReader` (`new List<PlayerInfo>()`).
  - Добавлен `MapListView`: перечисляет карты из `LocalFolder/Maps/*.phm`, даёт выбор стрелками и запуск по Enter.
  - Обновлён `MenuView`: пункт "Start New Game" открывает `MapListView` (убран хардкод пути).
  - Добавлен скелет обновления мира: `GameWorld.Update(GameTime)` и вызов из `GameProcess.Update` при состоянии Overland.
  - Заведен флаг возврата из боя (`_returningFromBattle`) и переход `ExitBattleToOverland()` с передачей текущего `World`.
  - Обновлён `PPH.csproj`: добавлен `MapListView.cs` в `<Compile>`, проект успешно пересобран (`Debug|x64`).
  - Добален `Clock` и `TurnManager`, связаны с `GameWorld` для игрового времени и очередности ходов.
  - `OverlandView`: восстановление позиции камеры из `GameWorld` и сохранение перед переходом в бой; вывод даты (Month/Week/Day) по текущему дню.
  - `BattleResult`: простой контейнер результатов; `BattleView` при выходе в overland передаёт заглушку результата; `GameProcess.ExitBattleToOverland(BattleResult)` применяет результат к `GameWorld`, `OverlandView` показывает краткую сводку.
  - `MapListView`: отображает доп. поля — сложность, режим, текущий день и краткое описание.
- Сборка ошибок: устранён `CS0246`/`GameTime` добавлением `using Microsoft.Xna.Framework;` в `GameWorld.cs`; устранён `CS4036` в `MapListView` корректным использованием `await` UWP API.
 - Исправлены ошибки сборки `CS0246` (не найдены типы `BattleResult`, `Clock`, `TurnManager`) добавлением недостающих `<Compile Include>` в `PPH.csproj`.

# Pending
- Добавить тест/диагностику чтения `.phm` и `.hmm` из `LocalFolder`.
- Уточнить комментарии в `HmmReader.cs` о ролях `.phm` (игровые) и `.hmm` (редактор/диагностика).
- Расширить извлечение дополнительных полей EMAP при необходимости.
 - Реализовать инфраструктуру распаковки паков: `UnpackAsync`, `PackIndexReader`, `Gfx/Sfx/Fonts/Objects` провайдеры, кэш-менеджер (см. `Docs/resource_unpacking.md`).
 - Добавить обработку ошибок/диагностику при отсутствии карт/ресурсов в `LocalFolder` (так как фоллбек удалён).
 - Включить диагностический лог копирования/распаковки (сводка по количеству файлов и пропускам).
 - Расширить парсинг `GMAP`: дополнить поля игроков (ресурсы, герои, замки, ключи) и обеспечить совместимость с разными версиями `GMAP`.
 - Связать переход хода из UI с `TurnManager.EndTurn()` и визуально отражать текущего игрока.
 - Уточнить применение `BattleResult`: учёт потерь/ресурсов/событий карты вместо заглушки; восстановление положения камеры для нескольких слоёв.
 - Добавить hot-reload предпросмотра UI (пригодится для проверки визуальных изменений без развертывания UWP).
 - Улучшить `MapListView`: сортировка/фильтрация, предпросмотр описания/сложности, отображение текущего дня.
 - Скелет хода игрока/таймеров: разнести в отдельные компоненты (TurnManager/Clock), связать с `GameWorld`.
 - Контекст возврата из боя: добавить сущность результата боя и обновление мира (потери, эффекты), восстановление позиции камеры.
