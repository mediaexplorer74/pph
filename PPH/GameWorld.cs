using System.Threading.Tasks;

namespace PPH
{
    // Минимальный скелет игрового мира: хранит метаданные карты
    public class GameWorld
    {
        public MapInfo Map { get; private set; }

        public string MapName => Map?.Name ?? string.Empty;
        public ushort MapWidth => Map?.Width ?? 0;
        public ushort MapHeight => Map?.Height ?? 0;

        public async Task InitializeAsync(string relativeMapPath)
        {
            Map = await MapInfoReader.ReadAsync(relativeMapPath);
        }
    }
}

