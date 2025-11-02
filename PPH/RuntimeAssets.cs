using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace PPH
{
    // Развёртывание (распаковка) игровых данных в LocalFolder при запуске
    public static class RuntimeAssets
    {
        static string Normalize(string path) => path.Replace("\\", "/");
        const string CacheVersion = "1"; // версия схемы кэша/распаковки

        public static async Task DeployAsync()
        {
            var local = ApplicationData.Current.LocalFolder;

            // Создаём базовые каталоги рантайма
            await EnsureFolderAsync(local, "Data");
            await EnsureFolderAsync(local, "Maps");
            await EnsureFolderAsync(local, "Save");
            await EnsureFolderAsync(local, "Cache");
            await EnsureFolderAsync(local, "Cache");

            // Пакеты из Data: перезаписываем на каждую установку/обновление
            await CopyFromPackageToLocalAsync("Data/fonts.dat", "Data/fonts.dat", overwrite: true);
            await CopyFromPackageToLocalAsync("Data/game.gfx", "Data/game.gfx", overwrite: true);
            await CopyFromPackageToLocalAsync("Data/game.pix", "Data/game.pix", overwrite: true);
            await CopyFromPackageToLocalAsync("Data/game.sfx", "Data/game.sfx", overwrite: true);
            await CopyFromPackageToLocalAsync("Data/objects.dat", "Data/objects.dat", overwrite: true);

            // Карты: копируем, если отсутствуют (пользователь может заменять/добавлять свои)
            await CopyFromPackageToLocalAsync("Maps/Tutorial_English.phm", "Maps/Tutorial_English.phm", overwrite: false);
            await CopyFromPackageToLocalAsync("Maps/Two_by_the_river_English.phm", "Maps/Two_by_the_river_English.phm", overwrite: false);

            // Конфиг: создаём по умолчанию, если не существует
            await CopyFromPackageToLocalAsync("PalmHeroes.cfg", "PalmHeroes.cfg", overwrite: false);
        }

        // Прототип распаковки: проверка наличия паков и запись маркера версии
        public static async Task UnpackAsync()
        {
            var local = ApplicationData.Current.LocalFolder;
            var cache = await local.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);

            // Если версия уже совпадает — пропускаем
            try
            {
                var verItem = await cache.TryGetItemAsync(".version") as StorageFile;
                if (verItem != null)
                {
                    var text = await FileIO.ReadTextAsync(verItem);
                    if (text.Trim() == CacheVersion) return;
                }
            }
            catch { /* игнорируем ошибки чтения */ }

            // Валидация магик-байт в паках, если доступны в LocalFolder
            try { await ValidateMagicIfExists("Data/game.gfx"); } catch { }
            try { await ValidateMagicIfExists("Data/game.pix"); } catch { }
            try { await ValidateMagicIfExists("Data/game.sfx"); } catch { }
            try { await ValidateMagicIfExists("Data/fonts.dat"); } catch { }
            try { await ValidateMagicIfExists("Data/objects.dat"); } catch { }

            // Пока прототип: только помечаем версию кэша без фактической распаковки
            try
            {
                var versionFile = await cache.CreateFileAsync(".version", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(versionFile, CacheVersion);
            }
            catch { /* игнорируем запись маркера при ошибке */ }
        }

        static async Task ValidateMagicIfExists(string localRelativePath)
        {
            var file = await TryGetLocalFileAsync(localRelativePath);
            if (file == null) return;
            await PackIndexReader.ValidateMagicAsync(file);
        }

        static async Task EnsureFolderAsync(StorageFolder root, string subfolder)
        {
            await root.CreateFolderAsync(subfolder, CreationCollisionOption.OpenIfExists);
        }

        static async Task CopyFromPackageToLocalAsync(string packageRelativePath, string localRelativePath, bool overwrite)
        {
            try
            {
                var pkgUri = new Uri("ms-appx:///" + Normalize(packageRelativePath));
                StorageFile src = await StorageFile.GetFileFromApplicationUriAsync(pkgUri);

                StorageFolder localRoot = ApplicationData.Current.LocalFolder;
                string folderPart = Path.GetDirectoryName(localRelativePath) ?? string.Empty;
                StorageFolder destFolder = localRoot;
                if (!string.IsNullOrEmpty(folderPart))
                {
                    // Создаём при необходимости вложенные папки
                    destFolder = await localRoot.CreateFolderAsync(folderPart, CreationCollisionOption.OpenIfExists);
                }

                string fileName = Path.GetFileName(localRelativePath);

                if (!overwrite)
                {
                    var existing = await destFolder.TryGetItemAsync(fileName);
                    if (existing is StorageFile)
                    {
                        // Уже существует — не перезаписываем
                        return;
                    }
                }

                await src.CopyAsync(destFolder, fileName, overwrite ? NameCollisionOption.ReplaceExisting : NameCollisionOption.FailIfExists);
            }
            catch
            {
                // Тихо игнорируем отсутствие в пакете (напр., если карты не включены)
            }
        }

        // Утилита: попытка получить файл из LocalFolder по относительному пути
        public static async Task<StorageFile> TryGetLocalFileAsync(string relativePath)
        {
            try
            {
                StorageFolder local = ApplicationData.Current.LocalFolder;
                var item = await local.TryGetItemAsync(relativePath.Replace("/", "\\"));
                return item as StorageFile;
            }
            catch
            {
                return null;
            }
        }
    }
}
