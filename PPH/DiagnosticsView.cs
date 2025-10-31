using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PPH
{
    // Тестовая вьюшка диагностики ввода/рендера
    public class DiagnosticsView : IView, IInputConsumer
    {
        private readonly ViewManager _mgr;
        private SpriteFont _font;

        // Диагностика контента
        private Texture2D _logo;
        private SoundEffect _sfx;
        private Song _song;
        private bool _musicPlaying;

        private Keys? _lastKey;
        private Point? _lastMouseClick;
        private Vector2? _lastTouch;
        private string _info = string.Empty;

        public DiagnosticsView(ViewManager mgr)
        {
            _mgr = mgr;
        }

        public void Update(GameTime gameTime)
        {
            // Здесь можно добавить анимации/таймеры, если нужно
        }

        public void OnInput(InputState input)
        {
            var ks = input.Keyboard;
            var prevKs = input.PrevKeyboard;

            // Новые нажатия клавиш
            var curr = ks.GetPressedKeys();
            var prev = prevKs.GetPressedKeys();
            var newlyPressed = curr.Except(prev).ToArray();
            if (newlyPressed.Length > 0)
            {
                _lastKey = newlyPressed[0];
                if (_lastKey == Keys.Escape)
                {
                    _mgr.Replace(new MenuView(_mgr));
                    return;
                }

                // Горячие клавиши аудио
                if (_lastKey == Keys.P)
                {
                    try { _sfx?.Play(); } catch { }
                }
                if (_lastKey == Keys.M)
                {
                    try
                    {
                        if (_song != null)
                        {
                            if (_musicPlaying)
                            {
                                MediaPlayer.Stop();
                                _musicPlaying = false;
                            }
                            else
                            {
                                MediaPlayer.IsRepeating = true;
                                MediaPlayer.Play(_song);
                                _musicPlaying = true;
                            }
                        }
                    }
                    catch { }
                }
            }

            // Клики мышью
            if (input.PrevMouse.LeftButton == ButtonState.Released && input.Mouse.LeftButton == ButtonState.Pressed)
            {
                _lastMouseClick = new Point(input.Mouse.X, input.Mouse.Y);
            }

            // Касания
            if (input.Touches.Count > input.PrevTouches.Count)
            {
                var t = input.Touches.FirstOrDefault();
                _lastTouch = t.Position;
            }

            // Сводка
            _info = $"Keys: {string.Join(", ", curr.Select(k => k.ToString()))} | Mouse: ({input.Mouse.X},{input.Mouse.Y}) | Touches: {input.Touches.Count}";
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                try { _font = Game1.ContentManager.Load<SpriteFont>("font"); } catch { }
            }

            // Ленивая загрузка ресурсов из Content Pipeline
            if (_logo == null)
            {
                try { _logo = Game1.ContentManager.Load<Texture2D>("iologo"); } catch { }
            }
            if (_sfx == null)
            {
                try { _sfx = Game1.ContentManager.Load<SoundEffect>("player_hit"); } catch { }
            }
            if (_song == null)
            {
                try { _song = Game1.ContentManager.Load<Song>("Music"); } catch { }
            }

            spriteBatch.GraphicsDevice.Clear(Color.DimGray);
            spriteBatch.Begin();

            if (_font != null)
            {
                spriteBatch.DrawString(_font, "Diagnostics (Input & Render)", new Vector2(40, 40), Color.White);
                spriteBatch.DrawString(_font, "[Esc] Back | [P] SFX | [M] Music", new Vector2(40, 70), Color.LightGray);

                spriteBatch.DrawString(_font, "Last Key: " + (_lastKey?.ToString() ?? "-"), new Vector2(40, 110), Color.Yellow);
                spriteBatch.DrawString(_font, "Last Mouse Click: " + (_lastMouseClick?.ToString() ?? "-"), new Vector2(40, 140), Color.Yellow);
                spriteBatch.DrawString(_font, "Last Touch: " + (_lastTouch?.ToString() ?? "-"), new Vector2(40, 170), Color.Yellow);
                spriteBatch.DrawString(_font, _info, new Vector2(40, 210), Color.LightGreen);

                // Отрисовка логотипа (если доступен)
                if (_logo != null)
                {
                    var pos = new Vector2(40, 260);
                    spriteBatch.Draw(_logo, pos, Color.White);
                    spriteBatch.DrawString(_font, "iologo.png loaded via mgcb", pos + new Vector2(0, _logo.Height + 8), Color.LightGray);
                }

                // Статус аудио
                spriteBatch.DrawString(_font, $"SFX loaded: {_sfx != null}", new Vector2(40, 360), Color.LightGray);
                spriteBatch.DrawString(_font, $"Music loaded: {_song != null}, playing: {_musicPlaying}", new Vector2(40, 390), Color.LightGray);
            }

            spriteBatch.End();
        }
    }
}
