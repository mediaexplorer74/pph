using System;

namespace PPH
{
    // Простой менеджер ходов: хранит текущего игрока и переключает по кругу
    public class TurnManager
    {
        public ushort PlayerCount { get; private set; }
        public ushort CurrentPlayerId { get; private set; }

        public void InitializeFromMap(MapInfo map)
        {
            PlayerCount = map?.PlayerCount ?? 0;
            CurrentPlayerId = map?.CurrentPlayerId ?? 0;
        }

        public void EndTurn()
        {
            if (PlayerCount == 0)
            {
                CurrentPlayerId = 0;
                return;
            }
            CurrentPlayerId = (ushort)((CurrentPlayerId + 1) % PlayerCount);
        }
    }
}

