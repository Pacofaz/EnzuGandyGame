using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ZombieGame
{
    public class StartMenuForm : Form
    {
        // Ressourcen
        private Bitmap backgroundImage;
        private List<Zombie> zombieList;
        private PrivateFontCollection fonts = new PrivateFontCollection();
        private Font titleFont, subtitleFont, buttonFont, smallFont;

        // Menü
        private string[] mainMenuItems = { "New Game", "Continue", "Settings", "Credits", "Exit" };
        private Rectangle[] mainMenuItemBounds;
        private int hoveredMainIndex = -1;
        private bool inSettings, inCredits;

        private Timer animationTimer;

        public StartMenuForm()
        {
            InitializeForm();
            LoadAssets();
            InitializeZombies();
            CalculateMenuLayout();
            SetupTimers();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            DoubleBuffered = true;
            KeyDown += StartMenuForm_KeyDown;
            MouseMove += StartMenuForm_MouseMove;
            MouseClick += StartMenuForm_MouseClick;
        }

        private void LoadAssets()
        {
            string baseDir = Application.StartupPath;
            backgroundImage = (Bitmap)Image.FromFile(System.IO.Path.Combine(baseDir, "assets", "bg_zombie_fullscreen.png"));
            fonts.AddFontFile(System.IO.Path.Combine(baseDir, "assets", "ZombieApocalypse8bit.ttf"));
            titleFont = new Font(fonts.Families[0], 50, FontStyle.Bold);
            subtitleFont = new Font(fonts.Families[0], 48, FontStyle.Regular);
            buttonFont = new Font(fonts.Families[0], 20, FontStyle.Regular);
            smallFont = new Font(fonts.Families[0], 24, FontStyle.Regular);
        }

        private void InitializeZombies()
        {
            zombieList = new List<Zombie>();
            var rnd = new Random();
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;
            for (int i = 0; i < 10; i++)
                zombieList.Add(new Zombie
                {
                    Position = new PointF(rnd.Next(w), rnd.Next(h / 2, h)),
                    Frame = 0,
                    FrameCount = 4,
                    FrameWidth = 64,
                    FrameHeight = 64
                });
        }

        private void CalculateMenuLayout()
        {
            int sw = Screen.PrimaryScreen.Bounds.Width;
            int sh = Screen.PrimaryScreen.Bounds.Height;
            int btnW = 300, btnH = 50, gap = 20;
            int startY = (sh / 2) - ((mainMenuItems.Length * (btnH + gap)) / 2);
            mainMenuItemBounds = new Rectangle[mainMenuItems.Length];
            for (int i = 0; i < mainMenuItems.Length; i++)
                mainMenuItemBounds[i] = new Rectangle((sw - btnW) / 2, startY + i * (btnH + gap), btnW, btnH);
        }

        private void SetupTimers()
        {
            animationTimer = new Timer { Interval = 200 };
            animationTimer.Tick += (s, e) => {
                // Animation der Zombies
                for (int i = 0; i < zombieList.Count; i++)
                {
                    var z = zombieList[i];
                    z.Frame = (z.Frame + 1) % z.FrameCount;
                    zombieList[i] = z;
                }
                Invalidate();
            };
            animationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            SetRenderingHints(g);
            g.DrawImage(backgroundImage, 0, 0, Width, Height);
            DrawTitle(g);
            if (inSettings) DrawSettingsMenu(g);
            else if (inCredits) DrawCreditsScreen(g);
            else DrawMainMenu(g);

        }

        private void SetRenderingHints(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;
        }

        private void DrawTitle(Graphics g)
        {
            const string title = "ZOMBIE APOCALYPSE";
            var ts = g.MeasureString(title, titleFont);
            float x = (Width - ts.Width) / 2, y = 50;
            g.DrawString(title, titleFont, Brushes.Black, x + 8, y + 8);
            using (var lg = new LinearGradientBrush(new PointF(x, y), new PointF(x + ts.Width, y), Color.Red, Color.DarkRed))
                g.DrawString(title, titleFont, lg, x, y);
        }

        private void DrawMainMenu(Graphics g)
        {
            for (int i = 0; i < mainMenuItems.Length; i++)
                DrawButton(g, mainMenuItemBounds[i], mainMenuItems[i], hoveredMainIndex == i);
        }

        private void DrawSettingsMenu(Graphics g)
        {
            const string subtitle = "Settings";
            var ss = g.MeasureString(subtitle, subtitleFont);
            float sx = (Width - ss.Width) / 2, sy = 100;
            g.DrawString(subtitle, subtitleFont, Brushes.White, sx, sy);
        }

        private void DrawCreditsScreen(Graphics g)
        {
            const string credits = "Developed by:\n- Team Alpha\n- Graphics by Beta Studios\n- Music by Gamma Beats";
            g.DrawString(credits, smallFont, Brushes.LightGray, 100, 200);
        }

        private void DrawButton(Graphics g, Rectangle r, string text, bool hover)
        {
            using (var path = new GraphicsPath())
            {
                path.AddRectangle(r);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = hover ? Color.DarkRed : Color.FromArgb(200, 50, 0, 0);
                    pgb.SurroundColors = new[] { Color.Black };
                    g.FillPath(pgb, path);
                }
            }
            using (var pen = new Pen(Color.White, 4))
                g.DrawRectangle(pen, r);
            var ts = g.MeasureString(text, buttonFont);
            float tx = (r.X + (r.Width - ts.Width) / 2), ty = (r.Y + (r.Height - ts.Height) / 2);
            g.DrawString(text, buttonFont, Brushes.Black, tx + 2, ty + 2);
            g.DrawString(text, buttonFont, hover ? Brushes.Yellow : Brushes.White, tx, ty);
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

        private void StartMenuForm_MouseMove(object s, MouseEventArgs e)
        {
            if (!inSettings && !inCredits)
            {
                int prev = hoveredMainIndex;
                hoveredMainIndex = Array.FindIndex(mainMenuItemBounds, r => r.Contains(e.Location));
                if (prev != hoveredMainIndex) Invalidate();
            }
        }

        private void StartMenuForm_MouseClick(object s, MouseEventArgs e)
        {
            if (!inSettings && !inCredits)
            {
                switch (hoveredMainIndex)
                {
                    case 0: DialogResult = DialogResult.OK; break;    // New Game
                    case 1: DialogResult = DialogResult.OK; break;    // Continue
                    case 2: inSettings = true; Invalidate(); break;
                    case 3: inCredits = true; Invalidate(); break;
                    case 4: DialogResult = DialogResult.Cancel; break;// Exit
                }
            }
            else { inSettings = inCredits = false; Invalidate(); }
        }

        private void StartMenuForm_KeyDown(object s, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (inSettings || inCredits) { inSettings = inCredits = false; Invalidate(); }
                else DialogResult = DialogResult.Cancel;
            }
        }

        private struct Zombie
        {
            public PointF Position; public int Frame, FrameCount, FrameWidth, FrameHeight;
        }
    }
}
