using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PPH
{
    // Менеджер вьюшек: стек активных представлений, Update/Draw делегируются верхушке стека
    public class ViewManager
    {
        private readonly Stack<IView> _stack = new Stack<IView>();
        private readonly InputRouter _inputRouter = new InputRouter();

        // Ссылка на центральный процесс игры для удобного доступа из вьюшек
        public GameProcess Process { get; set; }

        public IView ActiveView => _stack.Count > 0 ? _stack.Peek() : null;

        public void Push(IView view)
        {
            if (view != null) _stack.Push(view);
        }

        public void Replace(IView view)
        {
            if (_stack.Count > 0) _stack.Pop();
            if (view != null) _stack.Push(view);
        }

        public void Pop()
        {
            if (_stack.Count > 0) _stack.Pop();
        }

        public void Update(GameTime gameTime)
        {
            var active = ActiveView;
            if (active == null) return;

            // Передаём нормализованный ввод, если вьюшка его поддерживает
            if (active is IInputConsumer consumer)
            {
                var input = _inputRouter.Capture(gameTime);
                consumer.OnInput(input);
            }

            active.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ActiveView?.Draw(spriteBatch);
        }
    }
}

