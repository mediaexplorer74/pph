using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PPH
{
    // Снимок ввода за кадр
    public class InputState
    {
        public GameTime GameTime { get; set; }

        public KeyboardState Keyboard { get; set; }
        public KeyboardState PrevKeyboard { get; set; }

        public MouseState Mouse { get; set; }
        public MouseState PrevMouse { get; set; }

        public TouchCollection Touches { get; set; }
        public TouchCollection PrevTouches { get; set; }
    }
}
