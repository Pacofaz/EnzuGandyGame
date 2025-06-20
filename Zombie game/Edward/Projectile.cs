using System;
using System.Drawing;

namespace Edward
{
    public class Projectile
    {
        public PointF Position;
        public PointF Velocity;
        public float Lifetime; // Sekunden
        public const float MaxLifetime = 5f;
        public const float Speed = 6f;

        public Projectile(PointF start, PointF target)
        {
            Position = start;
            Lifetime = 0f;

            var dx = target.X - start.X;
            var dy = target.Y - start.Y;
            var length = (float)Math.Sqrt(dx * dx + dy * dy);
            Velocity = new PointF(dx / length * Speed, dy / length * Speed);

        }

        public void Update(float deltaTime)
        {
            Position = new PointF(Position.X + Velocity.X * deltaTime * 60, Position.Y + Velocity.Y * deltaTime * 60);
            Lifetime += deltaTime;
        }

        public bool IsAlive => Lifetime < MaxLifetime;

        public void Draw(Graphics g)
        {
            // Rote, leuchtende Kugel
            using (var brush = new SolidBrush(Color.FromArgb(255, 255, 50, 50)))
            {
                g.FillEllipse(brush, Position.X - 7, Position.Y - 7, 14, 14);
            }
            // Optional: Glow-Effekt simulieren
            using (var glow = new SolidBrush(Color.FromArgb(60, 255, 50, 50)))
            {
                g.FillEllipse(glow, Position.X - 15, Position.Y - 15, 30, 30);
            }
        }
    }
}
