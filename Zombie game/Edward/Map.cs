using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame.Utils
{
    public class Map
    {
        public int Width { get; }
        public int Height { get; }

        private const float BorderThickness = 10f;
        private static readonly Color BackgroundColor = Color.DarkGray;
        private static readonly Color BorderColor = Color.DimGray;

        private readonly Bitmap _backgroundImage;


        /// <param name="width"
        /// <param name="height"
        /// <param name="backgroundAsset">

        public Map(int width, int height, string backgroundAsset = null)
        {
            Width = width;
            Height = height;

            if (!string.IsNullOrWhiteSpace(backgroundAsset))
            {
                var path = Path.Combine(Application.StartupPath, "Assets", backgroundAsset);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Background asset '{backgroundAsset}' nicht gefunden.", path);
                _backgroundImage = new Bitmap(path);
            }
        }

        public void Draw(Graphics g)
        {
            // Hintergrund: Bild oder Farbe
            if (_backgroundImage != null)
            {
                // Bild über ganze Map skalieren
                g.DrawImage(_backgroundImage, 0, 0, Width, Height);
            }
            else
            {
                g.Clear(BackgroundColor);
            }

            // Rahmen
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
