using System.Collections.Generic;

namespace PPH
{
    // Скелет результата боя: победитель, потери и произвольные заметки
    public class BattleResult
    {
        public bool AttackerVictory { get; set; }
        public byte WinnerPlayerId { get; set; }
        // Упрощённо: потери в виде массива по юнитам (будет уточняться)
        public Dictionary<byte, int[]> CasualtiesByPlayer { get; set; } = new Dictionary<byte, int[]>();
        public int GoldDelta { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}

