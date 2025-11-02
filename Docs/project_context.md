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
 - Обновлён `MenuView`: пункт "Start New Game" вызывает `Process.StartNewGame("Data/xl.hmm", true)`.
 - Обновлён `GameProcess.StartNewGame`: синхронно инициализирует `GameWorld`, передаёт в `OverlandView`, использует `_started` в `Update` (устранено предупреждение CS0414 о неиспользуемом поле).
 - Обновлён `PPH.csproj`: добавлены `MapInfo.cs` и `GameWorld.cs` в `<Compile>`, проект успешно пересобран (`Debug|x64`).

# Pending
- Добавить тест/диагностику чтения `.phm` и `.hmm` из `LocalFolder`.
- Уточнить комментарии в `HmmReader.cs` о ролях `.phm` (игровые) и `.hmm` (редактор/диагностика).
- Расширить извлечение дополнительных полей EMAP при необходимости.
 - Реализовать инфраструктуру распаковки паков: `UnpackAsync`, `PackIndexReader`, `Gfx/Sfx/Fonts/Objects` провайдеры, кэш-менеджер (см. `Docs/resource_unpacking.md`).
 - Добавить обработку ошибок/диагностику при отсутствии карт/ресурсов в `LocalFolder` (так как фоллбек удалён).
 - Включить диагностический лог копирования/распаковки (сводка по количеству файлов и пропускам).
 - Расширить `MapInfoReader`: чтение дополнительных полей `GMAP` (версии, автора, текущего дня, режимов и т.п.) из `Map.cpp/iMapInfo::ReadMapInfo`.
 - Привязать выбор карты в меню (список `Maps/*.phm`) вместо хардкода `Data/xl.hmm`.
 - Скелет хода игрока/таймеров в `GameProcess.Update` и связка с `GameWorld`.
 - Протянуть переходы из боя обратно в мир с сохранением контекста мира.
