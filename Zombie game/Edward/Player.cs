using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame.Entities
{
    /// <summary>
    /// Stellt den Spieler dar – Animation, Bewegung, Inventar, Leben, Waffen, Geld und Eingabe.
    /// </summary>
    public class Player : Entity
    {
        // --- Grafiksprites: Idle & Laufen für Pistole/Rifle, Einheitsgröße ---
        private static readonly Bitmap IdlePistolFrame;
        private static readonly Bitmap IdleRifleFrame;
        private static readonly Bitmap[] RunPistolFrames;
        private static readonly Bitmap[] RunRifleFrames;
        private static readonly SizeF EntitySize;
        private const int MaxHealth = 100;
        private const float Scale = 0.7f;

        // --- Status & Animation ---
        private readonly PointF _startPosition;
        private bool _facingRight = true;
        private Bitmap[] _currentAnim;
        private int _currentFrame;
        private int _frameTimer;
        private const int FrameInterval = 8;

        // --- Spieler-Status ---
        private readonly HashSet<Keys> _pressed = new HashSet<Keys>();
        private readonly List<string> _inventory = new List<string>();
        private int _curWeap;
        private int _fireCd;
        private int _money;

        public int Health { get; private set; }

        /// <summary>
        /// Lädt die Sprite-Bitmaps für Idle- und Laufanimationen einmalig.
        /// </summary>
        static Player()
        {
            string assets = Path.Combine(Application.StartupPath, "Assets");

            // Idle-Sprites laden
            string idlePPath = Path.Combine(assets, "idle_pistol.png");
            if (!File.Exists(idlePPath)) throw new FileNotFoundException("idle_pistol.png nicht gefunden", idlePPath);
            IdlePistolFrame = new Bitmap(idlePPath);

            string idleRPath = Path.Combine(assets, "idle_rifle.png");
            if (!File.Exists(idleRPath)) throw new FileNotFoundException("idle_rifle.png nicht gefunden", idleRPath);
            IdleRifleFrame = new Bitmap(idleRPath);

            // Run-Sprites (2 Frames pro Waffe, SpriteSheet zerteilen)
            string runPPath = Path.Combine(assets, "run_pistol.png");
            if (!File.Exists(runPPath)) throw new FileNotFoundException("run_pistol.png nicht gefunden", runPPath);
            using (var sheet = new Bitmap(runPPath))
            {
                int w = sheet.Width / 2, h = sheet.Height;
                RunPistolFrames = new Bitmap[2];
                RunPistolFrames[0] = sheet.Clone(new Rectangle(0, 0, w, h), sheet.PixelFormat);
                RunPistolFrames[1] = sheet.Clone(new Rectangle(w, 0, w, h), sheet.PixelFormat);
            }
            string runRPath = Path.Combine(assets, "run_rifle.png");
            if (!File.Exists(runRPath)) throw new FileNotFoundException("run_rifle.png nicht gefunden", runRPath);
            using (var sheet = new Bitmap(runRPath))
            {
                int w = sheet.Width / 2, h = sheet.Height;
                RunRifleFrames = new Bitmap[2];
                RunRifleFrames[0] = sheet.Clone(new Rectangle(0, 0, w, h), sheet.PixelFormat);
                RunRifleFrames[1] = sheet.Clone(new Rectangle(w, 0, w, h), sheet.PixelFormat);
            }

            // Spritegröße bestimmen (größte Idle-Sprite als Basis)
            float width = Math.Max(IdlePistolFrame.Width, IdleRifleFrame.Width) * Scale;
            float height = Math.Max(IdlePistolFrame.Height, IdleRifleFrame.Height) * Scale;
            EntitySize = new SizeF(width, height);
        }

        /// <summary>
        /// Erstellt einen Spieler an einer Startposition und setzt alle Stats zurück.
        /// </summary>
        public Player(PointF start)
            : base(start, 5f, EntitySize)
        {
            _startPosition = start;
            Reset();
        }

        /// <summary>
        /// Setzt Inventar, Leben, Animation, Geld etc. zurück (für Restart).
        /// </summary>
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
            _currentAnim = new[] { IdlePistolFrame };
            _money = 0;
        }

        public void Reset()
        {
            Position = _startPosition;
            InitializeStats();
        }

        public void Heal(int amount) => Health = Math.Min(MaxHealth, Health + amount);

        /// <summary>
        /// Verarbeitet Bewegung, Animation und Waffen-Cooldown.
        /// </summary>
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

                _currentAnim = (_curWeap == 0) ? RunPistolFrames : RunRifleFrames;
                if (++_frameTimer >= FrameInterval)
                {
                    _frameTimer = 0;
                    _currentFrame = (_currentFrame + 1) % _currentAnim.Length;
                }
            }
            else
            {
                _currentAnim = new[] { (_curWeap == 0) ? IdlePistolFrame : IdleRifleFrame };
                _currentFrame = 0;
                _frameTimer = 0;
            }

            if (_fireCd > 0) _fireCd--;
        }

        /// <summary>
        /// Zeichnet den Spieler inklusive Schatten und Flip bei Linksbewegung.
        /// </summary>
        public override void Draw(Graphics g)
        {
            var frame = _currentAnim[_currentFrame];
            float w = EntitySize.Width, h = EntitySize.Height;
            float cx = Position.X, cy = Position.Y;
            float x = cx - w / 2, y = cy - h / 2;

            // Schatten unter dem Spieler
            float sw = w * 1.1f, sh = h * 0.3f;
            float sx = cx - sw / 2, sy = cy + h / 2 - sh + 8f;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(new RectangleF(sx, sy, sw, sh));
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(140, 0, 0, 0);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
                    pgb.FocusScales = new PointF(0.5f, 0.5f);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(pgb, new RectangleF(sx, sy, sw, sh));
                }
            }
            g.SmoothingMode = SmoothingMode.None;

            // Sprite zeichnen (nach Blickrichtung spiegeln)
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            var state = g.Save();
            if (_facingRight) g.DrawImage(frame, x, y, w, h);
            else
            {
                g.TranslateTransform(x + w, y);
                g.ScaleTransform(-1, 1);
                g.DrawImage(frame, 0, 0, w, h);
            }
            g.Restore(state);
            g.InterpolationMode = InterpolationMode.Default;
            g.PixelOffsetMode = PixelOffsetMode.Default;
        }

        /// <summary>
        /// Verarbeitet gedrückte Tasten (Bewegung, Waffenwechsel).
        /// </summary>
        public void OnKeyDown(KeyEventArgs e)
        {
            _pressed.Add(e.KeyCode);
            if (e.KeyCode >= Keys.D1 && e.KeyCode < Keys.D1 + _inventory.Count)
            {
                SetCurrentWeaponIndex(e.KeyCode - Keys.D1);
            }
        }

        public void OnKeyUp(KeyEventArgs e) => _pressed.Remove(e.KeyCode);

        // --- Waffen/Feuern-Methoden ---
        public bool CanFire() => _fireCd <= 0;
        public void ResetFireCooldown() => _fireCd = (_curWeap == 0) ? 15 : 5;
        public int GetCurrentWeaponDamage() => (_curWeap == 0) ? 25 : 50;
        public int GetCurrentWeaponIndex() => _curWeap;
        public List<string> GetInventory() => _inventory;
        public PointF GetCenter() => new PointF(Position.X, Position.Y);

        public void AddWeapon(string w)
        {
            if (!_inventory.Contains(w))
                _inventory.Add(w);
        }

        public void Damage(int amt) => Health = Math.Max(0, Health - amt);

        // --- Geld-Methoden ---
        public void AddMoney(int amount) => _money += amount;
        public int GetMoney() => _money;

        /// <summary>
        /// Setzt die aktuelle Waffe und die Animation zurück.
        /// </summary>
        public void SetCurrentWeaponIndex(int index)
        {
            if (index < 0 || index >= _inventory.Count) return;

            _curWeap = index;
            _currentFrame = 0;
            _frameTimer = 0;
            _currentAnim = new[] { (_curWeap == 0) ? IdlePistolFrame : IdleRifleFrame };
            ResetFireCooldown();
        }
    }
}
