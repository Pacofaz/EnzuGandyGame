// Engine/Engine.cs
using System.Drawing;

namespace ZombieGame.Engine
{
    public static class RectangleExtension
    {
        public static PointF Center(this RectangleF r)
            => new(r.X + r.Width / 2, r.Y + r.Height / 2);
    }

    public class Camera
    {
        public PointF Position { get; private set; }
        public void Follow(PointF target, SizeF viewport)
            => Position = new PointF(target.X - viewport.Width / 2, target.Y - viewport.Height / 2);
        public void ApplyTransform(Graphics g)
            => g.TranslateTransform(-Position.X, -Position.Y);
    }

    public class Map
    {
        private readonly int[,] tiles;
        public SizeF TileSize { get; }
        public Map(int[,] tiles, SizeF tileSize) => (this.tiles, TileSize) = (tiles, tileSize);

        public void Draw(Graphics g)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    if (tiles[x, y] == 1)
                    {
                        var rect = new RectangleF(x * TileSize.Width, y * TileSize.Height, TileSize.Width, TileSize.Height);
                        g.FillRectangle(Brushes.DarkGray, rect);
                    }
                }
        }
    }
}
