using System;
using System.Drawing;
using System.IO;

namespace ZombieGame.Entities
{
    public class Zombie : Entity
    {

        private const float SpriteScale = 0.5f;
        private const int FrameWidth = 73;           
        private const int FrameHeight = 99;

        private readonly Bitmap _spriteIdle;         
        private readonly Bitmap _spriteWalk;         
        private int _currentWalkFrame;
        private const int WalkFrameCount = 2;
        private const float WalkAnimationSpeed = 0.2f;
        private float _walkAnimTimer;

        // --- Gameplay ---
        protected readonly Player _playerRef;
        protected float _attackCooldown;
        protected const float AttackInterval = 60f;
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public bool IsDead { get; protected set; }
        private readonly float _detectRadius = 800f;

        public Zombie(PointF pos, Player player, int maxHealth = 100)
            : base(
                pos,
                speed: 2f,
                size: new SizeF(FrameWidth * SpriteScale, FrameHeight * SpriteScale)
              )
        {
            _playerRef = player;
            MaxHealth = maxHealth;
            Health = MaxHealth;
            _attackCooldown = 0f;

            // --- Bilder aus Assets laden ---
            string assetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            _spriteIdle = (Bitmap)Image.FromFile(Path.Combine(assetDir, "zombie_idle.png"));
            _spriteWalk = (Bitmap)Image.FromFile(Path.Combine(assetDir, "zombie_walk.png"));

            _currentWalkFrame = 0;
            _walkAnimTimer = 0f;
        }

        public override void Update()
        {
            // 1) Bewegung in Richtung Spieler
            float dx = _playerRef.Position.X - Position.X;
            float dy = _playerRef.Position.Y - Position.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            bool isWalking = false;

            if (dist < _detectRadius && dist > 0f)
            {
                Position = new PointF(
                    Position.X + dx / dist * Speed,
                    Position.Y + dy / dist * Speed
                );
                isWalking = true;
            }

            // 2) Attack-Cooldown runterzählen
            if (_attackCooldown > 0f)
                _attackCooldown--;

            // 3) Walk-Animation updaten
            if (isWalking)
            {
                _walkAnimTimer += WalkAnimationSpeed;
                if (_walkAnimTimer >= WalkFrameCount)
                    _walkAnimTimer = 0f;
                _currentWalkFrame = (int)_walkAnimTimer;
            }
            else
            {
                _walkAnimTimer = 0f;
                _currentWalkFrame = 0;
            }
        }

        public bool CanAttack() => _attackCooldown <= 0f;
        public void ResetAttackCooldown() => _attackCooldown = AttackInterval;

        public override void Draw(Graphics g)
        {
            // --- 1) Sprite auswählen ---
            Bitmap spriteToDraw;
            Rectangle srcRect;

            if (_currentWalkFrame > 0)
            {
                spriteToDraw = _spriteWalk;
                srcRect = new Rectangle(
                    FrameWidth * _currentWalkFrame,
                    0,
                    FrameWidth,
                    FrameHeight
                );
            }
            else
            {
                spriteToDraw = _spriteIdle;
                srcRect = new Rectangle(0, 0, FrameWidth, FrameHeight);
            }

            // --- 2) Zielrechteck (skaliert & zentriert auf Position) ---
            var destRect = new RectangleF(
                Position.X - (FrameWidth * SpriteScale - Size.Width) / 2f,
                Position.Y - (FrameHeight * SpriteScale - Size.Height) / 2f,
                FrameWidth * SpriteScale,
                FrameHeight * SpriteScale
            );

            // --- 3) Healthbar ÜBER dem Sprite ---
            const float barHeight = 4f;
            float barWidth = destRect.Width;
            float healthPercent = Math.Max(0, Health) / (float)MaxHealth;
            var barPos = new PointF(destRect.X, destRect.Y - barHeight - 2f);

            // Hintergrund (Rot)
            g.FillRectangle(Brushes.Red, barPos.X, barPos.Y, barWidth, barHeight);
            // Füllung (Grün)
            g.FillRectangle(Brushes.Lime, barPos.X, barPos.Y, barWidth * healthPercent, barHeight);

            // --- 4) Sprite zeichnen ---
            g.DrawImage(spriteToDraw, destRect, srcRect, GraphicsUnit.Pixel);
        }

        public void Damage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
                IsDead = true;
        }
    }
}
