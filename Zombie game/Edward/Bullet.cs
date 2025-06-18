
using System.Drawing;

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
            g.FillEllipse(Brushes.White, Position.X, Position.Y, Size.Width, Size.Height);
        }

        public bool IsOffMap(int mapWidth, int mapHeight)
        {
            return Position.X < 0 || Position.Y < 0
                || Position.X > mapWidth || Position.Y > mapHeight;
        }

        public int GetDamage() => damage;
    }
}
