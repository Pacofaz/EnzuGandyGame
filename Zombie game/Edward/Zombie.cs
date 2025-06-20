using System;
using System.Drawing;

namespace ZombieGame.Entities
{
    public class Zombie : Entity
    {
        protected readonly Player _playerRef; // <-- PROTECTED!
        protected float _attackCooldown;
        protected const float AttackInterval = 60f;

        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public bool IsDead { get; protected set; }
        private readonly float _detectRadius = 800f;
        public Zombie(PointF pos, Player player, int maxHealth = 100)
           : base(pos, 2f, new SizeF(28, 28))
        {
            _playerRef = player;
            MaxHealth = maxHealth;
            Health = MaxHealth;
            _attackCooldown = 0f;
        }

        public override void Update()
        {
            // ... wie gehabt
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
