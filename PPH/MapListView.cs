using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace PPH
{
    public class MapListView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;
        private List<MapInfo> _maps;
        private Task<List<MapInfo>> _loadTask;
        private int _selectedIndex;

        public MapListView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime)
        {
            if (_maps == null)
            {
                if (_loadTask == null)
                {
                    _loadTask = LoadMapsAsync();
                }
                else if (_loadTask.IsCompleted)
                {
                    try { _maps = _loadTask.Result; } catch { _maps = new List<MapInfo>(); }
                    if (_maps.Count == 0) _selectedIndex = 0; else _selectedIndex = 0;
                }
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

            if (_maps == null || _maps.Count == 0) return;

            if (prev.IsKeyUp(Keys.Up) && ks.IsKeyDown(Keys.Up))
            {
                if (_selectedIndex > 0) _selectedIndex--;
            }
            if (prev.IsKeyUp(Keys.Down) && ks.IsKeyDown(Keys.Down))
            {
                if (_selectedIndex < _maps.Count - 1) _selectedIndex++;
            }
            if (prev.IsKeyUp(Keys.Enter) && ks.IsKeyDown(Keys.Enter))
            {
                var mi = _maps[_selectedIndex];
                if (_mgr.Process != null) _mgr.Process.StartNewGame(mi.Path, true);
                else _mgr.Replace(new OverlandView(_mgr, mi.Path));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                try { _font = Game1.ContentManager.Load<SpriteFont>("fonts/ui"); } catch { }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DarkSlateGray);
            spriteBatch.Begin();
            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Select Map", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back", new Vector2(40, 80), Color.LightGray);

                int y = 120;
                if (_maps == null)
                {
                    spriteBatch.DrawString(_font, "Loading maps...", new Vector2(40, y), Color.LightGray);
                }
                else if (_maps.Count == 0)
                {
                    spriteBatch.DrawString(_font, "No maps found", new Vector2(40, y), Color.Orange);
                }
                else
                {
                    for (int i = 0; i < _maps.Count; i++)
                    {
                        var mi = _maps[i];
                        var color = (i == _selectedIndex) ? Color.Yellow : Color.LightGray;
                        string line1 = $"{mi.Name} {mi.Width}x{mi.Height} {mi.Author}";
                        string line2 = $"Diff:{mi.Difficulty} Mode:{mi.GameMode} Day:{mi.CurrentDay}";
                        string desc = string.IsNullOrWhiteSpace(mi.Description) ? string.Empty : mi.Description;
                        if (desc.Length > 60) desc = desc.Substring(0, 60) + "…";

                        spriteBatch.DrawString(_font, line1, new Vector2(60, y), color);
                        y += 22;
                        spriteBatch.DrawString(_font, line2, new Vector2(64, y), Color.LightGray);
                        y += 22;
                        if (desc.Length > 0)
                        {
                            spriteBatch.DrawString(_font, desc, new Vector2(64, y), Color.DimGray);
                            y += 22;
                        }
                        y += 6;
                    }
                }
            }
            spriteBatch.End();
        }

        private async Task<List<MapInfo>> LoadMapsAsync()
        {
            var list = new List<MapInfo>();
            try
            {
                var local = ApplicationData.Current.LocalFolder;
                var mapsFolder = await local.CreateFolderAsync("Maps", CreationCollisionOption.OpenIfExists);
                var files = await mapsFolder.GetFilesAsync();
                foreach (var f in files)
                {
                    if (f.FileType.ToLowerInvariant() == ".phm")
                    {
                        string rel = "Maps/" + f.Name;
                        try
                        {
                            var mi = await MapInfoReader.ReadAsync(rel);
                            list.Add(mi);
                        }
                        catch
                        {
                            // Игнорируем файлы с ошибками чтения
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки перечисления
            }
            return list;
        }
    }
}
