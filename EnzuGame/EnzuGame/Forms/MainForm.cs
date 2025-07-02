#nullable enable

using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    /// <summary>
    /// Hauptmenü des Spiels. Zeigt Buttons (Start, Einstellungen, Beenden) an
    /// und kümmert sich um das Layout und die Events.
    /// </summary>
    public partial class MainForm : BaseForm
    {
        // --- Konstanten für Ressourcenpfade ---
        private const string BgPath = "Resources/background.png";
        private const string OverlayPath = "Resources/overlay_board.png";
        private const string SoundtrackPath = "Resources/soundtrack.wav";

        /// <summary>
        /// Definiert, wie die Buttons im Hauptmenü aussehen und platziert werden.
        /// </summary>
        private readonly (string normal, string hover, string pressed, Rectangle rect, string key)[] buttonConfigs = {
            ("Resources/btn_start.png",    "Resources/btn_start_hover.png",    "Resources/btn_start_pressed.png",    new Rectangle(90,  60, 140, 30), "Start"),
            ("Resources/btn_settings.png", "Resources/btn_settings_hover.png", "Resources/btn_settings_pressed.png", new Rectangle(90,  90, 140, 30), "Settings"),
            ("Resources/btn_exit.png",     "Resources/btn_exit_hover.png",     "Resources/btn_exit_pressed.png",     new Rectangle(90, 120, 140, 30), "Exit"),
        };

        // --- Layout-Parameter für das Overlay-Board ---
        private readonly Size overlayOriginalSize = new(320, 200);
        private Rectangle overlayRect;
        private float scaleFactorX = 1f, scaleFactorY = 1f;

        // --- Ressourcen und Buttons ---
        private Image? backgroundImage;
        private Image? overlayImage;
        private readonly Dictionary<string, ImageButton> buttons = new();

        // --- Verweise auf geöffnete Sub-Forms ---
        private Form? activeSettingsForm;

        /// <summary>
        /// Initialisiert das Hauptmenü und lädt Grafiken und Buttons.
        /// </summary>
        public MainForm()
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(640, 480);
            InitializeComponent();

            backgroundImage = TryLoadImage(BgPath);
            overlayImage = TryLoadImage(OverlayPath);

            CreateButtons();
            Resize += (_, _) => RepositionUI();
            Load += MainForm_Load;
        }

        /// <summary>
        /// Versucht, ein Bild von Platte zu laden. Gibt null zurück, wenn das Bild nicht gefunden wird.
        /// </summary>
        private Image? TryLoadImage(string path)
        {
            try
            {
                return File.Exists(path) ? Image.FromFile(path) : null;
            }
            catch
            {
                // Fehlerhandling für defekte oder fehlende Ressourcen
                return null;
            }
        }

        /// <summary>
        /// Wird beim Laden der Form ausgeführt. Initialisiert die Einstellungen, startet die Musik und positioniert das UI.
        /// </summary>
        private void MainForm_Load(object? sender, EventArgs e)
        {
            GameSettings.Initialize(this);
            SoundManager.PlayBackgroundMusic(SoundtrackPath);
            SoundManager.SetMusicVolume(GameSettings.MusicVolume / 100f);
            RepositionUI();
        }

        /// <summary>
        /// Erstellt die Hauptmenü-Buttons und registriert ihre Click-Events.
        /// </summary>
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
                    Size = rect.Size
                };
                btn.Click += clickActions[key];
                Controls.Add(btn);
                buttons[key] = btn;
            }
        }

        /// <summary>
        /// Berechnet die aktuelle Overlay- und Button-Positionen abhängig von der Fenstergröße.
        /// </summary>
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
            Invalidate();
        }

        /// <summary>
        /// Öffnet das Level-Auswahlfenster modal, blendet das Hauptmenü währenddessen aus.
        /// </summary>
        private void BtnStart_Click(object? sender, EventArgs e)
        {
            Hide();
            using (var levelSelect = new LevelSelectForm())
            {
                levelSelect.ShowDialog(this);
            }
            Show();
        }

        /// <summary>
        /// Öffnet das Einstellungsfenster modal. Nur eine Instanz gleichzeitig erlaubt.
        /// </summary>
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

        /// <summary>
        /// Beendet das Programm, wenn der Exit-Button gedrückt wird.
        /// </summary>
        private void BtnExit_Click(object? sender, EventArgs e) => Close();

        /// <summary>
        /// Zeichnet Hintergrund und Overlay neu.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (backgroundImage != null) g.DrawImage(backgroundImage, ClientRectangle);
            if (overlayImage != null) g.DrawImage(overlayImage, overlayRect);
            base.OnPaint(e);
        }
    }
}
