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
            _mgr.Replace(new OverlandView(_mgr));
        }

        public void EnterBattle()
        {
            State = GameState.Battle;
            _mgr.Replace(new BattleView(_mgr));
        }

        public void ExitBattleToOverland()
        {
            State = GameState.Overland;
            _mgr.Replace(new OverlandView(_mgr));
        }

        public void OpenDiagnostics()
        {
            State = GameState.Diagnostics;
            _mgr.Replace(new DiagnosticsView(_mgr));
        }
    }
}

