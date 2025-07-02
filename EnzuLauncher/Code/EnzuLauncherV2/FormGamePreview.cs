using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuLauncherV2
{
    public partial class FormGamePreview : Form
    {
        private Image logoImage;
        private Image startButtonImage;
        private Image uskImage;
        private Rectangle startButtonRect;
        private Rectangle uskRect;

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

            // Dynamische Galerie und USK-Bild je nach Spiel:
            if (gameTitle == "Candy Game")
            {
                galleryImages = new Image[]
                {
                    Image.FromFile("Resources/candy1.png"),
                    Image.FromFile("Resources/candy2.png"),
                    Image.FromFile("Resources/candy3.png"),
                    Image.FromFile("Resources/candy4.png"),
                };
                uskImage = Image.FromFile("Resources/usk12.png");
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
                uskImage = Image.FromFile("Resources/usk18.png");
            }
            else
            {
                galleryImages = new Image[0];
                uskImage = null;
            }

            // --- Start-Button-Position ---
            int btnWidth = 290;
            int btnHeight = 80;
            int btnX = this.ClientSize.Width - btnWidth - 180;
            int btnY = 180;
            startButtonRect = new Rectangle(btnX, btnY, btnWidth, btnHeight);

            // --- USK-Bild direkt darunter ---
            int uskWidth = 290;
            int uskHeight = 120;
            int uskX = btnX + (btnWidth - uskWidth) / 2;
            int uskY = btnY + btnHeight + 0;
            uskRect = new Rectangle(uskX, uskY, uskWidth, uskHeight);

            this.MouseClick += FormGamePreview_MouseClick;
            this.Resize += (s, e) =>
            {
                btnX = this.ClientSize.Width - btnWidth - 80;
                startButtonRect = new Rectangle(btnX, btnY, btnWidth, btnHeight);

                uskX = btnX + (btnWidth - uskWidth) / 2;
                uskY = btnY + btnHeight + 24;
                uskRect = new Rectangle(uskX, uskY, uskWidth, uskHeight);

                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Logo oben links
            int logoWidth = 120;
            int logoHeight = 90;
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

                int cX = titleX;
                int previewY = titleY + (int)e.Graphics.MeasureString(titel, font).Height + 10;

                int previewHeight = 400;
                int previewMaxWidth = 700;
                int imgWidth = previewMaxWidth;
                int imgHeight = previewHeight;

                if (galleryImages != null && galleryImages.Length > 0 && galleryImages[selectedIndex] != null)
                {
                    Image img = galleryImages[selectedIndex];

                    imgWidth = img.Width * previewHeight / img.Height;
                    imgHeight = previewHeight;

                    if (imgWidth > previewMaxWidth)
                    {
                        imgWidth = previewMaxWidth;
                        imgHeight = img.Height * previewMaxWidth / img.Width;
                    }

                    int imgX = cX;
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

            // USK-Bild darunter
            if (uskImage != null)
                e.Graphics.DrawImage(uskImage, uskRect);

            // Text unterhalb des USK-Bildes
            int textStartX = uskRect.X;
            int textStartY = uskRect.Bottom + 10;
            int lineHeight = 25;

            using (Font textFont = new Font("Segoe UI", 16, FontStyle.Regular))
            {
                e.Graphics.DrawString("Entwickler: Enzu Games", textFont, Brushes.White, textStartX, textStartY);
                e.Graphics.DrawString("Plattform: Windows", textFont, Brushes.White, textStartX, textStartY + lineHeight);
                e.Graphics.DrawString("Veröffentlichungsdatum: 01.07.2025", textFont, Brushes.White, textStartX, textStartY + 2 * lineHeight);
            }
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
                    exePath = @"C:\Pfad\zu\CandyGame.exe"; // <-- Muss ich noch anpassen (WICHTIG)
                else if (gameTitle == "Igor Survival")
                    exePath = @"C:\Pfad\zu\IgorSurvival.exe"; // <-- ANPASSEN!!

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
