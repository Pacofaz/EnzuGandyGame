using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame.Utils
{
    /// <summary>
    /// Stellt die Spielkarte dar: Verwalten von Map-Größe, Hintergrund (Farbe oder Bild), und Rahmen-Rendering.
    /// </summary>
    public class Map
    {
        public int Width { get; }
        public int Height { get; }

        private const float BorderThickness = 10f;
        private static readonly Color BackgroundColor = Color.DarkGray;
        private static readonly Color BorderColor = Color.DimGray;

        private readonly Bitmap _backgroundImage;

        /// <summary>
        /// Erstellt eine Map mit optionalem Hintergrundbild.
        /// </summary>
        /// <param name="width">Breite der Map</param>
        /// <param name="height">Höhe der Map</param>
        /// <param name="backgroundAsset">Optionales Hintergrundbild (Dateiname im Assets-Ordner)</param>
        public Map(int width, int height, string backgroundAsset = null)
        {
            Width = width;
            Height = height;

            // Lädt das Hintergrundbild, falls angegeben
            if (!string.IsNullOrWhiteSpace(backgroundAsset))
            {
                var path = Path.Combine(Application.StartupPath, "Assets", backgroundAsset);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Background asset '{backgroundAsset}' nicht gefunden.", path);
                _backgroundImage = new Bitmap(path);
            }
        }

        /// <summary>
        /// Zeichnet die Map (entweder Hintergrundbild oder einfache Farbe) sowie einen Rahmen.
        /// </summary>
        public void Draw(Graphics g)
        {
            // Hintergrund: Bild (skaliert) oder Farbfläche
            if (_backgroundImage != null)
            {
                g.DrawImage(_backgroundImage, 0, 0, Width, Height);
            }
            else
            {
                g.Clear(BackgroundColor);
            }

            // Rahmen um die Map zeichnen
            using (var pen = new Pen(BorderColor, BorderThickness))
            {
                g.DrawRectangle(
                    pen,
                    BorderThickness / 2f,
                    BorderThickness / 2f,
                    Width - BorderThickness,
                    Height - BorderThickness
                );
            }
        }
    }
}
