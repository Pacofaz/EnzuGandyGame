using Edward;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ZombieGame.Entities
{
    public class ZombieRange : Zombie
    {
        private float _shootCooldown;
        private const float ShootInterval = 90f;
        private readonly List<Projectile> _projectiles;

        // ---- Animation Fields ----
        private readonly Bitmap _spriteSheet;
        private const int FrameCount = 4;
        private readonly int _frameWidth;
        private readonly int _frameHeight;
        private int _currentFrame = 0;
        private int _animTimer = 0;
        private const int AnimInterval = 10; // Updates per frame-change

        // ---- Scale down the sprite (50% of original) ----
        private const float Scale = 0.5f;

        public ZombieRange(PointF pos, Player player, List<Projectile> projectiles)
            : base(pos, player, 60)
        {
            _projectiles = projectiles;
            _shootCooldown = 0f;

            // Spritesheet laden: Gesamtbreite = 4 Frames nebeneinander
            _spriteSheet = (Bitmap)Image.FromFile("Assets/ZombieRange.png");
            _frameWidth = _spriteSheet.Width / FrameCount;
            _frameHeight = _spriteSheet.Height;

            // Setze die physische Size auf die verkleinerte Frame-Größe
            Size = new Size((int)(_frameWidth * Scale), (int)(_frameHeight * Scale));
        }

        public override void Update()
        {
            base.Update();

            // Animation
            _animTimer++;
            if (_animTimer >= AnimInterval)
            {
                _animTimer = 0;
                _currentFrame = (_currentFrame + 1) % FrameCount;
            }

            // Shoot logic
            if (_shootCooldown > 0f)
                _shootCooldown--;

            if (_shootCooldown <= 0f)
            {
                ShootAtPlayer();
                _shootCooldown = 120f;
            }
        }

        private void ShootAtPlayer()
        {
            if (_projectiles != null)
            {
                _projectiles.Add(new Projectile(Position, _playerRef.Position));
                Console.WriteLine("ZombieRange schießt! Anzahl Projektile: " + _projectiles.Count);
            }
        }

        public override void Draw(Graphics g)
        {
            // Healthbar
            const float barHeight = 4f;
            float barWidth = Size.Width;
            float healthPercent = Math.Max(0, Health) / (float)MaxHealth;
            var barPos = new PointF(Position.X, Position.Y - barHeight - 2f);

            g.FillRectangle(Brushes.Red, barPos.X, barPos.Y, barWidth, barHeight);
            g.FillRectangle(Brushes.Orange, barPos.X, barPos.Y, barWidth * healthPercent, barHeight);

            // Sprite aus Spritesheet zeichnen (skaliert)
            var srcRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            var dstRect = new RectangleF(
                Position.X,
                Position.Y,
                _frameWidth * Scale,
                _frameHeight * Scale
            );
            g.DrawImage(_spriteSheet, dstRect, srcRect, GraphicsUnit.Pixel);

            // Optional: "Arm" zum Spieler (ebenfalls skaliert mittig auf das Sprite)
            float midX = Position.X + (Size.Width / 2f);
            float midY = Position.Y + (Size.Height / 2f);
            float armLen = 18f * Scale;
            float dx = (_playerRef.Position.X + _playerRef.Size.Width / 2f) - midX;
            float dy = (_playerRef.Position.Y + _playerRef.Size.Height / 2f) - midY;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len > 0.1f)
            {
                float ex = midX + dx / len * armLen;
                float ey = midY + dy / len * armLen;
                using (var pen = new Pen(Color.DarkRed, 4f * Scale))
                {
                    g.DrawLine(pen, midX, midY, ex, ey);
                }
            }
        }
    }
}
