#nullable enable

using EnzuGame.Klassen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnzuGame.Forms
{
    public partial class MainForm : BaseForm
    {
        private const string BgPath = "Resources/background.png";
        private const string OverlayPath = "Resources/overlay_board.png";
        private const string SoundtrackPath = "Resources/soundtrack.wav";

        private readonly (string normal, string hover, string pressed, Rectangle rect, string key)[] buttonConfigs = {
            ("Resources/btn_start.png", "Resources/btn_start_hover.png", "Resources/btn_start_pressed.png", new Rectangle(90, 60, 140, 30), "Start"),
            ("Resources/btn_settings.png", "Resources/btn_settings_hover.png", "Resources/btn_settings_pressed.png", new Rectangle(90, 90, 140, 30), "Settings"),
            ("Resources/btn_exit.png", "Resources/btn_exit_hover.png", "Resources/btn_exit_pressed.png", new Rectangle(90, 120, 140, 30), "Exit"),
        };

        private readonly Size overlayOriginalSize = new(320, 200);
        private Rectangle overlayRect;
        private float scaleFactorX = 1f, scaleFactorY = 1f;

        private Image? backgroundImage;
        private Image? overlayImage;
        private readonly Dictionary<string, ImageButton> buttons = new();
        private Form? activeSettingsForm;
        private Form? activeLevelSelectForm;

        public MainForm()
        {
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(640, 480);
            InitializeComponent();

            backgroundImage = LoadImage(BgPath);
            overlayImage = LoadImage(OverlayPath);

            CreateButtons();
            Resize += (_, _) => RepositionUI();
            Load += MainForm_Load;
        }

        private Image LoadImage(string path)
            => File.Exists(path) ? Image.FromFile(path) : throw new FileNotFoundException($"Bild nicht gefunden: {path}");

        private void MainForm_Load(object? sender, EventArgs e)
        {
            GameSettings.Initialize(this);
            SoundManager.PlayBackgroundMusic(SoundtrackPath);
            SoundManager.SetMusicVolume(GameSettings.MusicVolume / 100f);
            RepositionUI();
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
                    NormalImage = LoadImage(normal),
                    HoverImage = LoadImage(hover),
                    ClickedImage = LoadImage(pressed),
                    Size = rect.Size
                };
                btn.Click += clickActions[key];
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
            Invalidate();
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (activeLevelSelectForm == null || activeLevelSelectForm.IsDisposed)
            {
                activeLevelSelectForm = new LevelSelectForm();
                activeLevelSelectForm.FormClosed += (_, _) => { activeLevelSelectForm = null; Show(); };
                Hide();
                activeLevelSelectForm.StartPosition = FormStartPosition.CenterScreen;
                activeLevelSelectForm.ShowDialog(this);
            }
            else { activeLevelSelectForm.Activate(); }
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
