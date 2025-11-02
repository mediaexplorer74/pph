using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PPH
{
    public class Game1 : Game
    {
        public static Microsoft.Xna.Framework.Content.ContentManager ContentManager { get; private set; }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ViewManager _viewManager;
        private GameProcess _process;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ContentManager = Content;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _viewManager = new ViewManager();
            _process = new GameProcess(_viewManager);
            _viewManager.Process = _process;
            _process.GoToMenu();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            _process?.Update(gameTime);
            _viewManager?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (_spriteBatch == null)
                _spriteBatch = new SpriteBatch(GraphicsDevice);

            _viewManager?.Draw(_spriteBatch);
            base.Draw(gameTime);
        }
    }
}
