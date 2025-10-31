using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PPH
{
    // Роутер ввода: хранит предыдущие состояния и формирует InputState
    public class InputRouter
    {
        private KeyboardState _prevKeyboard;
        private MouseState _prevMouse;
        private TouchCollection _prevTouches;

        public InputState Capture(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            var touches = TouchPanel.GetState();

            var state = new InputState
            {
                GameTime = gameTime,
                Keyboard = keyboard,
                PrevKeyboard = _prevKeyboard,
                Mouse = mouse,
                PrevMouse = _prevMouse,
                Touches = touches,
                PrevTouches = _prevTouches
            };

            _prevKeyboard = keyboard;
            _prevMouse = mouse;
            _prevTouches = touches;

            return state;
        }
    }
}
