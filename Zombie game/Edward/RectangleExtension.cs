
using System.Drawing;

namespace ZombieGame.Utils
{
    public static class RectangleExtensions
    {
        // Rundet ans nächstliegende Ganzzahl
        public static Rectangle ToRectangle(this RectangleF rf)
            => Rectangle.Round(rf);

        // Castet (floors) auf ints
        public static Rectangle ToRectangleFloor(this RectangleF rf)
            => new Rectangle(
                   (int)rf.X, (int)rf.Y,
                   (int)rf.Width, (int)rf.Height
               );
    }
}
