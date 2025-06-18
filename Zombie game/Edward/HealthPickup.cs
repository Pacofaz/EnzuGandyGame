using System.Drawing;

namespace ZombieGame.Entities
{
    public class HealthPickup : Entity
    {
        private const int HealAmount = 50; // 50% deiner MaxHealth

        public HealthPickup(PointF centerPos)
            : base(new PointF(centerPos.X - 12, centerPos.Y - 12), 0f, new SizeF(24, 24))
        {
        }

        public override void Update()
        {
            
        }

        public override void Draw(Graphics g)
        {
            // Rotes Herz-Rechteck
            using (var brush = new SolidBrush(Color.Red))
                g.FillEllipse(brush, Position.X, Position.Y, Size.Width, Size.Height);

            // H drauf
            using (var font = new Font("Arial", 12, FontStyle.Bold))
                g.DrawString("H", font, Brushes.White,
                    Position.X + Size.Width / 4,
                    Position.Y + Size.Height / 8);
        }

        public int GetHealAmount() => HealAmount;
    }
}
