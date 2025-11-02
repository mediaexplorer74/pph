using Microsoft.Xna.Framework;

namespace PPH
{
    public enum GameState
    {
        Menu,
        Overland,
        Battle,
        Diagnostics
    }

    // Центральный контроллер состояний игры: единая точка переходов между вьюшками
    public class GameProcess
    {
        private readonly ViewManager _mgr;
        private bool _goToMainMenu;
        private bool _started;
        private bool _returningFromBattle;
        public GameWorld World { get; private set; }

        public GameState State { get; private set; }

        public GameProcess(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void GoToMenu()
        {
            State = GameState.Menu;
            _mgr.Replace(new MenuView(_mgr));
        }

        public void StartOverland()
        {
            State = GameState.Overland;
            // Если мир уже создан, передадим его в OverlandView, иначе вью загрузит заголовок карты лениво
            _mgr.Replace(new OverlandView(_mgr, null, World));
            _started = true;
        }

        public void EnterBattle()
        {
            State = GameState.Battle;
            _returningFromBattle = false;
            _mgr.Replace(new BattleView(_mgr));
        }

        public void ExitBattleToOverland()
        {
            State = GameState.Overland;
            _returningFromBattle = true; // помечаем, что вернулись из боя
            _mgr.Replace(new OverlandView(_mgr, null, World));
        }

        public void ExitBattleToOverland(BattleResult result)
        {
            State = GameState.Overland;
            _returningFromBattle = true;
            // Применяем итоги боя к миру до переключения вью
            World?.ApplyBattleResult(result);
            _mgr.Replace(new OverlandView(_mgr, null, World));
        }

        public void OpenDiagnostics()
        {
            State = GameState.Diagnostics;
            _mgr.Replace(new DiagnosticsView(_mgr));
        }

        // --- Перенос каркаса из iGame ---
        public void MainMenu()
        {
            _goToMainMenu = true;
        }

        public bool StartNewGame(string mapFilePath, bool newGame)
        {
            try
            {
                // Создаём скелет мира иинициализируем его на основе заголовка карты
                var world = new GameWorld();
                world.InitializeAsync(mapFilePath).GetAwaiter().GetResult();
                World = world;

                State = GameState.Overland;
                _started = true;
                _mgr.Replace(new OverlandView(_mgr, mapFilePath, World));
                return true;
            }
            catch
            {
                // Если что-то пошло не так при загрузке карты — вернёмся в меню
                World = null;
                _started = false;
                GoToMenu();
                return false;
            }
        }

        public void ExitGame(bool changeView)
        {
            _started = false;
            if (changeView)
            {
                GoToMenu();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_goToMainMenu)
            {
                _goToMainMenu = false;
                ExitGame(true);
                return;
            }

            if (!_started) return;

            if (State == GameState.Overland)
            {
                // Обновление игрового мира (таймеры/ход игрока)
                World?.Update(gameTime);

                if (_returningFromBattle)
                {
                    // Здесь может быть восстановление контекста мира (позиции, эффекты, итоги боя)
                    // Пока оставляем маркер, затем добавим конкретные поля
                    _returningFromBattle = false;
                }
            }
        }
    }
}
