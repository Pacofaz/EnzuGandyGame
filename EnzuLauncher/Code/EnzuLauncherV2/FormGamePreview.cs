using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuLauncherV2
{
    public partial class FormGamePreview : Form
    {
        private Image logoImage;
        private string gameTitle;

        public FormGamePreview(string title)
        {
            InitializeComponent();

            // Setze die Daten für das aktuelle Spiel
            this.gameTitle = title;

            // Layout
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(43, 43, 43);

            // Logo laden (links oben)
            logoImage = Image.FromFile("Resources/Enzulogo.png");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Logo oben links (z.B. 120x60 Pixel)
            int logoWidth = 120;
            int logoHeight = 100;
            int logoX = 25;
            int logoY = 20;
            if (logoImage != null)
                e.Graphics.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);

            // *** ENZU GAMES Text wurde entfernt! ***

            // Spieltitel groß links oben (z.B. 48pt)
            using (Font font = new Font("Segoe UI", 48, FontStyle.Bold))
            {
                e.Graphics.DrawString(gameTitle, font, Brushes.White, 210, 60);
            }
        }
    }
}
