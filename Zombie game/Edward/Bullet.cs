using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZombieGame.Entities
{
    public class Bullet : Entity
    {
        private readonly float dirX, dirY;
        private readonly int damage;

        public Bullet(PointF startPos, float dx, float dy, int damage)
            : base(new PointF(startPos.X - 4, startPos.Y - 4), 0f, new SizeF(8, 8))
        {
            dirX = dx;
            dirY = dy;
            this.damage = damage;
        }

        public override void Update()
        {
            Position = new PointF(
                Position.X + dirX * 15f,
                Position.Y + dirY * 15f
            );
        }

        public override void Draw(Graphics g)
        {
            var rect = new RectangleF(Position.X, Position.Y, Size.Width, Size.Height);

            // 1) Großer, diffuser Glow
            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(rect);
                using (var glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(0, 255, 50, 50);
                    glowBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 50, 50) };
                    // Ausdehnung des Glows
                    glowBrush.FocusScales = new PointF(0.8f, 0.8f);
                    // Erweitere das Pfadgebiet für noch diffuseren Rand:
                    var inflated = rect;
                    inflated.Inflate(12, 12);
                    using (var inflatedPath = new GraphicsPath())
                    {
                        inflatedPath.AddEllipse(inflated);
                        g.FillPath(glowBrush, inflatedPath);
                    }
                }
            }

            // 2) Mittlerer Glow-Ring
            using (var ringPath = new GraphicsPath())
            {
                // etwas kleinerer Ring
                var ringRect = rect;
                ringRect.Inflate(4, 4);
                ringPath.AddEllipse(ringRect);
                using (var ringBrush = new PathGradientBrush(ringPath))
                {
                    ringBrush.CenterColor = Color.FromArgb(120, 255, 50, 50);
                    ringBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 50, 50) };
                    ringBrush.FocusScales = new PointF(0.5f, 0.5f);
                    g.FillPath(ringBrush, ringPath);
                }
            }

            // 3) Harter roter Kern
            using (var coreBrush = new SolidBrush(Color.FromArgb(255, 255, 50, 50)))
            {
                g.FillEllipse(coreBrush, rect);
            }
        }

        public bool IsOffMap(int mapWidth, int mapHeight)
        {
            return Position.X < 0 || Position.Y < 0
                || Position.X > mapWidth || Position.Y > mapHeight;
        }

        public int GetDamage() => damage;
    }
}
