using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace ZombieGame
{
    /// <summary>
    /// Startmenü-Form für das ZombieGame.
    /// Bietet zwei große, eckige, rote Buttons (Settings und Start Game),
    /// die rechts unten untereinander angeordnet sind und auf Hover reagieren.
    /// Im Fenstermodus und im Vollbild immer responsive und düster gestylt.
    /// </summary>
    public class StartMenuForm : Form
    {
        // --- Assets und UI-State ---
        private Bitmap backgroundImage;
        private Rectangle startButtonBounds;
        private Rectangle settingsButtonBounds;
        private bool isHoverStart;
        private bool isHoverSettings;

        private PrivateFontCollection fonts = new PrivateFontCollection();
        private Font buttonFontLarge;
        private Font buttonFontSmall;

        // --- Button-Design ---
        private const int BUTTON_WIDTH = 400;
        private const int BUTTON_HEIGHT = 120;
        private const int BUTTON_GAP = 50;
        private readonly Color redDark = Color.FromArgb(220, 40, 10, 10);
        private readonly Color redHover = Color.FromArgb(220, 90, 20, 20);
        private readonly Color redBorder = Color.FromArgb(255, 160, 30, 30);

        /// <summary>
        /// Konstruktor: Initialisiert das Menü als Vollbild, lädt Assets und setzt Events.
        /// </summary>
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

        /// <summary>
        /// Nach dem Anzeigen werden die Buttons korrekt positioniert und neu gezeichnet.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            PositionButtons();
            Invalidate();
        }

        /// <summary>
        /// Lädt das Hintergrundbild und die Schriften aus den Assets.
        /// </summary>
        private void LoadAssets()
        {
            string baseDir = Application.StartupPath;
            backgroundImage = (Bitmap)Image.FromFile(Path.Combine(baseDir, "assets", "fullscreen.png"));
            fonts.AddFontFile(Path.Combine(baseDir, "assets", "ZombieApocalypse8bit.ttf"));
            buttonFontLarge = new Font(fonts.Families[0], 35, FontStyle.Bold); // Start Game
            buttonFontSmall = new Font(fonts.Families[0], 30, FontStyle.Bold); // Settings
        }

        /// <summary>
        /// Positioniert die Buttons rechts unten, untereinander und immer sichtbar,
        /// unabhängig von der Fenstergröße.
        /// </summary>
        private void PositionButtons()
        {
            int totalHeight = BUTTON_HEIGHT * 2 + BUTTON_GAP;
            int cx = ClientSize.Width - BUTTON_WIDTH - 80;
            int cy = ClientSize.Height - totalHeight - 100;

            settingsButtonBounds = new Rectangle(cx, cy, BUTTON_WIDTH, BUTTON_HEIGHT);
            startButtonBounds = new Rectangle(cx, cy + BUTTON_HEIGHT + BUTTON_GAP, BUTTON_WIDTH, BUTTON_HEIGHT);
        }

        /// <summary>
        /// Malt das Hintergrundbild und beide Buttons.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.DrawImage(backgroundImage, 0, 0, Width, Height);
            DrawButton(g, settingsButtonBounds, "SETTINGS", isHoverSettings, isSettings: true);
            DrawButton(g, startButtonBounds, "START GAME", isHoverStart, isSettings: false);
        }

        /// <summary>
        /// Zeichnet einen eckigen, roten Button mit Schatten, Rand und angepasster Schriftgröße.
        /// </summary>
        /// <param name="g">Grafikobjekt</param>
        /// <param name="rect">Button-Bereich</param>
        /// <param name="text">Button-Text</param>
        /// <param name="hover">Hover-State</param>
        /// <param name="isSettings">Ob der Button der Settings-Button ist (kleinere Schrift)</param>
        private void DrawButton(Graphics g, Rectangle rect, string text, bool hover, bool isSettings)
        {
            var shadowRect = new Rectangle(rect.X + 7, rect.Y + 7, rect.Width, rect.Height);
            using (var shadowB = new SolidBrush(Color.FromArgb(110, 10, 0, 0)))
                g.FillRectangle(shadowB, shadowRect);

            using (var fill = new SolidBrush(hover ? redHover : redDark))
                g.FillRectangle(fill, rect);

            using (var border = new Pen(redBorder, 6))
                g.DrawRectangle(border, rect);

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            Rectangle textRect = new Rectangle(rect.X, rect.Y + 2, rect.Width, rect.Height);

            Font font = isSettings ? buttonFontSmall : buttonFontLarge;

            using (var darkText = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.DrawString(text, font, darkText, new Rectangle(rect.X, rect.Y + 5, rect.Width, rect.Height), sf);

            using (var txtBrush = new SolidBrush(Color.White))
                g.DrawString(text, font, txtBrush, textRect, sf);
        }

        /// <summary>
        /// Aktualisiert den Hover-Status beider Buttons und sorgt für visuelles Feedback.
        /// </summary>
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

        /// <summary>
        /// Klick-Handler für beide Buttons: Startet das Spiel oder öffnet das Settings-Form.
        /// </summary>
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

        /// <summary>
        /// Schaltet zwischen Vollbild und großem Fenstermodus um,
        /// passt Fenstergröße und Position an und sorgt für korrekte Darstellung.
        /// </summary>
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
                var screen = Screen.FromControl(this).WorkingArea;
                int width = (int)(screen.Width * 0.8);
                int height = (int)(screen.Height * 0.8);
                MinimumSize = new Size(900, 600);

                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                TopMost = false;
                Size = new Size(Math.Max(width, MinimumSize.Width), Math.Max(height, MinimumSize.Height));
                Location = new Point(
                    screen.Left + (screen.Width - Width) / 2,
                    screen.Top + (screen.Height - Height) / 2
                );
            }
            PositionButtons();
            Invalidate();
        }

        // Unbenutzter Designer-Code
        private void InitializeComponent() { }
        private void StartMenuForm_Load(object sender, EventArgs e) { }
    }
}
