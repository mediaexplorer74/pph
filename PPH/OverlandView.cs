using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PPH
{
    public class OverlandView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;

        public OverlandView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime)
        {
            // TODO: таймеры, анимации, обновление карты
        }

        public void OnInput(InputState input)
        {
            var ks = input.Keyboard;
            var prev = input.PrevKeyboard;
            if (prev.IsKeyUp(Keys.Escape) && ks.IsKeyDown(Keys.Escape))
            {
                _mgr.Replace(new MenuView(_mgr));
            }
            if (prev.IsKeyUp(Keys.B) && ks.IsKeyDown(Keys.B))
            {
                _mgr.Replace(new BattleView(_mgr));
            }
            // TODO: обработка мыши/тача для скролла/выбора
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                try { _font = Game1.ContentManager.Load<SpriteFont>("font"); } catch { }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkGreen);
            spriteBatch.Begin();
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Overland View (placeholder)", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back to Menu", new Vector2(40, 80), Color.LightGray);
                spriteBatch.DrawString(_font, "[B] Enter Battle", new Vector2(40, 110), Color.LightGray);
            }
            spriteBatch.End();
        }
    }
}

