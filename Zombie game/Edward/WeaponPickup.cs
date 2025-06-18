// File: Entities/WeaponPickup.cs
using System.Drawing;

namespace ZombieGame.Entities
{
    public class WeaponPickup : Entity
    {
        public string WeaponName { get; }

        public WeaponPickup(PointF centerPos, string name)
            : base(new PointF(centerPos.X - 12, centerPos.Y - 12), 0f, new SizeF(24, 24))
        {
            WeaponName = name;
        }

        public override void Update()
        {
            // steht still
        }

        public override void Draw(Graphics g)
        {
            using (var brush = new SolidBrush(Color.Gold))
                g.FillRectangle(brush, Position.X, Position.Y, Size.Width, Size.Height);

            using (var font = new Font("Arial", 10, FontStyle.Bold))
                g.DrawString(WeaponName.Substring(0, 1), font, Brushes.Black, Position.X + 4, Position.Y + 2);

            g.DrawRectangle(Pens.White, Position.X, Position.Y, Size.Width, Size.Height);
        }
    }
}
