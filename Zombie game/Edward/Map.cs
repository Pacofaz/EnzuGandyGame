// File: Utils/Map.cs
using System.Collections.Generic;
using System.Drawing;

namespace ZombieGame.Utils
{
    public class Map
    {
        public int Width { get; }
        public int Height { get; }

        // Straßen-Konfiguration
        private const float RoadThickness = 80f;
        private const float RoadSpacingX = 800f; // Abstand der Vertikalen Straßen
        private const float RoadSpacingY = 800f; // Abstand der Horizontalen Straßen

        public Map(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public void Draw(Graphics g)
        {
            g.Clear(Color.FromArgb(20, 20, 20));

            using (var roadPen = new Pen(Color.DimGray, RoadThickness))
            {
                // Vertikale Straßen
                for (float x = RoadSpacingX; x < Width; x += RoadSpacingX)
                    g.DrawLine(roadPen, x, 0, x, Height);

                // Horizontale Straßen
                for (float y = RoadSpacingY; y < Height; y += RoadSpacingY)
                    g.DrawLine(roadPen, 0, y, Width, y);
            }

            // Beispiel-Häuser (wird später von Building gezeichnet)
            // hier nur Dekoration:
            g.FillRectangle(Brushes.Maroon, 150, 150, 400, 300);
            g.FillRectangle(Brushes.Navy, Width - 600, Height - 600, 500, 400);
        }

        /// <summary>
        /// Liefert alle Straßen als RectangleF-Areale,
        /// damit man beim Spawnen von Gebäuden Kollision testen kann.
        /// </summary>
        public IEnumerable<RectangleF> GetRoadAreas()
        {
            var roads = new List<RectangleF>();

            // Vertikale Straßen
            for (float x = RoadSpacingX; x < Width; x += RoadSpacingX)
                roads.Add(new RectangleF(x - RoadThickness / 2f, 0, RoadThickness, Height));

            // Horizontale Straßen
            for (float y = RoadSpacingY; y < Height; y += RoadSpacingY)
                roads.Add(new RectangleF(0, y - RoadThickness / 2f, Width, RoadThickness));

            return roads;
        }
    }
}
