using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PPH
{
    public class MenuView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;
        private readonly string[] _items = { "Start Overland", "Diagnostics", "Exit" };
        private int _sel = 0;

        public MenuView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime) { /* таймеры/анимации по мере необходимости */ }

        public void OnInput(InputState input)
        {
            var ks = input.Keyboard;
            var prev = input.PrevKeyboard;
            if (prev.IsKeyUp(Keys.Down) && ks.IsKeyDown(Keys.Down)) _sel = System.Math.Min(_sel + 1, _items.Length - 1);
            if (prev.IsKeyUp(Keys.Up) && ks.IsKeyDown(Keys.Up)) _sel = System.Math.Max(_sel - 1, 0);
            if (prev.IsKeyUp(Keys.Enter) && ks.IsKeyDown(Keys.Enter))
            {
                switch (_sel)
                {
                    case 0:
                        _mgr.Replace(new OverlandView(_mgr));
                        break;
                    case 1:
                        _mgr.Replace(new DiagnosticsView(_mgr));
                        break;
                    case 2:
                        // TODO: корректное завершение приложения в UWP (пока игнорируем)
                        break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Lazy-load шрифта из Content
            if (_font == null)
            {
                try { _font = Game1.ContentManager.Load<SpriteFont>("font"); } catch { }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkSlateGray);
            spriteBatch.Begin();

            var title = "Pocket Palm Heroes (Menu)";
            var pos = new Vector2(40, 40);

            if (_font != null)
            {
                spriteBatch.DrawString(_font, title, pos, Color.White);
                for (int i = 0; i < _items.Length; i++)
                {
                    var color = i == _sel ? Color.Yellow : Color.White;
                    var prefix = i == _sel ? "> " : "  ";
                    spriteBatch.DrawString(_font, prefix + _items[i], pos + new Vector2(0, 50 + i * 30), color);
                }
            }

            spriteBatch.End();
        }
    }
}
