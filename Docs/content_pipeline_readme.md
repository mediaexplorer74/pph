# Content Pipeline: старт интеграции

Этот проект использует MonoGame Content Pipeline для подготовки ассетов (текстуры, шрифты, звук) в формат XNB.

## План действий
- Переименовать `pheroes/bin` → `AssetsRaw` (или `ContentRaw`) и перенести папку рядом с `PPH/Content`.
- Откорректировать `PPH/Content/Content.mgcb`:
  - Обновить `/ContentRoot` на путь к `AssetsRaw`.
  - Добавлять нужные файлы с подходящими импортёрами/процессорами.
- Убедиться, что сборка UWP использует пакет `MonoGame.Content.Builder.Task` и файл `Content.mgcb` (уже подключено в `PPH.csproj`).

## Импортёры/процессоры (черновик)
- Изображения (`.png`): `TextureImporter` + `TextureProcessor`.
- Шрифты: создать `.spritefont` и добавить через `FontDescriptionImporter` + `FontDescriptionProcessor`.
- Звук:
  - Эффекты (`.wav`): `WavImporter` + `SoundEffectProcessor`.
  - Музыка (`.wma/.mp3`): `Mp3Importer`/`WmaImporter` + `SongProcessor` (для UWP рекомендуется `.wma` или `.mp3`).

## Пример записи в mgcb
```
# изображение
/Importer:TextureImporter
/Processor:TextureProcessor
/ProcessorParam:ColorKeyEnabled=False
/Build:AssetsRaw/ui/menu_bg.png

# шрифт
/Importer:FontDescriptionImporter
/Processor:FontDescriptionProcessor
/Build:Content/fonts/ui.spritefont
```

## Примечания
- Текущие XNB (`font.xnb`, `Music.xnb`) остаются рабочими до полной миграции.
- Добавление новых ассетов пойдёт через `Content.mgcb`; существующие — параллельно, с постепенным отказом от предсборенных XNB.
