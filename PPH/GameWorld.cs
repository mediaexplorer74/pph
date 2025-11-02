using Microsoft.Xna.Framework;
using System;
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

        // Время и ходы
        public Clock Clock { get; } = new Clock();
        public TurnManager TurnManager { get; } = new TurnManager();
        public uint CurrentDay => Map?.CurrentDay ?? Clock.CurrentDay;
        public sbyte Difficulty => Map?.Difficulty ?? 0;
        public ushort GameMode => Map?.GameMode ?? 0;
        public System.Collections.Generic.List<PlayerInfo> Players => Map?.Players;

        // Камера мира (упрощённо)
        public int CameraX { get; set; } = 40;
        public int CameraY { get; set; } = 200;

        // Результат последнего боя (будет заполнен BattleView)
        public BattleResult LastBattleResult { get; set; }

        public async Task InitializeAsync(string relativeMapPath)
        {
            Map = await MapInfoReader.ReadAsync(relativeMapPath);
            // Инициализируем таймер и ходы из данных карты
            Clock.Reset(Map.CurrentDay);
            TurnManager.InitializeFromMap(Map);
        }

        // Условный апдейт мира: тик таймера и синхронизация дня
        public void Update(GameTime gameTime)
        {
            if (Map == null) return;
            Clock.Update(gameTime);
            Map.CurrentDay = Clock.CurrentDay;
        }

        public void ApplyBattleResult(BattleResult result)
        {
            if (result == null) return;
            LastBattleResult = result;
            // TODO: применить потери/ресурсы/баффы по миру
        }
    }
}
