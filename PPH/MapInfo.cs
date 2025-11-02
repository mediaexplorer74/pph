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

        // Дополнительные поля GMAP
        public uint CurrentDay { get; set; }
        public ushort GameMode { get; set; }
        public sbyte Difficulty { get; set; }
        public ushort PlayerCount { get; set; }
        public ushort CurrentPlayerId { get; set; }
        public System.Collections.Generic.List<PlayerInfo> Players { get; set; } = new System.Collections.Generic.List<PlayerInfo>();
    }

    // Сводная информация об игроке из заголовка GMAP
    public class PlayerInfo
    {
        public byte Id { get; set; }
        public byte Nation { get; set; }
        public byte TypeMask { get; set; }
        public byte Type { get; set; }
        public int[] Resources { get; set; } = new int[7];
        public ushort HeroId { get; set; }
        public ushort CastleIdx { get; set; }
        public byte Keys { get; set; }
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
                RawFourCC = hdr.RawFourCC,
                CurrentDay = hdr.CurrentDay,
                GameMode = hdr.GameMode,
                Difficulty = hdr.Difficulty,
                PlayerCount = hdr.PlayerCount,
                CurrentPlayerId = hdr.CurrentPlayerId,
                Players = hdr.Players
            };
        }
    }
}
