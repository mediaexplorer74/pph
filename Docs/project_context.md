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

# Pending
- Добавить тест/диагностику чтения `.phm` и `.hmm` из `LocalFolder`.
- Уточнить комментарии в `HmmReader.cs` о ролях `.phm` (игровые) и `.hmm` (редактор/диагностика).
- Расширить извлечение дополнительных полей EMAP при необходимости.
