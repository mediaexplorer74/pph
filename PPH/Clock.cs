using System;
using Microsoft.Xna.Framework;

namespace PPH
{
    // Простой таймер игрового времени: увеличивает день каждые N секунд
    public class Clock
    {
        private double _accumSeconds;
        public uint CurrentDay { get; private set; }
        public double SecondsPerDay { get; set; } = 10.0; // заглушка: 1 игровой день = 10 секунд

        public event Action<uint> DayChanged;

        public void Reset(uint day)
        {
            CurrentDay = day;
            _accumSeconds = 0.0;
        }

        public void Update(GameTime gameTime)
        {
            _accumSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if (_accumSeconds >= SecondsPerDay)
            {
                _accumSeconds = 0.0;
                CurrentDay++;
                DayChanged?.Invoke(CurrentDay);
            }
        }
    }
}

