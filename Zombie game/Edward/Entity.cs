// File: Entities/Entity.cs
using System.Drawing;

namespace ZombieGame.Entities
{
    public abstract class Entity
    {
        public PointF Position;
        public SizeF Size;
        public float Speed;

        protected Entity(PointF pos, float speed, SizeF size)
        {
            Position = pos;
            Speed = speed;
            Size = size;
        }

        public abstract void Update();
        public abstract void Draw(Graphics g);
    }
}
