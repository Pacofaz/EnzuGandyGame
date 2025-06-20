using Edward;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ZombieGame.Entities
{
    public class ZombieRange : Zombie
    {
        private float _shootCooldown;
        private const float ShootInterval = 90f; // Alle 1,5 Sekunden schießen (bei 60 FPS)
        private readonly List<Projectile> _projectiles;

        public ZombieRange(PointF pos, Player player, List<Projectile> projectiles)
            : base(pos, player, 60) // z.B. 60 Lebenspunkte für Fernkämpfer
        {
            _projectiles = projectiles;
            _shootCooldown = 0f;
        }

        public override void Update()
        {
            base.Update();

            float dx = _playerRef.Position.X - Position.X;
            float dy = _playerRef.Position.Y - Position.Y;
            var dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (_shootCooldown > 0f)
                _shootCooldown--;

            if (_shootCooldown <= 0f) // Testweise keine Distanz
            {
                ShootAtPlayer();
                _shootCooldown = 120f; // Alle 0,5 Sekunden
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

            // Hintergrund (Rot)
            g.FillRectangle(Brushes.Red, barPos.X, barPos.Y, barWidth, barHeight);
            // Füllung (Orange) für Fernkämpfer
            g.FillRectangle(Brushes.Orange, barPos.X, barPos.Y, barWidth * healthPercent, barHeight);

            // Range-Zombie-Körper (orange)
            g.FillEllipse(Brushes.OrangeRed, Position.X, Position.Y, Size.Width, Size.Height);

            // Optional: "Arm" zum Spieler
            float midX = Position.X + Size.Width / 2;
            float midY = Position.Y + Size.Height / 2;
            float armLen = 18f;
            float dx = _playerRef.Position.X + _playerRef.Size.Width / 2 - midX;
            float dy = _playerRef.Position.Y + _playerRef.Size.Height / 2 - midY;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len > 0.1f)
            {
                float ex = midX + dx / len * armLen;
                float ey = midY + dy / len * armLen;
                using (var pen = new Pen(Color.DarkRed, 4f))
                {
                    g.DrawLine(pen, midX, midY, ex, ey);
                }
            }
        }
    }
}
