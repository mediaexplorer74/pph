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

        public static async Task DeployAsync()
        {
            var local = ApplicationData.Current.LocalFolder;

            // Создаём базовые каталоги рантайма
            await EnsureFolderAsync(local, "Data");
            await EnsureFolderAsync(local, "Maps");
            await EnsureFolderAsync(local, "Save");

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

