using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace PPH
{
    public class OverlandView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;
        private Texture2D _pixel;
        private Task<HmmHeader> _loadHeaderTask;
        private ushort? _mapW;
        private ushort? _mapH;
        private string _hdrInfo;

        public OverlandView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime)
        {
            // TODO: таймеры, анимации, обновление карты
            if (_loadHeaderTask == null)
            {
                // Ленивая загрузка заголовка xl.hmm (размеры карты)
                _loadHeaderTask = HmmReader.ReadHeaderAsync("Data/xl.hmm");
            }
            else if (_loadHeaderTask.IsCompleted && (_mapW == null || _mapH == null))
            {
                try
                {
                    var hdr = _loadHeaderTask.Result;
                    _mapW = hdr.Width;
                    _mapH = hdr.Height;
                    _hdrInfo = $"Map: {hdr.Name} ({hdr.FileVersion}) by {hdr.Author}";
                }
                catch { }
            }
        }

        public void OnInput(InputState input)
        {
            var ks = input.Keyboard;
            var prev = input.PrevKeyboard;
            if (prev.IsKeyUp(Keys.Escape) && ks.IsKeyDown(Keys.Escape))
            {
                if (_mgr.Process != null) _mgr.Process.GoToMenu();
                else _mgr.Replace(new MenuView(_mgr));
            }
            if (prev.IsKeyUp(Keys.B) && ks.IsKeyDown(Keys.B))
            {
                if (_mgr.Process != null) _mgr.Process.EnterBattle();
                else _mgr.Replace(new BattleView(_mgr));
            }
            // TODO: обработка мыши/тача для скролла/выбора
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                try { _font = Game1.ContentManager.Load<SpriteFont>("fonts/ui"); } catch { }
            }
            if (_pixel == null)
            {
                try
                {
                    _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                    _pixel.SetData(new[] { Color.White });
                }
                catch { }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkGreen);
            spriteBatch.Begin();
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Overland View (placeholder)", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back to Menu", new Vector2(40, 80), Color.LightGray);
                spriteBatch.DrawString(_font, "[B] Enter Battle", new Vector2(40, 110), Color.LightGray);

                if (_mapW != null && _mapH != null)
                {
                    spriteBatch.DrawString(_font, $"Map size: {_mapW} x {_mapH}", new Vector2(40, 140), Color.Yellow);
                    if (!string.IsNullOrEmpty(_hdrInfo))
                        spriteBatch.DrawString(_font, _hdrInfo, new Vector2(40, 165), Color.LightGray);

                    // Простейшая сетка по метрикам карты
                    int cell = 10; // базовый размер клетки для визуализации
                    int originX = 40;
                    int originY = 200;
                    int wpx = cell * _mapW.Value;
                    int hpx = cell * _mapH.Value;

                    if (_pixel != null)
                    {
                        // Вертикальные линии
                        for (int x = 0; x <= _mapW.Value; x++)
                        {
                            int xp = originX + x * cell;
                            spriteBatch.Draw(_pixel, new Rectangle(xp, originY, 1, hpx), Color.White * 0.35f);
                        }
                        // Горизонтальные линии
                        for (int y = 0; y <= _mapH.Value; y++)
                        {
                            int yp = originY + y * cell;
                            spriteBatch.Draw(_pixel, new Rectangle(originX, yp, wpx, 1), Color.White * 0.35f);
                        }
                        // Рамка
                        spriteBatch.Draw(_pixel, new Rectangle(originX, originY, wpx, 1), Color.White);
                        spriteBatch.Draw(_pixel, new Rectangle(originX, originY + hpx, wpx, 1), Color.White);
                        spriteBatch.Draw(_pixel, new Rectangle(originX, originY, 1, hpx), Color.White);
                        spriteBatch.Draw(_pixel, new Rectangle(originX + wpx, originY, 1, hpx), Color.White);
                    }
                }
            }
            spriteBatch.End();
        }
    }
}

