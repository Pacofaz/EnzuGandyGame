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
        private Rectangle startButtonBounds;
        private Rectangle settingsButtonBounds;
        private bool isHoverStart;
        private bool isHoverSettings;

        private PrivateFontCollection fonts = new PrivateFontCollection();
        private Font buttonFont;

        private const int BUTTON_WIDTH = 240;
        private const int BUTTON_HEIGHT = 72;
        private const int BUTTON_OFFSET_Y = 460;
        private const int BUTTON_GAP = 24;
        private const int BUTTON_RADIUS = 16;
        private readonly Color goldTop = Color.FromArgb(255, 224, 180, 20);
        private readonly Color goldBottom = Color.FromArgb(255, 160, 120, 0);

        public StartMenuForm()
        {
            DoubleBuffered = true;
            KeyPreview = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;

            MouseMove += (s, e) => UpdateHover(e.Location);
            MouseClick += OnMouseClick;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel; };
            Resize += (s, e) => { PositionButtons(); Invalidate(); };

            LoadAssets();
            PositionButtons();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionButtons();
            Invalidate();
        }

        private void LoadAssets()
        {
            string baseDir = Application.StartupPath;
            backgroundImage = (Bitmap)Image.FromFile(Path.Combine(baseDir, "assets", "fullscreen.png"));

            fonts.AddFontFile(Path.Combine(baseDir, "assets", "ZombieApocalypse8bit.ttf"));
            buttonFont = new Font(fonts.Families[0], 28, FontStyle.Bold);
        }

        private void PositionButtons()
        {
            int cx = (ClientSize.Width - BUTTON_WIDTH) / 2;
            int cy = (ClientSize.Height - BUTTON_HEIGHT) / 2 + BUTTON_OFFSET_Y;

            startButtonBounds = new Rectangle(cx, cy, BUTTON_WIDTH, BUTTON_HEIGHT);
            settingsButtonBounds = new Rectangle(cx - BUTTON_WIDTH - BUTTON_GAP, cy, BUTTON_WIDTH, BUTTON_HEIGHT);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Hintergrund
            g.DrawImage(backgroundImage, 0, 0, Width, Height);

            // Buttons
            DrawButton(g, settingsButtonBounds, "SETTINGS", isHoverSettings);
            DrawButton(g, startButtonBounds, "START GAME", isHoverStart);
        }

        private void DrawButton(Graphics g, Rectangle rect, string text, bool hover)
        {
            // 1) Schatten
            var shadowRect = new Rectangle(rect.X + 4, rect.Y + 4, rect.Width, rect.Height);
            using (var shadowB = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                g.FillPath(shadowB, RoundedRect(shadowRect, BUTTON_RADIUS));

            // 2) Goldener Verlauf + Kontur
            using (var path = RoundedRect(rect, BUTTON_RADIUS))
            using (var fill = new LinearGradientBrush(rect, goldTop, goldBottom, LinearGradientMode.Vertical))
            {
                if (hover)
                    fill.SetSigmaBellShape(0.6f);

                g.FillPath(fill, path);
                using (var darkPen = new Pen(Color.FromArgb(200, 50, 30, 0), 4) { LineJoin = LineJoin.Round })
                    g.DrawPath(darkPen, path);
                using (var lightPen = new Pen(Color.FromArgb(200, 255, 240, 180), 2) { LineJoin = LineJoin.Round })
                    g.DrawPath(lightPen, path);
            }

            // 3) Text mit Schatten und weißer Schrift
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            // Text Schatten
            var ts = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
            using (var tb = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                g.DrawString(text, buttonFont, tb, ts, sf);

            // Weißer Text
            g.DrawString(text, buttonFont, Brushes.White, rect, sf);
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void UpdateHover(Point mouse)
        {
            bool overStart = startButtonBounds.Contains(mouse);
            bool overSettings = settingsButtonBounds.Contains(mouse);
            if (overStart != isHoverStart || overSettings != isHoverSettings)
            {
                isHoverStart = overStart;
                isHoverSettings = overSettings;
                Invalidate(startButtonBounds);
                Invalidate(settingsButtonBounds);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (startButtonBounds.Contains(e.Location))
            {
                DialogResult = DialogResult.OK;
            }
            else if (settingsButtonBounds.Contains(e.Location))
            {
                using (var settings = new SettingsForm())
                {
                    settings.FullscreenEnabled =
                        (WindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None);
                    if (settings.ShowDialog(this) == DialogResult.OK)
                        ApplySettings(settings.FullscreenEnabled);
                }
            }
        }

        private void ApplySettings(bool fullscreen)
        {
            if (fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                TopMost = false;
            }
            PositionButtons();
            Invalidate();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // StartMenuForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "StartMenuForm";
            this.Load += new System.EventHandler(this.StartMenuForm_Load);
            this.ResumeLayout(false);

        }

        private void StartMenuForm_Load(object sender, EventArgs e)
        {

        }
    }
}
