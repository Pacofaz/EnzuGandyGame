using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuLauncherV2
{
    public partial class FormMain : Form
    {
        private Image logoImage;

        public FormMain()
        {
            InitializeComponent();

            this.Text = "Enzu Games Launcher";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(43, 43, 43); // Dunkelgrau

            // Logo laden 
            logoImage = Image.FromFile("Resources/EnzuLogo.png");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Größe des Logos (z.B. 160x80 Pixel)
            int logoWidth = 160;
            int logoHeight = 140;

            int logoX = (this.ClientSize.Width - logoWidth) / 2;
            int logoY = 5; // Abstand nach oben 

            if (logoImage != null)
            {
                // Runterskaliert und mittig oben zeichnen
                e.Graphics.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);
            }
        }
    }
}
