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
        private static readonly Bitmap IdleFrame;
        private static readonly Bitmap[] RunFrames;
        private static readonly SizeF EntitySize;
        private const int MaxHealth = 100;
        private const float Scale = 0.7f; 

        private readonly PointF _startPosition;
        private bool _facingRight = true;
        private Bitmap[] _currentAnim;
        private int _currentFrame;
        private int _frameTimer;
        private const int FrameInterval = 8;

        private readonly HashSet<Keys> _pressed = new HashSet<Keys>();
        private readonly List<string> _inventory = new List<string>();
        private int _curWeap;
        private int _fireCd;

        public int Health { get; private set; }

        static Player()
        {
            // Idle-Frame laden
            string idlePath = Path.Combine(Application.StartupPath, "Assets", "player_idle.png");
            if (!File.Exists(idlePath))
                throw new FileNotFoundException("player_idle.png nicht gefunden", idlePath);
            IdleFrame = new Bitmap(idlePath);

            // Run-Spritesheet laden und in zwei Frames splitten
            string runPath = Path.Combine(Application.StartupPath, "Assets", "player_run.png");
            if (!File.Exists(runPath))
                throw new FileNotFoundException("player_run.png nicht gefunden", runPath);
            using (var sheet = new Bitmap(runPath))
            {
                int frameWidth = sheet.Width / 2;
                int frameHeight = sheet.Height;
                RunFrames = new Bitmap[2];
                RunFrames[0] = sheet.Clone(new Rectangle(0, 0, frameWidth, frameHeight), sheet.PixelFormat);
                RunFrames[1] = sheet.Clone(new Rectangle(frameWidth, 0, frameWidth, frameHeight), sheet.PixelFormat);
            }

            // EntitySize basierend auf Scale
            EntitySize = new SizeF(IdleFrame.Width * Scale, IdleFrame.Height * Scale);
        }

        public Player(PointF start)
            : base(start, 5f, EntitySize)
        {
            _startPosition = start;
            Reset();
        }

        private void InitializeStats()
        {
            Health = MaxHealth;
            _inventory.Clear();
            _inventory.Add("Pistol");
            _inventory.Add("Rifle");
            _curWeap = 0;
            _fireCd = 0;
            _currentFrame = 0;
            _frameTimer = 0;
            _pressed.Clear();
            _currentAnim = new[] { IdleFrame };
        }

        public void Reset()
        {
            Position = _startPosition;
            InitializeStats();
        }

        public void Heal(int amount) => Health = Math.Min(MaxHealth, Health + amount);

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
                _currentAnim = RunFrames;
                _facingRight = dx >= 0;
                dx /= len; dy /= len;
                Position = new PointF(
                    Position.X + dx * Speed,
                    Position.Y + dy * Speed
                );

                if (++_frameTimer >= FrameInterval)
                {
                    _frameTimer = 0;
                    _currentFrame = (_currentFrame + 1) % _currentAnim.Length;
                }
            }
            else
            {
                _currentAnim = new[] { IdleFrame };
                _currentFrame = 0;
                _frameTimer = 0;
            }

            if (_fireCd > 0) _fireCd--;
        }

        public override void Draw(Graphics g)
        {
            var frame = _currentAnim[_currentFrame];

            // Zielgröße und Position berechnen
            float drawW = EntitySize.Width;
            float drawH = EntitySize.Height;
            float centerX = Position.X;
            float centerY = Position.Y;
            float drawX = centerX - drawW / 2f;
            float drawY = centerY - drawH / 2f;

            // --- Weicher Schatten-Kreis etwas höher und größer ---
            float ellipseW = drawW * 1.1f;    
            float ellipseH = drawH * 0.3f;
            // horizontale Mitte
            float ellipseX = centerX - ellipseW / 2f;

            float ellipseY = centerY + drawH / 2f - ellipseH + 8f;

            using (var path = new GraphicsPath())
            {
                var shadowRect = new RectangleF(ellipseX, ellipseY, ellipseW, ellipseH);
                path.AddEllipse(shadowRect);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(140, 0, 0, 0);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
                    pgb.FocusScales = new PointF(0.5f, 0.5f);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(pgb, shadowRect);
                }
            }
            // zurück auf pixel-art Modus
            g.SmoothingMode = SmoothingMode.None;

            // Sprite selbst in Pixel-Optik zeichnen
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var state = g.Save();
            if (_facingRight)
            {
                g.DrawImage(frame, drawX, drawY, drawW, drawH);
            }
            else
            {
                g.TranslateTransform(drawX + drawW, drawY);
                g.ScaleTransform(-1, 1);
                g.DrawImage(frame, 0, 0, drawW, drawH);
            }
            g.Restore(state);

            // Grafik-Modi zurücksetzen
            g.InterpolationMode = InterpolationMode.Default;
            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.SmoothingMode = SmoothingMode.None;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            _pressed.Add(e.KeyCode);
            if (e.KeyCode >= Keys.D1 && e.KeyCode < Keys.D1 + _inventory.Count)
                _curWeap = e.KeyCode - Keys.D1;
        }

        public void OnKeyUp(KeyEventArgs e) => _pressed.Remove(e.KeyCode);

        public bool CanFire() => _fireCd <= 0;

        public void ResetFireCooldown() => _fireCd = (_curWeap == 0) ? 15 : 5;

        public PointF GetCenter() => new PointF(Position.X, Position.Y);

        public int GetCurrentWeaponDamage() => (_curWeap == 0) ? 25 : 50;
        public int GetCurrentWeaponIndex() => _curWeap;
        public List<string> GetInventory() => _inventory;

        public void AddWeapon(string w)
        {
            if (!_inventory.Contains(w))
                _inventory.Add(w);
        }

        public void Damage(int amt) => Health = Math.Max(0, Health - amt);
    }
}
