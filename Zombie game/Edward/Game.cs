using Edward;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZombieGame.Entities;
using ZombieGame.Enums;
using ZombieGame.Managers;
using ZombieGame.Utils;

namespace ZombieGame
{
    public class Game
    {
        private readonly Size _screenSize;
        private readonly Map _map;
        private readonly Player _player;
        private Camera _camera;
        private WaveManager _waveManager;

        private readonly List<Entity> _pickups;
        private readonly List<Bullet> _bullets;
        private readonly List<Projectile> _enemyProjectiles;
        private readonly List<Zombie> _zombies;

        private readonly MusicPlayer _musicPlayer;

        private GameState _state;
        private bool _deathHandled = false;
        private bool _shopTriggered = false;
        private Point _lastMousePos;

        private Rectangle _tryAgainBtn, _backToMenuBtn;
        private Rectangle _shopBuyBtn, _shopContinueBtn;

        // Für Buy-Button-Puls
        private bool _justBought = false;
        private int _buyPulseTimer = 0;

        // Neuer Timer für Damage-Flash
        private const int DamageFlashDuration = 15;
        private int _damageFlashTimer = 0;

        public Game(Size screenSize)
        {
            _screenSize = screenSize;
            _map = new Map(1024, 1024, "map.png");
            _player = new Player(new PointF(_map.Width / 2f, _map.Height / 2f));
            _zombies = new List<Zombie>();
            _enemyProjectiles = new List<Projectile>();
            _waveManager = new WaveManager(_zombies, _map, _player, _enemyProjectiles);

            _camera = new Camera(screenSize, _player) { Zoom = 1.9f };
            _pickups = new List<Entity>();
            _bullets = new List<Bullet>();
            SpawnPickups();

            _musicPlayer = new MusicPlayer();
            string filePath = Path.Combine(Application.StartupPath, "Assets", "Music", "background.mp3");
            Debug.WriteLine($"Loading MP3: {filePath}");
            Debug.WriteLine("Exists? " + File.Exists(filePath));
            _musicPlayer.Play(filePath, loop: true);

            _state = GameState.Playing;
        }

        private void SpawnPickups()
        {
            _pickups.Add(new WeaponPickup(new PointF(350, 300), "Pistol"));
            _pickups.Add(new WeaponPickup(new PointF(_map.Width - 350, _map.Height - 400), "Rifle"));
            _pickups.Add(new HealthPickup(new PointF(_map.Width / 2f + 100, _map.Height / 2f)));
        }

        public void Update()
        {
            // 1) Immer die Spiel-Logik updaten (auch im Shop)
            bool mouseLeftDown = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (Form.ActiveForm != null)
                _lastMousePos = Form.ActiveForm.PointToClient(Cursor.Position);

            _waveManager.Update();
            _player.Update();

            // Kamera & Karte begrenzen
            var pos = _player.Position;
            pos.X = Math.Max(0f, Math.Min(pos.X, _map.Width - _player.Size.Width));
            pos.Y = Math.Max(0f, Math.Min(pos.Y, _map.Height - _player.Size.Height));
            _player.Position = pos;
            _camera.Update();

            // 2) Shop-Trigger nur einmal pro Runde (wenn Wave fertig geplant ist)
            if (_state == GameState.Playing && !_shopTriggered && _waveManager.NextWaveScheduled)
            {
                _state = GameState.Shop;
                _shopTriggered = true;
            }

            // 3) Wenn wir gerade im Shop sind, keine weiteren Zustandswechsel
            if (_state == GameState.Shop)
                return;

            // 4) Restliche Spiel-Logik (Kollisionen, Zombies, Projektile, Pickups, Bullets)
            var playerRect = new RectangleF(_player.Position, _player.Size);

            // Zombies
            foreach (var z in _zombies)
            {
                z.Update();
                var zRect = new RectangleF(z.Position, z.Size);
                if (zRect.IntersectsWith(playerRect) && z.CanAttack())
                {
                    _player.Damage(10);
                    _damageFlashTimer = DamageFlashDuration;
                    z.ResetAttackCooldown();
                    if (_player.Health <= 0 && !_deathHandled)
                    {
                        _deathHandled = true;
                        _state = GameState.GameOver;
                        return;
                    }
                }
            }

            // Feindliche Projektile
            for (int i = _enemyProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _enemyProjectiles[i];
                proj.Update(1f / 60f);
                var projRect = new RectangleF(proj.Position.X - 4, proj.Position.Y - 4, 14, 14);
                if (projRect.IntersectsWith(playerRect))
                {
                    _player.Damage(10);
                    _damageFlashTimer = DamageFlashDuration;
                    _enemyProjectiles.RemoveAt(i);
                    if (_player.Health <= 0 && !_deathHandled)
                    {
                        _deathHandled = true;
                        _state = GameState.GameOver;
                        return;
                    }
                    continue;
                }
                if (!proj.IsAlive)
                    _enemyProjectiles.RemoveAt(i);
            }

            // Pickups
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var pu = _pickups[i];
                if (new RectangleF(pu.Position, pu.Size).IntersectsWith(playerRect))
                {
                    if (pu is HealthPickup hp) _player.Heal(hp.GetHealAmount());
                    else if (pu is WeaponPickup wp) _player.AddWeapon(wp.WeaponName);
                    _pickups.RemoveAt(i);
                }
            }

            // Bullets & Kills
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var b = _bullets[i];
                b.Update();
                if (b.IsOffMap(_map.Width, _map.Height))
                {
                    _bullets.RemoveAt(i);
                    continue;
                }
                for (int j = _zombies.Count - 1; j >= 0; j--)
                {
                    var z = _zombies[j];
                    if (new RectangleF(b.Position, b.Size).IntersectsWith(new RectangleF(z.Position, z.Size)))
                    {
                        z.Damage(b.GetDamage());
                        _bullets.RemoveAt(i);
                        if (z.Health <= 0)
                        {
                            // Geld pro Kill
                            const int moneyPerKill = 10;
                            _player.AddMoney(moneyPerKill);

                            _zombies.RemoveAt(j);
                        }
                        break;
                    }
                }
            }

            // Schießen
            if (mouseLeftDown && _player.GetCurrentWeaponIndex() == 1 && _player.CanFire())
            {
                FireAt(_lastMousePos);
                _player.ResetFireCooldown();
            }
        }

        public void Draw(Graphics g)
        {
            // 1) Grund-Spiel zeichnen
            g.ResetTransform();
            g.Clear(Color.DimGray);
            g.ScaleTransform(_camera.Zoom, _camera.Zoom);
            g.TranslateTransform(-_camera.Position.X, -_camera.Position.Y);
            _map.Draw(g);
            foreach (var pu in _pickups) pu.Draw(g);
            _player.Draw(g);
            foreach (var proj in _enemyProjectiles) proj.Draw(g);
            foreach (var z in _zombies) z.Draw(g);
            foreach (var b in _bullets) b.Draw(g);

            // 2) UI immer darüber
            g.ResetTransform();
            UI.DrawGame(g, _player, _waveManager, _screenSize);

            // 2.1) Schaden-Glow
            if (_damageFlashTimer > 0)
            {
                int alpha = (int)(150f * (_damageFlashTimer / (float)DamageFlashDuration));
                using (var flashBrush = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0)))
                {
                    g.FillRectangle(flashBrush, 0, 0, _screenSize.Width, _screenSize.Height);
                }
                _damageFlashTimer--;
            }

            // 3) Spezial-Zustände
            if (_state == GameState.GameOver)
            {
                DrawGameOverScreen(g);
                return;
            }
            if (_state == GameState.Paused)
            {
                UI.DrawPause(g, _screenSize);
                return;
            }
            if (_state == GameState.Inventory)
            {
                UI.DrawInventory(g, _player, _screenSize);
                return;
            }

            // 4) Shop-Overlay
            if (_state == GameState.Shop)
            {
                DrawShopOverlay(g);
            }
        }

        private void DrawShopOverlay(Graphics g)
        {
            using (var b = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(b, 0, 0, _screenSize.Width, _screenSize.Height);

            int w = 400, h = 250;
            int x = (_screenSize.Width - w) / 2, y = (_screenSize.Height - h) / 2;
            g.FillRectangle(Brushes.Gray, x, y, w, h);
            g.DrawRectangle(Pens.White, x, y, w, h);

            using (var titleF = new Font("Segoe Print", 24, FontStyle.Bold))
            {
                string title = "Shop – Health kaufen";
                var tsz = g.MeasureString(title, titleF);
                g.DrawString(title, titleF, Brushes.White, x + (w - tsz.Width) / 2, y + 10);
            }

            _shopBuyBtn = new Rectangle(x + 20, y + 80, 180, 50);
            _shopContinueBtn = new Rectangle(x + w - 200 - 20, y + 80, 180, 50);

            float scale = 1f;
            if (_justBought && _buyPulseTimer > 0)
            {
                scale = 1f + 0.1f * (_buyPulseTimer / 15f);
                _buyPulseTimer--;
                if (_buyPulseTimer == 0) _justBought = false;
            }

            var c = new PointF(_shopBuyBtn.X + _shopBuyBtn.Width / 2f, _shopBuyBtn.Y + _shopBuyBtn.Height / 2f);
            g.TranslateTransform(c.X, c.Y);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-c.X, -c.Y);
            DrawSimpleButton(g, _shopBuyBtn, "20 HP für $15");
            g.ResetTransform();

            DrawSimpleButton(g, _shopContinueBtn, "Weiter");
        }

        private void DrawGameOverScreen(Graphics g)
        {
            const string msg = "YOU ARE DEAD";
            using (var font = new Font("Arial", 48, FontStyle.Bold))
            {
                var sz = g.MeasureString(msg, font);
                g.DrawString(msg, font, Brushes.Red,
                    (_screenSize.Width - sz.Width) / 2,
                    _screenSize.Height / 3);
            }
            int bw = 200, bh = 50, gap = 20;
            int cx = (_screenSize.Width - bw) / 2, y = _screenSize.Height / 2;
            _tryAgainBtn = new Rectangle(cx, y, bw, bh);
            _backToMenuBtn = new Rectangle(cx, y + bh + gap, bw, bh);
            DrawSimpleButton(g, _tryAgainBtn, "Try Again");
            DrawSimpleButton(g, _backToMenuBtn, "Back to Menu");
        }

        private void DrawSimpleButton(Graphics g, Rectangle r, string text)
        {
            g.FillRectangle(Brushes.Black, r);
            g.DrawRectangle(Pens.White, r);
            using (var font = new Font("Arial", 18))
            {
                var sz = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.White,
                    r.X + (r.Width - sz.Width) / 2,
                    r.Y + (r.Height - sz.Height) / 2);
            }
        }

        public bool HandleMouseClick(MouseEventArgs e)
        {
            if (_state == GameState.GameOver)
            {
                if (_tryAgainBtn.Contains(e.Location))
                {
                    RestartGame();
                    return true;
                }
                if (_backToMenuBtn.Contains(e.Location))
                {
                    Form.ActiveForm?.Hide();
                    using (var menu = new StartMenuForm())
                        if (menu.ShowDialog() == DialogResult.OK)
                            RestartGame();
                    return true;
                }
                return false;
            }

            if (_state == GameState.Shop)
            {
                if (_shopBuyBtn.Contains(e.Location))
                {
                    if (_player.GetMoney() >= 15)
                    {
                        _player.AddMoney(-15);
                        _player.Heal(20);
                        _justBought = true;
                        _buyPulseTimer = 15;
                    }
                    return true;
                }
                if (_shopContinueBtn.Contains(e.Location))
                {
                    _state = GameState.Playing;
                    return true;
                }
                return false;
            }

            return false;
        }

        private void RestartGame()
        {
            _player.Reset();
            _deathHandled = false;
            _shopTriggered = false;
            _justBought = false;
            _buyPulseTimer = 0;
            _zombies.Clear();
            _enemyProjectiles.Clear();
            _waveManager = new WaveManager(_zombies, _map, _player, _enemyProjectiles);
            _pickups.Clear();
            _bullets.Clear();
            SpawnPickups();
            _state = GameState.Playing;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_state == GameState.Playing &&
                e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D5)
            {
                _player.SetCurrentWeaponIndex(e.KeyCode - Keys.D1);
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (_state == GameState.Playing) _state = GameState.Paused;
                else if (_state == GameState.Paused ||
                         _state == GameState.Inventory) _state = GameState.Playing;
                return;
            }
            if (e.KeyCode == Keys.I && _state == GameState.Playing)
            {
                _state = GameState.Inventory;
                return;
            }
            if (_state == GameState.Playing || _state == GameState.Shop)
                _player.OnKeyDown(e);
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (_state == GameState.Playing || _state == GameState.Shop)
                _player.OnKeyUp(e);
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if ((_state == GameState.Playing || _state == GameState.Shop)
                && e.Button == MouseButtons.Left)
            {
                _lastMousePos = e.Location;
                if (_player.GetCurrentWeaponIndex() != 1 && _player.CanFire())
                {
                    FireAt(e.Location);
                    _player.ResetFireCooldown();
                }
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (_state == GameState.Playing || _state == GameState.Shop)
                _lastMousePos = e.Location;
        }

        private void FireAt(Point screenPos)
        {
            float wx = _camera.Position.X + screenPos.X / _camera.Zoom;
            float wy = _camera.Position.Y + screenPos.Y / _camera.Zoom;
            var c = _player.GetCenter();
            float dx = wx - c.X, dy = wy - c.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist <= 0) return;
            dx /= dist; dy /= dist;
            _bullets.Add(new Bullet(c, dx, dy, _player.GetCurrentWeaponDamage()));
        }
    }
}
