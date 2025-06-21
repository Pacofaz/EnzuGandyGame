using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame
{
    public class StartMenuForm : Form
    {
        private Bitmap backgroundImage;
        private Bitmap startButtonImage;
        private Rectangle startButtonBounds;
        private bool isHovering;

        private PrivateFontCollection fonts = new PrivateFontCollection();
        private Font titleFont;

        // Button-Verschiebung in Pixel: 
        // Positiv verschiebt nach rechts bzw. nach unten,
        // negativ nach links bzw. nach oben.
        private const int BUTTON_OFFSET_X = -10;
        private const int BUTTON_OFFSET_Y = 460;

        public StartMenuForm()
        {
            InitializeForm();
            LoadAssets();
            PositionButton();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionButton();
            Invalidate();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            DoubleBuffered = true;
            KeyPreview = true;

            MouseMove += OnMouseMove;
            MouseClick += OnMouseClick;
            KeyDown += OnKeyDown;
            Resize += (s, e) => { PositionButton(); Invalidate(); };
        }

        private void LoadAssets()
        {
            string baseDir = Application.StartupPath;
            backgroundImage = (Bitmap)Image.FromFile(Path.Combine(baseDir, "assets", "fullscreen.png"));
            startButtonImage = (Bitmap)Image.FromFile(Path.Combine(baseDir, "assets", "start.png"));

            fonts.AddFontFile(Path.Combine(baseDir, "assets", "ZombieApocalypse8bit.ttf"));
            titleFont = new Font(fonts.Families[0], 60, FontStyle.Bold);
        }

        private void PositionButton()
        {
            var btnW = startButtonImage.Width;
            var btnH = startButtonImage.Height;

            // Center + Offset
            var centerX = (ClientSize.Width - btnW) / 2;
            var centerY = (ClientSize.Height - btnH) / 2;

            var x = centerX + BUTTON_OFFSET_X;
            var y = centerY + BUTTON_OFFSET_Y;

            startButtonBounds = new Rectangle(x, y, btnW, btnH);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            SetRenderingHints(g);


            g.DrawImage(backgroundImage, 0, 0, Width, Height);


            if (isHovering)
            {
                var glowRect = startButtonBounds;
                glowRect.Inflate(12, 12);
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(glowRect);
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(180, Color.Yellow);
                        brush.SurroundColors = new[] { Color.FromArgb(0, Color.Yellow) };
                        g.FillPath(brush, path);
                    }
                }
            }


            g.DrawImage(startButtonImage, startButtonBounds);
        }

        private void SetRenderingHints(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool nowHover = startButtonBounds.Contains(e.Location);
            if (nowHover != isHovering)
            {
                isHovering = nowHover;
                Invalidate(startButtonBounds);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && startButtonBounds.Contains(e.Location))
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
