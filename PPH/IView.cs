using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PPH
{
    // Простой интерфейс представления: Update/Draw
    public interface IView
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
