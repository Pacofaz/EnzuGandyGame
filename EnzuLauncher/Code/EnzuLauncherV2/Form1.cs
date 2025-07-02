using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;  // Wichtig für den Sound

namespace EnzuLauncherV2
{
    public partial class FormMain : Form
    {
        private Image logoImage;
        private Image candyImage;
        private Image igorImage;

        private Rectangle candyRect;
        private Rectangle igorRect;

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

            // Bilder laden 
            logoImage = Image.FromFile("Resources/EnzuLogo.png");
            candyImage = Image.FromFile("Resources/CandyGame.png");
            igorImage = Image.FromFile("Resources/IgorSurvival.png");

            // Bereiche für die Game-Bilder festlegen (Position und Größe)
            int imageWidth = 320;
            int imageHeight = 450;
            int imageY = 180;
            int padding = 120;
            int leftX = (this.ClientSize.Width / 2) - imageWidth - (padding / 2);
            int rightX = (this.ClientSize.Width / 2) + (padding / 2);

            candyRect = new Rectangle(leftX, imageY, imageWidth, imageHeight);
            igorRect = new Rectangle(rightX, imageY, imageWidth, imageHeight);

            // MouseClick Event abonnieren
            this.MouseClick += FormMain_MouseClick;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Logo zeichnen
            int logoWidth = 160;
            int logoHeight = 140;
            int logoX = (this.ClientSize.Width - logoWidth) / 2;
            int logoY = 5;
            if (logoImage != null)
                e.Graphics.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);

            // Candy Game Button mit Bild
            if (candyImage != null)
                e.Graphics.DrawImage(candyImage, candyRect);

            // Igor Survival Button mit Bild
            if (igorImage != null)
                e.Graphics.DrawImage(igorImage, igorRect);

            // Texte unterhalb der Bilder (Titel und Untertitel)
            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            // Candy Game Text
            var candyTitleY = candyRect.Bottom + 0;
            var candySubtitleY = candyRect.Bottom + 40;
            e.Graphics.DrawString("Candy Game", new Font("Segoe UI", 28, FontStyle.Regular), Brushes.White, candyRect.X + candyRect.Width / 2, candyTitleY, centerFormat);
            e.Graphics.DrawString("Kostenlos", new Font("Segoe UI", 14, FontStyle.Regular), Brushes.LightGray, candyRect.X + candyRect.Width / 2, candySubtitleY, centerFormat);

            // Igor Survival Text
            var igorTitleY = igorRect.Bottom + 0;
            var igorSubtitleY = igorRect.Bottom + 40;
            e.Graphics.DrawString("Igor Survival", new Font("Segoe UI", 28, FontStyle.Regular), Brushes.White, igorRect.X + igorRect.Width / 2, igorTitleY, centerFormat);
            e.Graphics.DrawString("Kostenlos", new Font("Segoe UI", 14, FontStyle.Regular), Brushes.LightGray, igorRect.X + igorRect.Width / 2, igorSubtitleY, centerFormat);
        }

        // Sound abspielen
        private void PlayClickSound()
        {
            try
            {
              
                using (SoundPlayer player = new SoundPlayer("Resources/SoundClick1.wav"))
                {
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                // Im Fehlerfall kein Absturz
                Console.WriteLine("Sound konnte nicht abgespielt werden: " + ex.Message);
            }
        }

        private void FormMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (candyRect.Contains(e.Location))
            {
                PlayClickSound();
                var preview = new FormGamePreview("Candy Game");
                preview.ShowDialog();
            }
            else if (igorRect.Contains(e.Location))
            {
                PlayClickSound();
                var preview = new FormGamePreview("Igor Survival");
                preview.ShowDialog();
            }
        }

    }
}
