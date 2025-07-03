#nullable enable

using EnzuGame.Klassen;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace EnzuGame.Forms
{
    /// <summary>
    /// Hauptmenü des Spiels. Zeigt Buttons (Start, Einstellungen, Beenden) an,
    /// unterstützt Maus- und Tastatursteuerung.
    /// </summary>
    public partial class MainForm : BaseForm
    {
        // --- Konstanten für Ressourcenpfade ---
        private const string BgPath = "Resources/background.png";
        private const string OverlayPath = "Resources/overlay_board.png";
        private const string SoundtrackPath = "Resources/soundtrack.wav";

        /// <summary>
        /// Button-Configs: normal, hover, pressed, rect, key
        /// </summary>
        private readonly (string normal, string hover, string pressed, Rectangle rect, string key)[] buttonConfigs = {
            ("Resources/btn_start.png",    "Resources/btn_start_hover.png",    "Resources/btn_start_pressed.png",    new Rectangle(90,  60, 140, 30), "Start"),
            ("Resources/btn_settings.png", "Resources/btn_settings_hover.png", "Resources/btn_settings_pressed.png", new Rectangle(90,  90, 140, 30), "Settings"),
            ("Resources/btn_exit.png",     "Resources/btn_exit_hover.png",     "Resources/btn_exit_pressed.png",     new Rectangle(90, 120, 140, 30), "Exit"),
        };

        // --- Layout Overlay ---
        private readonly Size overlayOriginalSize = new(320, 200);
        private Rectangle overlayRect;
        private float scaleFactorX = 1f, scaleFactorY = 1f;

        // --- Ressourcen & Buttons ---
        private Image? backgroundImage;
        private Image? overlayImage;
        private readonly Dictionary<string, ImageButton> buttons = new();

        // --- Settings ---
        private Form? activeSettingsForm;

        // --- Tastatursteuerung ---
        private int selectedIndex = 0;
        private bool blockKeyRepeat = false; // Optional, falls du KeyRepeat verhindern willst

        public MainForm()
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(640, 480);
            KeyPreview = true;
            InitializeComponent();

            backgroundImage = TryLoadImage(BgPath);
            overlayImage = TryLoadImage(OverlayPath);

            CreateButtons();
            Resize += (_, _) => RepositionUI();
            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;
        }

        private Image? TryLoadImage(string path)
        {
            try
            {
                return File.Exists(path) ? Image.FromFile(path) : null;
            }
            catch { return null; }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            GameSettings.Initialize(this);
            SoundManager.PlayBackgroundMusic(SoundtrackPath);
            SoundManager.SetMusicVolume(GameSettings.MusicVolume / 100f);
            RepositionUI();
            UpdateButtonHighlight();
        }

        private void CreateButtons()
        {
            var clickActions = new Dictionary<string, EventHandler>
            {
                ["Start"] = BtnStart_Click,
                ["Settings"] = BtnSettings_Click,
                ["Exit"] = BtnExit_Click
            };

            foreach (var (normal, hover, pressed, rect, key) in buttonConfigs)
            {
                var btn = new ImageButton
                {
                    NormalImage = TryLoadImage(normal) ?? new Bitmap(rect.Width, rect.Height),
                    HoverImage = TryLoadImage(hover) ?? new Bitmap(rect.Width, rect.Height),
                    ClickedImage = TryLoadImage(pressed) ?? new Bitmap(rect.Width, rect.Height),
                    Size = rect.Size,
                    TabStop = false
                };
                btn.Click += clickActions[key];

                // --- Synchronisiere Tastatur/Maus: ---
                btn.MouseEnter += (s, e) =>
                {
                    int idx = Array.FindIndex(buttonConfigs, x => x.key == key);
                    if (idx != -1)
                    {
                        selectedIndex = idx;
                        UpdateButtonHighlight();
                    }
                };

                Controls.Add(btn);
                buttons[key] = btn;
            }
        }

        private void RepositionUI()
        {
            float targetWidth = GameSettings.Fullscreen ? ClientSize.Width * 0.4f : overlayOriginalSize.Width;
            float aspectRatio = (float)overlayOriginalSize.Height / overlayOriginalSize.Width;
            float targetHeight = targetWidth * aspectRatio;

            overlayRect = new Rectangle(
                (int)((ClientSize.Width - targetWidth) / 2),
                (int)((ClientSize.Height - targetHeight) / 2),
                (int)targetWidth, (int)targetHeight);

            scaleFactorX = overlayRect.Width / (float)overlayOriginalSize.Width;
            scaleFactorY = overlayRect.Height / (float)overlayOriginalSize.Height;

            foreach (var (_, _, _, relRect, key) in buttonConfigs)
            {
                if (buttons.TryGetValue(key, out var btn))
                {
                    btn.Size = new Size((int)(relRect.Width * scaleFactorX), (int)(relRect.Height * scaleFactorY));
                    btn.Location = new Point(
                        overlayRect.X + (int)(relRect.X * scaleFactorX),
                        overlayRect.Y + (int)(relRect.Y * scaleFactorY));
                }
            }
            UpdateButtonHighlight();
            Invalidate();
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            var buttonKeys = buttonConfigs.Select(cfg => cfg.key).ToArray();
            int btnCount = buttonKeys.Length;

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                selectedIndex = (selectedIndex - 1 + btnCount) % btnCount;
                UpdateButtonHighlight();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                selectedIndex = (selectedIndex + 1) % btnCount;
                UpdateButtonHighlight();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                var selectedKey = buttonKeys[selectedIndex];
                if (buttons.TryGetValue(selectedKey, out var btn))
                {
                    btn.PerformClick();
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Setzt optisches Highlight auf den aktuell selektierten Button (Tastaturfokus)
        /// </summary>
        private void UpdateButtonHighlight()
        {
            var buttonKeys = buttonConfigs.Select(cfg => cfg.key).ToArray();
            for (int i = 0; i < buttonKeys.Length; i++)
            {
                if (buttons.TryGetValue(buttonKeys[i], out var btn))
                {
                    btn.IsHovered = (i == selectedIndex);
                    btn.Invalidate();
                }
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            Hide();
            using (var levelSelect = new LevelSelectForm())
            {
                levelSelect.ShowDialog(this);
            }
            Show();
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            if (activeSettingsForm == null || activeSettingsForm.IsDisposed)
            {
                activeSettingsForm = new SettingsForm();
                activeSettingsForm.FormClosed += (_, _) => { activeSettingsForm = null; Invalidate(); };
                activeSettingsForm.ShowDialog(this);
            }
            else { activeSettingsForm.Activate(); }
        }

        private void BtnExit_Click(object? sender, EventArgs e) => Close();

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (backgroundImage != null) g.DrawImage(backgroundImage, ClientRectangle);
            if (overlayImage != null) g.DrawImage(overlayImage, overlayRect);
            base.OnPaint(e);
        }
    }
}
