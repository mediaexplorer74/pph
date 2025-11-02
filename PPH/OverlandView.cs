using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System;

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
        private Task<Texture2D> _loadSurfTask;
        private Texture2D _surfTiles;
        private readonly string _mapPath;
        private readonly GameWorld _world;

        public OverlandView(ViewManager mgr, string mapRelativePath = "Data/xl.hmm", GameWorld world = null)
        {
            _mgr = mgr;
            _mapPath = mapRelativePath;
            _world = world;
        }

        public void Update(GameTime gameTime)
        {
            // TODO: таймеры, анимации, обновление карты
            if (_world != null && _world.Map != null && (_mapW == null || _mapH == null))
            {
                // Используем уже инициализированный мир для вывода размеров
                _mapW = _world.MapWidth;
                _mapH = _world.MapHeight;
                var fmt = _world.Map.IsEditorMap ? "EMAP" : "GMAP";
                var verHex = $"0x{_world.Map.Version:X}";
                _hdrInfo = $"Format: {fmt} {verHex} | Map: {_world.Map.Name} ({_world.Map.FileVersion}) by {_world.Map.Author}";
            }
            else if (_loadHeaderTask == null)
            {
                // Ленивая загрузка заголовка указанной карты
                _loadHeaderTask = HmmReader.ReadHeaderAsync(_mapPath);
            }
            else if (_loadHeaderTask.IsCompleted && (_mapW == null || _mapH == null))
            {
                HmmHeader hdr = default;

                try
                {
                    hdr = _loadHeaderTask.Result;
                    if (hdr.Width == 0 || hdr.Height == 0)
                    {
                        _mapW = null;
                        _mapH = null;
                    }
                    else
                    {
                        _mapW = hdr.Width;
                        _mapH = hdr.Height;
                    }

                    var fmt = hdr.ParseStatus == "Unknown" ? "Unknown" : (hdr.IsEditorMap ? "EMAP" : "GMAP");
                    var verHex = $"0x{hdr.Version:X}";
                    _hdrInfo = $"Format: {fmt} {verHex} | Map: {hdr.Name} ({hdr.FileVersion}) by {hdr.Author}";
                }
                catch 
                {
                    _mapW = null;
                    _mapH = null;
                    _hdrInfo = $"Format: Unknown 0x0 | Map: - (-) by -";
                }
            }

            // Загрузка SurfTiles переносится в Draw, где доступен GraphicsDevice
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

            // Ленивая загрузка тайлсета поверхности из PNG (без XNB), нужна GraphicsDevice
            if (_surfTiles == null)
            {
                if (_loadSurfTask == null)
                {
                    _loadSurfTask = RawAssetLoader.LoadTextureAsync(spriteBatch.GraphicsDevice, "Data/Resources/hmm/GFX/Common/SurfTiles.png");
                }
                else if (_loadSurfTask.IsCompleted)
                {
                    try { _surfTiles = _loadSurfTask.Result; } catch { }
                }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkGreen);
            spriteBatch.Begin();
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Overland View (placeholder)", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back to Menu", new Vector2(40, 80), Color.LightGray);
                spriteBatch.DrawString(_font, "[B] Enter Battle", new Vector2(40, 110), Color.LightGray);

                // Размер карты: всегда выводим строку, неизвестный размер отображаем явно
                var sizeLine = (_mapW != null && _mapH != null) ? $"Map size: {_mapW} x {_mapH}" : "Map size: Unknown";
                spriteBatch.DrawString(_font, sizeLine, new Vector2(40, 140), Color.Yellow);

                if (!string.IsNullOrEmpty(_hdrInfo))
                    spriteBatch.DrawString(_font, _hdrInfo, new Vector2(40, 165), Color.LightGray);

                // Отрисовка одного тайла из SurfTiles.png только если известен размер (как признак корректного заголовка)
                if (_mapW != null && _mapH != null)
                {
                    int tile = 32; // временный размер тайла
                    int originX = 40;
                    int originY = 200;
                    if (_surfTiles != null)
                    {
                        var src = new Rectangle(0, 0, Math.Min(tile, _surfTiles.Width), Math.Min(tile, _surfTiles.Height));
                        var dst = new Rectangle(originX, originY, tile, tile);
                        spriteBatch.Draw(_surfTiles, dst, src, Color.White);
                    }
                }
            }
            spriteBatch.End();
        }
    }
}
