using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PPH
{
    public class BattleView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;

        public BattleView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime) { }

        public void OnInput(InputState input)
        {
            var ks = input.Keyboard;
            var prev = input.PrevKeyboard;
            if (prev.IsKeyUp(Keys.Escape) && ks.IsKeyDown(Keys.Escape))
            {
                if (_mgr.Process != null) _mgr.Process.ExitBattleToOverland();
                else _mgr.Replace(new OverlandView(_mgr));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                try 
                { 
                    _font = Game1.ContentManager.Load<SpriteFont>("fonts/ui"); 
                } 
                catch 
                { 
                }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkSlateBlue);
            spriteBatch.Begin();
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Battle View (placeholder)", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back to Overland", new Vector2(40, 80), Color.LightGray);
            }
            spriteBatch.End();
        }
    }
}
