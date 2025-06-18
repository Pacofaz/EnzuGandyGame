using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly Camera _camera;
        private readonly WaveManager _waveManager;

        private readonly List<Entity> _pickups;
        private readonly List<Bullet> _bullets;
        private List<Zombie> _zombies;

        private GameState _state;

        // für Hold-to-Auto-Fire
        private Point _lastMousePos;

        // verhindert mehrfaches Öffnen des Death-Menüs
        private bool _deathHandled = false;

        public Game(Size screenSize)
        {
            _screenSize = screenSize;

            _map = new Map(1024, 1024, "map.png");
            _player = new Player(new PointF(_map.Width / 2f, _map.Height / 2f));
            _player = new Player(new PointF(_map.Width / 2f, _map.Height / 2f));
            _zombies = new List<Zombie>();
            _waveManager = new WaveManager(_zombies, _map, _player);

            _camera = new Camera(screenSize, _player)
            {
                Zoom = 1.9f
            };

            _pickups = new List<Entity>();
            _bullets = new List<Bullet>();
            SpawnPickups();

            _state = GameState.MainMenu;
        }

        private void SpawnPickups()
        {
            _pickups.Add(new WeaponPickup(new PointF(350, 300), "Pistol"));
            _pickups.Add(new WeaponPickup(new PointF(_map.Width - 350, _map.Height - 400), "Rifle"));
            _pickups.Add(new HealthPickup(new PointF(_map.Width / 2f + 100, _map.Height / 2f)));
        }

        public void Update()
        {
            if (_state != GameState.Playing)
                return;

            // 1) Mauszustand und Position in jedem Frame neu auslesen
            bool mouseLeftDown = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (Form.ActiveForm != null)
                _lastMousePos = Form.ActiveForm.PointToClient(Cursor.Position);

            // 2) Welle & Spieler
            _waveManager.Update();
            _player.Update();

            // Grenze/Collision: Spieler innerhalb der Map halten
            {
                var pos = _player.Position;
                pos.X = Math.Max(0f, Math.Min(pos.X, _map.Width - _player.Size.Width));
                pos.Y = Math.Max(0f, Math.Min(pos.Y, _map.Height - _player.Size.Height));
                _player.Position = pos;
            }

            // 3) Zombies updaten
            foreach (var z in _zombies)
                z.Update();

            // 4) Schaden durch Zombies & Tod prüfen
            var playerRect = new RectangleF(_player.Position, _player.Size);
            foreach (var z in _zombies)
            {
                var zombieRect = new RectangleF(z.Position, z.Size);
                if (zombieRect.IntersectsWith(playerRect) && z.CanAttack())
                {
                    _player.Damage(10);
                    z.ResetAttackCooldown();

                    if (_player.Health <= 0 && !_deathHandled)
                    {
                        _deathHandled = true;
                        var currentForm = Form.ActiveForm;
                        currentForm?.Hide();

                        using (var menu = new StartMenuForm())
                        {
                            var dr = menu.ShowDialog();
                            if (dr == DialogResult.OK)
                                Application.Restart();
                            else
                                Application.Exit();
                        }
                        return;
                    }
                }
            }

            // 5) Pickups aufsammeln
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var pickup = _pickups[i];
                var puRect = new RectangleF(pickup.Position, pickup.Size);
                var plRect = new RectangleF(_player.Position, _player.Size);
                if (!plRect.IntersectsWith(puRect)) continue;

                if (pickup is HealthPickup hp) _player.Heal(hp.GetHealAmount());
                else if (pickup is WeaponPickup wp) _player.AddWeapon(wp.WeaponName);
                _pickups.RemoveAt(i);
            }

            // 6) Bullets updaten & Kollision prüfen
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var b = _bullets[i];
                b.Update();
                if (b.IsOffMap(_map.Width, _map.Height))
                {
                    _bullets.RemoveAt(i);
                    continue;
                }

                bool removed = false;
                for (int j = _zombies.Count - 1; j >= 0; j--)
                {
                    var z = _zombies[j];
                    if (new RectangleF(b.Position, b.Size)
                        .IntersectsWith(new RectangleF(z.Position, z.Size)))
                    {
                        z.Damage(b.GetDamage());
                        _bullets.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
                if (removed) continue;
            }

            // 7) Hold-to-Auto-Fire fürs Sturmgewehr (Index 1)
            if (mouseLeftDown
                && _player.GetCurrentWeaponIndex() == 1
                && _player.CanFire())
            {
                FireAt(_lastMousePos);
                _player.ResetFireCooldown();
            }

            // 8) Kamera updaten
            _camera.Update();
        }

        public void Draw(Graphics g)
        {
            switch (_state)
            {
                case GameState.MainMenu:
                    DrawMainMenu(g);
                    break;

                default:
                    g.ResetTransform();
                    g.ScaleTransform(_camera.Zoom, _camera.Zoom);
                    g.TranslateTransform(-_camera.Position.X, -_camera.Position.Y);

                    // Welt zeichnen
                    _map.Draw(g);

                    foreach (var wp in _pickups) wp.Draw(g);
                    _player.Draw(g);
                    foreach (var z in _zombies) z.Draw(g);
                    foreach (var b in _bullets) b.Draw(g);

                    // UI
                    g.ResetTransform();
                    UI.DrawGame(g, _player, _waveManager, _screenSize);
                    if (_state == GameState.Paused) UI.DrawPause(g, _screenSize);
                    else if (_state == GameState.Inventory) UI.DrawInventory(g, _player, _screenSize);
                    break;
            }
        }

        private void DrawMainMenu(Graphics g)
        {
            g.Clear(Color.Black);
            using (var font = new Font("Arial", 64, FontStyle.Bold))
            {
                const string t = "Zombie Survival";
                var sz = g.MeasureString(t, font);
                g.DrawString(t, font, Brushes.Red,
                    (_screenSize.Width - sz.Width) / 2,
                    _screenSize.Height / 3);
            }
            using (var font = new Font("Arial", 24))
            {
                const string i = "Press ENTER to Start";
                var sz = g.MeasureString(i, font);
                g.DrawString(i, font, Brushes.White,
                    (_screenSize.Width - sz.Width) / 2,
                    _screenSize.Height / 2);
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (_state == GameState.MainMenu && e.KeyCode == Keys.Enter)
            {
                _state = GameState.Playing;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (_state == GameState.Playing) _state = GameState.Paused;
                else if (_state == GameState.Paused || _state == GameState.Inventory)
                    _state = GameState.Playing;
                return;
            }
            if (e.KeyCode == Keys.I && _state == GameState.Playing)
            {
                _state = GameState.Inventory;
                return;
            }
            if (_state == GameState.Playing)
                _player.OnKeyDown(e);
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (_state == GameState.Playing)
                _player.OnKeyUp(e);
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (_state != GameState.Playing || e.Button != MouseButtons.Left)
                return;

            _lastMousePos = e.Location;

            if (_player.GetCurrentWeaponIndex() != 1 && _player.CanFire())
            {
                FireAt(e.Location);
                _player.ResetFireCooldown();
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (_state != GameState.Playing)
                return;

            _lastMousePos = e.Location;
        }


        private void FireAt(Point screenPos)
        {
            float worldX = _camera.Position.X + (screenPos.X / _camera.Zoom);
            float worldY = _camera.Position.Y + (screenPos.Y / _camera.Zoom);
            PointF center = _player.GetCenter();
            float dx = worldX - center.X, dy = worldY - center.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist <= 0) return;
            dx /= dist; dy /= dist;
            _bullets.Add(new Bullet(center, dx, dy, _player.GetCurrentWeaponDamage()));
        }
    }
}
