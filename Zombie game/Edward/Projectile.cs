using System;
using System.Drawing;

namespace Edward
{
    /// <summary>
    /// Einfaches feindliches Projektil (z.B. für Fernkampf-Zombies):
    /// Bewegt sich mit konstanter Geschwindigkeit zum Ziel, hat Lebensdauer, kann sich selbst zeichnen.
    /// </summary>
    public class Projectile
    {
        public PointF Position;
        public PointF Velocity;
        public float Lifetime; // Lebensdauer in Sekunden
        public const float MaxLifetime = 5f;
        public const float Speed = 6f;

        /// <summary>
        /// Initialisiert das Projektil und berechnet die Richtung (normalisiert auf feste Geschwindigkeit).
        /// </summary>
        public Projectile(PointF start, PointF target)
        {
            Position = start;
            Lifetime = 0f;

            var dx = target.X - start.X;
            var dy = target.Y - start.Y;
            var length = (float)Math.Sqrt(dx * dx + dy * dy);
            Velocity = new PointF(dx / length * Speed, dy / length * Speed);
        }

        /// <summary>
        /// Bewegt das Projektil weiter (Frame-basiert, Zeitkorrektur) und zählt Lebenszeit hoch.
        /// </summary>
        public void Update(float deltaTime)
        {
            Position = new PointF(Position.X + Velocity.X * deltaTime * 60, Position.Y + Velocity.Y * deltaTime * 60);
            Lifetime += deltaTime;
        }

        /// <summary>
        /// Gibt zurück, ob das Projektil noch aktiv ist.
        /// </summary>
        public bool IsAlive => Lifetime < MaxLifetime;

        /// <summary>
        /// Zeichnet das Projektil als rote Kugel mit leichtem Glow.
        /// </summary>
        public void Draw(Graphics g)
        {
            using (var brush = new SolidBrush(Color.FromArgb(255, 255, 50, 50)))
            {
                g.FillEllipse(brush, Position.X - 7, Position.Y - 7, 14, 14);
            }
            using (var glow = new SolidBrush(Color.FromArgb(60, 255, 50, 50)))
            {
                g.FillEllipse(glow, Position.X - 15, Position.Y - 15, 30, 30);
            }
        }
    }
}
