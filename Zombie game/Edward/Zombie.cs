
using System;
using System.Drawing;
using ZombieGame.Entities;

namespace ZombieGame.Entities
{
    public class Zombie : Entity
    {
        private readonly Player _playerRef;
        private readonly float _detectRadius = 800f;
        private float _attackCooldown;                        // << Cooldown-Timer (in Frames)
        private const float AttackInterval = 60f;             // << 60 Frames ≈ 1 Sekunde bei 60 FPS

        public int Health { get; private set; }
        public int MaxHealth { get; }
        public bool IsDead { get; private set; }

        public Zombie(PointF pos, Player player)
            : base(pos, 2f, new SizeF(28, 28))
        {
            _playerRef = player;
            MaxHealth = 100;
            Health = MaxHealth;
            _attackCooldown = 0f;
        }

        public override void Update()
        {
            // Bewegung zum Spieler
            float dx = _playerRef.Position.X - Position.X;
            float dy = _playerRef.Position.Y - Position.Y;
            var dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < _detectRadius)
            {
                Position = new PointF(
                    Position.X + dx / dist * Speed,
                    Position.Y + dy / dist * Speed
                );
            }

            // Cooldown herunterzählen
            if (_attackCooldown > 0f)
                _attackCooldown--;
        }

        public bool CanAttack()
        {
            return _attackCooldown <= 0f;
        }

        public void ResetAttackCooldown()
        {
            _attackCooldown = AttackInterval;
        }

        public override void Draw(Graphics g)
        {
            // 1) Healthbar zeichnen
            const float barHeight = 4f;
            float barWidth = Size.Width;
            float healthPercent = Math.Max(0, Health) / (float)MaxHealth;
            var barPos = new PointF(Position.X, Position.Y - barHeight - 2f);

            // Hintergrund (Rot)
            g.FillRectangle(Brushes.Red, barPos.X, barPos.Y, barWidth, barHeight);
            // Füllung (Grün), anteilig zur Restgesundheit
            g.FillRectangle(Brushes.Lime, barPos.X, barPos.Y, barWidth * healthPercent, barHeight);

            // 2) Zombie zeichnen
            g.FillEllipse(Brushes.DarkGreen, Position.X, Position.Y, Size.Width, Size.Height);
        }


        public void Damage(int amount)
        {
            Health -= amount;
            if (Health <= 0) IsDead = true;
        }
    }
}
