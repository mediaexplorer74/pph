using System.Threading.Tasks;

namespace PPH
{
    // Упрощённая информация о карте, полученная из заголовка HMM
    public class MapInfo
    {
        public string Path { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileVersion { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public uint Version { get; set; }
        public bool IsEditorMap { get; set; }
        public uint RawFourCC { get; set; }
    }

    public static class MapInfoReader
    {
        // Чтение MapInfo на основе уже реализованного HmmReader
        public static async Task<MapInfo> ReadAsync(string relativePath)
        {
            var hdr = await HmmReader.ReadHeaderAsync(relativePath);
            return new MapInfo
            {
                Path = relativePath,
                Name = hdr.Name,
                Description = hdr.Description,
                FileVersion = hdr.FileVersion,
                Author = hdr.Author,
                Width = hdr.Width,
                Height = hdr.Height,
                Version = hdr.Version,
                IsEditorMap = hdr.IsEditorMap,
                RawFourCC = hdr.RawFourCC
            };
        }
    }
}

