// File: Entities/Player.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame.Entities
{
    public class Player : Entity
    {
        private static readonly Bitmap[] StaticFrames;
        private static readonly SizeF EntitySize;

        private bool _facingRight = true;
        private readonly Bitmap[] _frames;
        private int _currentFrame;
        private int _frameTimer;
        private const int FrameInterval = 8;

        private readonly HashSet<Keys> _pressed = new HashSet<Keys>();
        private readonly List<string> _inventory = new List<string> { "Pistol", "Rifle" };
        private int _curWeap;
        private int _fireCd; // als int, nicht float

        public int Health { get; private set; }

        static Player()
        {
            string path = Path.Combine(Application.StartupPath, "Assets", "player32.png");
            if (!File.Exists(path))
                throw new FileNotFoundException("player32.png nicht gefunden", path);

            using (var sheet = new Bitmap(path))
            {
                int size = sheet.Height;
                int count = sheet.Width / size;
                StaticFrames = new Bitmap[count];
                for (int i = 0; i < count; i++)
                {
                    StaticFrames[i] = sheet.Clone(
                        new Rectangle(i * size, 0, size, size),
                        sheet.PixelFormat
                    );
                }
            }

            EntitySize = new SizeF(StaticFrames[0].Width * 2f, StaticFrames[0].Height * 2f);
        }

        public Player(PointF start)
            : base(start, 5f, EntitySize)
        {
            _frames = StaticFrames;
            Health = 100;
        }

        public void Heal(int amount)
        {
            Health = Math.Min(100, Health + amount);
        }

        public override void Update()
        {
            float dx = 0, dy = 0;
            if (_pressed.Contains(Keys.W)) dy -= 1;
            if (_pressed.Contains(Keys.S)) dy += 1;
            if (_pressed.Contains(Keys.A)) dx -= 1;
            if (_pressed.Contains(Keys.D)) dx += 1;

            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            bool moving = len > 0;

            if (moving)
            {
                _facingRight = dx >= 0;
                dx /= len; dy /= len;
                Position = new PointF(Position.X + dx * Speed, Position.Y + dy * Speed);

                if (++_frameTimer >= FrameInterval)
                {
                    _frameTimer = 0;
                    _currentFrame = (_currentFrame + 1) % _frames.Length;
                }
            }
            else
            {
                _currentFrame = 0;
                _frameTimer = 0;
            }

            // Cooldown pro Frame herunterzählen
            if (_fireCd > 0)
                _fireCd--;
        }

        public override void Draw(Graphics g)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var state = g.Save();
            if (_facingRight)
            {
                g.DrawImage(_frames[_currentFrame], Position.X, Position.Y, Size.Width, Size.Height);
            }
            else
            {
                g.TranslateTransform(Position.X + Size.Width, Position.Y);
                g.ScaleTransform(-1, 1);
                g.DrawImage(_frames[_currentFrame], 0, 0, Size.Width, Size.Height);
            }
            g.Restore(state);

            g.InterpolationMode = InterpolationMode.Default;
            g.PixelOffsetMode = PixelOffsetMode.Default;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            _pressed.Add(e.KeyCode);
            if (e.KeyCode >= Keys.D1 && e.KeyCode < Keys.D1 + _inventory.Count)
                _curWeap = e.KeyCode - Keys.D1;
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            _pressed.Remove(e.KeyCode);
        }

        public bool CanFire() => _fireCd <= 0;

        public void ResetFireCooldown()
        {
            // _curWeap == 0 => Pistol, langsamer
            // _curWeap == 1 => Rifle, schneller (Spray)
            _fireCd = (_curWeap == 0) ? 15 : 5;
        }

        public PointF GetCenter() => new PointF(Position.X + Size.Width / 2f, Position.Y + Size.Height / 2f);

        public int GetCurrentWeaponDamage() => (_curWeap == 0) ? 25 : 50;
        public int GetCurrentWeaponIndex() => _curWeap;
        public List<string> GetInventory() => _inventory;

        public void AddWeapon(string w)
        {
            if (!_inventory.Contains(w)) _inventory.Add(w);
        }

        public void Damage(int amt)
        {
            Health = Math.Max(0, Health - amt);
        }
    }
}
