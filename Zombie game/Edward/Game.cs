using Edward;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZombieGame.Entities;
using ZombieGame.Enums;
using ZombieGame.Managers;
using ZombieGame.Utils;

namespace ZombieGame
{
    /// <summary>
    /// Die zentrale Game-Logikklasse: verwaltet Spielfluss, Spieler, Gegner, Shop, Wellen, Musik, Rendering & Input.
    /// </summary>
    public class Game
    {
        // --- Spiel-Konstanten (Schadensanzeige, Shop, Belohnungen) ---
        private const int DamageFlashDuration = 15;
        private const int ShopHealthCost = 15;
        private const int ShopHealthAmount = 20;
        private const int MoneyPerKill = 10;

        // --- Spielfeld, Spieler, Gegner & Verwaltung ---
        private readonly Size _screenSize;
        private readonly Map _map;
        private readonly Player _player;
        private Camera _camera;
        private WaveManager _waveManager;
        private readonly MusicPlayer _musicPlayer;
        private readonly string _musicPath;

        // --- Entitäten-Listen für Kollision & Anzeige ---
        private readonly List<Entity> _pickups = new List<Entity>();
        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Projectile> _enemyProj = new List<Projectile>();
        private readonly List<Zombie> _zombies = new List<Zombie>();

        // --- Status & Steuerelemente für verschiedene Spielphasen ---
        private GameState _state = GameState.Playing;
        private bool _deathHandled;
        private bool _shopTriggered;
        private bool _justBought;
        private int _buyPulseTimer;
        private int _damageFlashTimer;
        private Point _lastMousePos;
        private Rectangle _tryAgainBtn, _backToMenuBtn;
        private Rectangle _shopBuyBtn, _shopContinueBtn;

        /// <summary>
        /// Initialisiert das Spiel: Map, Spieler, Kamera, Gegnerwellen, Musik etc.
        /// </summary>
        public Game(Size screenSize)
        {
            _screenSize = screenSize;
            _map = new Map(1024, 1024, "map.png");
            _player = new Player(new PointF(_map.Width / 2f, _map.Height / 2f));
            _camera = new Camera(screenSize, _player) { Zoom = 1.9f };
            _waveManager = new WaveManager(_zombies, _map, _player, _enemyProj);
            _musicPlayer = new MusicPlayer();
            _musicPath = Path.Combine(Application.StartupPath, "Assets", "Music", "background.mp3");
            _musicPlayer.Play(_musicPath, loop: true);
        }

        /// <summary>
        /// Haupt-Update: Verarbeitet Spielzustand, Bewegung, Gegner, Schüsse, Shop und GameOver.
        /// </summary>
        public void Update()
        {
            bool mouseLeft = (Control.MouseButtons & MouseButtons.Left) != 0;
            if (Form.ActiveForm != null)
                _lastMousePos = Form.ActiveForm.PointToClient(Cursor.Position);

            _waveManager.Update();
            _player.Update();
            ClampPlayerToMap();
            _camera.Update();

            // Shop anzeigen, wenn neue Welle vorbereitet wird
            if (_state == GameState.Playing && !_waveManager.NextWaveScheduled)
                _shopTriggered = false;
            if (_state == GameState.Playing && !_shopTriggered && _waveManager.NextWaveScheduled)
            {
                _state = GameState.Shop;
                _shopTriggered = true;
            }
            if (_state == GameState.Shop)
                return;

            var playerRect = new RectangleF(_player.Position, _player.Size);
            UpdateZombies(playerRect);
            UpdateEnemyProjectiles(playerRect);
            UpdateBullets();

            // Spieler schießt mit Gewehr
            if (mouseLeft && _player.GetCurrentWeaponIndex() == 1 && _player.CanFire())
            {
                FireAt(_lastMousePos);
                _player.ResetFireCooldown();
            }
        }

        /// <summary>
        /// Spieler bleibt innerhalb der Map-Grenzen.
        /// </summary>
        private void ClampPlayerToMap()
        {
            var p = _player.Position;
            p.X = Math.Max(0, Math.Min(p.X, _map.Width - _player.Size.Width));
            p.Y = Math.Max(0, Math.Min(p.Y, _map.Height - _player.Size.Height));
            _player.Position = p;
        }

        /// <summary>
        /// Gegner (Zombies) updaten und prüfen auf Kollision mit Spieler.
        /// </summary>
        private void UpdateZombies(RectangleF pr)
        {
            foreach (var z in _zombies)
            {
                z.Update();
                var zr = new RectangleF(z.Position, z.Size);
                if (zr.IntersectsWith(pr) && z.CanAttack())
                {
                    HitPlayer();
                    z.ResetAttackCooldown();
                    if (_player.Health <= 0 && !_deathHandled)
                    {
                        _deathHandled = true;
                        _state = GameState.GameOver;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Feindliche Projektile (z.B. ZombieRange) updaten und prüfen auf Treffer beim Spieler.
        /// </summary>
        private void UpdateEnemyProjectiles(RectangleF pr)
        {
            for (int i = _enemyProj.Count - 1; i >= 0; i--)
            {
                var proj = _enemyProj[i];
                proj.Update(1f / 60f);
                var r = new RectangleF(proj.Position.X - 4, proj.Position.Y - 4, 14, 14);
                if (r.IntersectsWith(pr))
                {
                    HitPlayer();
                    _enemyProj.RemoveAt(i);
                    if (_player.Health <= 0 && !_deathHandled)
                    {
                        _deathHandled = true;
                        _state = GameState.GameOver;
                        return;
                    }
                }
                else if (!proj.IsAlive)
                {
                    _enemyProj.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Spieler-Projektile (Bullets) updaten, prüfen auf Treffer bei Zombies, ggf. Geld auszahlen.
        /// </summary>
        private void UpdateBullets()
        {
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
                    var br = new RectangleF(b.Position, b.Size);
                    var zr = new RectangleF(z.Position, z.Size);

                    if (!br.IntersectsWith(zr)) continue;

                    z.Damage(b.GetDamage());
                    _bullets.RemoveAt(i);

                    if (z.Health <= 0)
                    {
                        _player.AddMoney(MoneyPerKill);
                        _zombies.RemoveAt(j);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Spieler nimmt Schaden (inkl. Schadensblende für visuelles Feedback).
        /// </summary>
        private void HitPlayer()
        {
            _player.Damage(10);
            _damageFlashTimer = DamageFlashDuration;
        }

        /// <summary>
        /// Zentrales Rendering: Spielfeld, Entitäten und je nach Status (UI, Shop, GameOver).
        /// </summary>
        public void Draw(Graphics g)
        {
            // Spielfeld & Entitäten zeichnen
            g.ResetTransform();
            g.Clear(Color.DimGray);
            g.ScaleTransform(_camera.Zoom, _camera.Zoom);
            g.TranslateTransform(-_camera.Position.X, -_camera.Position.Y);

            _map.Draw(g);
            _pickups.ForEach(p => p.Draw(g));
            _player.Draw(g);
            _enemyProj.ForEach(p => p.Draw(g));
            _zombies.ForEach(z => z.Draw(g));
            _bullets.ForEach(b => b.Draw(g));

            // User Interface (UI) und Spezialanzeigen je nach Status
            g.ResetTransform();
            UI.DrawGame(g, _player, _waveManager, _screenSize);

            if (_damageFlashTimer-- > 0)
            {
                int alpha = (int)(150f * (_damageFlashTimer / (float)DamageFlashDuration));
                using (var fb = new SolidBrush(Color.FromArgb(alpha, 255, 0, 0)))
                    g.FillRectangle(fb, 0, 0, _screenSize.Width, _screenSize.Height);
            }

            switch (_state)
            {
                case GameState.GameOver: DrawGameOver(g); break;
                case GameState.Paused: UI.DrawPause(g, _screenSize); break;
                case GameState.Inventory: UI.DrawInventory(g, _player, _screenSize); break;
                case GameState.Shop: DrawShop(g); break;
            }
        }

        /// <summary>
        /// Shop-UI: Health kaufen oder weiter.
        /// </summary>
        private void DrawShop(Graphics g)
        {
            using (var ov = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(ov, 0, 0, _screenSize.Width, _screenSize.Height);

            int w = 400, h = 250;
            int x = (_screenSize.Width - w) / 2, y = (_screenSize.Height - h) / 2;
            g.FillRectangle(Brushes.Gray, x, y, w, h);
            g.DrawRectangle(Pens.White, x, y, w, h);

            using (var f = new Font("Segoe Print", 24, FontStyle.Bold))
            {
                var text = "Shop – Health kaufen";
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, Brushes.White, x + (w - sz.Width) / 2, y + 10);
            }

            _shopBuyBtn = new Rectangle(x + 20, y + 80, 180, 50);
            _shopContinueBtn = new Rectangle(x + w - 200 - 20, y + 80, 180, 50);

            float scale = 1f;
            if (_justBought && _buyPulseTimer-- > 0)
                scale = 1f + 0.1f * (_buyPulseTimer / (float)DamageFlashDuration);
            else
                _justBought = false;

            if (scale != 1f)
            {
                var c = new PointF(
                    _shopBuyBtn.X + _shopBuyBtn.Width / 2f,
                    _shopBuyBtn.Y + _shopBuyBtn.Height / 2f);
                g.TranslateTransform(c.X, c.Y);
                g.ScaleTransform(scale, scale);
                g.TranslateTransform(-c.X, -c.Y);
            }

            DrawButton(g, _shopBuyBtn, "20 HP für $15");
            g.ResetTransform();
            DrawButton(g, _shopContinueBtn, "Weiter");
        }

        /// <summary>
        /// GameOver-UI: Try Again/Back to Menu anzeigen.
        /// </summary>
        private void DrawGameOver(Graphics g)
        {
            const string msg = "YOU ARE DEAD";
            using (var f = new Font("Arial", 48, FontStyle.Bold))
            {
                var sz = g.MeasureString(msg, f);
                g.DrawString(msg, f, Brushes.Red,
                    (_screenSize.Width - sz.Width) / 2,
                    _screenSize.Height / 3);
            }

            int bw = 200, bh = 50, gap = 20;
            int cx = (_screenSize.Width - bw) / 2, yy = _screenSize.Height / 2;
            _tryAgainBtn = new Rectangle(cx, yy, bw, bh);
            _backToMenuBtn = new Rectangle(cx, yy + bh + gap, bw, bh);

            DrawButton(g, _tryAgainBtn, "Try Again");
            DrawButton(g, _backToMenuBtn, "Back to Menu");
        }

        /// <summary>
        /// Basis-Button-Renderer (UI-Buttons, z.B. Shop, GameOver).
        /// </summary>
        private void DrawButton(Graphics g, Rectangle r, string text)
        {
            g.FillRectangle(Brushes.Black, r);
            g.DrawRectangle(Pens.White, r);
            using (var f = new Font("Arial", 18))
            {
                var sz = g.MeasureString(text, f);
                g.DrawString(text, f, Brushes.White,
                    r.X + (r.Width - sz.Width) / 2,
                    r.Y + (r.Height - sz.Height) / 2);
            }
        }

        // --- Maus- und Tastatureingaben für UI/Spielsteuerung ---
        public bool HandleMouseClick(MouseEventArgs e)
        {
            // GameOver-Buttons
            if (_state == GameState.GameOver)
            {
                if (_tryAgainBtn.Contains(e.Location))
                {
                    Restart();
                    return true;
                }
                if (_backToMenuBtn.Contains(e.Location))
                {
                    _musicPlayer.Stop();
                    var gameForm = Form.ActiveForm;
                    gameForm?.Hide();
                    using (var menu = new StartMenuForm())
                    {
                        if (menu.ShowDialog() == DialogResult.OK)
                        {
                            Restart();
                            _musicPlayer.Play(_musicPath, loop: true);
                            gameForm?.Show();
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                    return true;
                }
                return false;
            }

            // Shop-Buttons
            if (_state == GameState.Shop)
            {
                if (_shopBuyBtn.Contains(e.Location))
                {
                    if (_player.GetMoney() >= ShopHealthCost)
                    {
                        _player.AddMoney(-ShopHealthCost);
                        _player.Heal(ShopHealthAmount);
                        _justBought = true;
                        _buyPulseTimer = DamageFlashDuration;
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

        /// <summary>
        /// Startet das Spiel neu: Spieler und Gegner zurücksetzen, neue Wellen starten.
        /// </summary>
        private void Restart()
        {
            _player.Reset();
            _deathHandled = false;
            _shopTriggered = false;
            _justBought = false;
            _buyPulseTimer = _damageFlashTimer = 0;
            _zombies.Clear();
            _enemyProj.Clear();
            _bullets.Clear();
            _pickups.Clear();
            _waveManager = new WaveManager(_zombies, _map, _player, _enemyProj);
            _state = GameState.Playing;
        }

        // --- Tastatur- und Mausevents an Spieler und Statusverwaltung weiterleiten ---
        public void OnKeyDown(KeyEventArgs e)
        {
            if (_state == GameState.Playing && e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D5)
            {
                _player.SetCurrentWeaponIndex(e.KeyCode - Keys.D1);
            }
            else if (e.KeyCode == Keys.Escape
                     && (_state == GameState.Playing || _state == GameState.Paused))
            {
                _state = _state == GameState.Playing
                    ? GameState.Paused
                    : GameState.Playing;
            }
            else if (e.KeyCode == Keys.I && _state == GameState.Playing)
            {
                _state = GameState.Inventory;
            }
            else if (_state == GameState.Playing || _state == GameState.Shop)
            {
                _player.OnKeyDown(e);
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (_state == GameState.Playing || _state == GameState.Shop)
                _player.OnKeyUp(e);
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if ((_state == GameState.Playing || _state == GameState.Shop) && e.Button == MouseButtons.Left)
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



        /// <summary>
        /// Projektil (Bullet) abfeuern in Richtung Mausposition.
        /// </summary>
        private void FireAt(Point sp)
        {
            float wx = _camera.Position.X + sp.X / _camera.Zoom;
            float wy = _camera.Position.Y + sp.Y / _camera.Zoom;
            var c = _player.GetCenter();
            float dx = wx - c.X, dy = wy - c.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist <= 0) return;
            dx /= dist; dy /= dist;
            _bullets.Add(new Bullet(c, dx, dy, _player.GetCurrentWeaponDamage()));
        }
    }
}
