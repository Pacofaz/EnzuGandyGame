using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuLauncherV2
{
    public partial class FormGamePreview : Form
    {
        private Image logoImage;
        private Image startButtonImage;
        private Rectangle startButtonRect;

        private string gameTitle;
        private Image[] galleryImages;
        private int selectedIndex = 0;

        public FormGamePreview(string title)
        {
            InitializeComponent();

            this.gameTitle = title;
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(43, 43, 43);

            logoImage = Image.FromFile("Resources/Enzulogo.png");
            startButtonImage = Image.FromFile("Resources/start_button.png");

            // Dynamische Galerie je nach Spiel:
            if (gameTitle == "Candy Game")
            {
                galleryImages = new Image[]
                {
                    Image.FromFile("Resources/candy1.png"),
                    Image.FromFile("Resources/candy2.png"),
                    Image.FromFile("Resources/candy3.png"),
                    Image.FromFile("Resources/candy4.png"),
                };
            }
            else if (gameTitle == "Igor Survival")
            {
                galleryImages = new Image[]
                {
                    Image.FromFile("Resources/igor1.png"),
                    Image.FromFile("Resources/igor2.png"),
                    Image.FromFile("Resources/igor3.png"),
                    Image.FromFile("Resources/igor4.png"),
                };
            }
            else
            {
                galleryImages = new Image[0];
            }

            // --- Start-Button-Position ---
            int btnWidth = 290;
            int btnHeight = 80;
            int btnX = this.ClientSize.Width - btnWidth - 180; // 180px vom rechten Rand
            int btnY = 180; // Je nach Optik anpassbar 
            startButtonRect = new Rectangle(btnX, btnY, btnWidth, btnHeight);

            this.MouseClick += FormGamePreview_MouseClick;
            this.Resize += (s, e) => // Damit der Button beim Resize immer oben rechts bleibt
            {
                btnX = this.ClientSize.Width - btnWidth - 80;
                startButtonRect = new Rectangle(btnX, btnY, btnWidth, btnHeight);
                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Logo oben links
            int logoWidth = 120;
            int logoHeight = 60;
            int logoX = 25;
            int logoY = 20;
            if (logoImage != null)
                e.Graphics.DrawImage(logoImage, logoX, logoY, logoWidth, logoHeight);

            // Spieltitel groß links oben
            string titel = gameTitle;
            using (Font font = new Font("Segoe UI", 48, FontStyle.Bold))
            {
                int titleX = 160;
                int titleY = 70;
                e.Graphics.DrawString(titel, font, Brushes.White, titleX, titleY);

                // X-Position für die linke Kante (exakt unter dem "C")
                int cX = titleX;
                int previewY = titleY + (int)e.Graphics.MeasureString(titel, font).Height + 10;

                // Höhe für das große Bild fest, maximale Breite z.B. 700px
                int previewHeight = 400;
                int previewMaxWidth = 700;

                int imgWidth = previewMaxWidth;
                int imgHeight = previewHeight;
                if (galleryImages != null && galleryImages.Length > 0 && galleryImages[selectedIndex] != null)
                {
                    Image img = galleryImages[selectedIndex];

                    // Höhe immer previewHeight, Breite nach Seitenverhältnis berechnen
                    imgWidth = img.Width * previewHeight / img.Height;
                    imgHeight = previewHeight;

                    // Breite anpassbar
                    if (imgWidth > previewMaxWidth)
                    {
                        imgWidth = previewMaxWidth;
                        imgHeight = img.Height * previewMaxWidth / img.Width;
                    }

                    int imgX = cX; // Startet exakt unter dem "C"
                    int imgY = previewY;

                    e.Graphics.DrawImage(img, new Rectangle(imgX, imgY, imgWidth, imgHeight));
                }

                // Thumbnails mittig unter dem großen Bild
                int thumbs = galleryImages.Length;
                int thumbWidth = 140;
                int thumbHeight = 90;
                int spacing = 30;
                int totalWidth = thumbs * thumbWidth + (thumbs - 1) * spacing;
                int startX = cX + (previewMaxWidth - totalWidth) / 2;
                int thumbY = previewY + previewHeight + 30;

                for (int i = 0; i < thumbs; i++)
                {
                    Rectangle thumbRect = new Rectangle(startX + i * (thumbWidth + spacing), thumbY, thumbWidth, thumbHeight);

                    if (i == selectedIndex)
                        e.Graphics.FillRectangle(Brushes.LightBlue, thumbRect);

                    Size thumbImgSize = GetFitSize(galleryImages[i].Size, thumbRect.Size);
                    int tx = thumbRect.X + (thumbRect.Width - thumbImgSize.Width) / 2;
                    int ty = thumbRect.Y + (thumbRect.Height - thumbImgSize.Height) / 2;
                    e.Graphics.DrawImage(galleryImages[i], new Rectangle(tx, ty, thumbImgSize.Width, thumbImgSize.Height));
                    e.Graphics.DrawRectangle(Pens.White, thumbRect);
                }
            }

            // Start-Button oben rechts
            if (startButtonImage != null)
                e.Graphics.DrawImage(startButtonImage, startButtonRect);
        }

        private void FormGamePreview_MouseClick(object sender, MouseEventArgs e)
        {
            using (Font font = new Font("Segoe UI", 48, FontStyle.Bold))
            {
                int titleX = 160;
                int titleY = 70;
                int cX = titleX;
                int previewY = titleY + (int)CreateGraphics().MeasureString(gameTitle, font).Height + 10;
                int previewMaxWidth = 700;
                int previewHeight = 400;

                int thumbs = galleryImages.Length;
                int thumbWidth = 140;
                int thumbHeight = 90;
                int spacing = 30;
                int totalWidth = thumbs * thumbWidth + (thumbs - 1) * spacing;
                int startX = cX + (previewMaxWidth - totalWidth) / 2;
                int thumbY = previewY + previewHeight + 30;

                for (int i = 0; i < thumbs; i++)
                {
                    Rectangle thumbRect = new Rectangle(startX + i * (thumbWidth + spacing), thumbY, thumbWidth, thumbHeight);
                    if (thumbRect.Contains(e.Location))
                    {
                        selectedIndex = i;
                        this.Invalidate();
                        return;
                    }
                }
            }

            // --- Start-Button Klick ---
            if (startButtonRect.Contains(e.Location))
            {
                string exePath = "";
                if (gameTitle == "Candy Game")
                    exePath = @"C:\Pfad\zu\CandyGame.exe"; //Pfad noch anpassen
                else if (gameTitle == "Igor Survival")
                    exePath = @"C:\Pfad\zu\IgorSurvival.exe"; // Pfad noch anpassen

                if (!string.IsNullOrEmpty(exePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(exePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Spiel konnte nicht gestartet werden:\n" + ex.Message);
                    }
                }
            }
        }

        // Bild fitten ohne Verzerrung 
        private Size GetFitSize(Size original, Size box)
        {
            double wr = (double)box.Width / original.Width;
            double hr = (double)box.Height / original.Height;
            double ratio = Math.Min(wr, hr);
            return new Size(
                Math.Max(1, (int)(original.Width * ratio)),
                Math.Max(1, (int)(original.Height * ratio))
            );
        }
    }
}
