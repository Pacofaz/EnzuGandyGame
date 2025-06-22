using EnzuGame.Klassen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnzuGame.Forms
{
    public partial class SettingsForm : BaseForm
    {
        // --- Layout & UI-Konstanten ---
        private const int FormWidth = 480;
        private const int FormHeight = 350;
        private const int CornerRadius = 10;
        private const int SliderKnobSize = 18;
        private const int SliderStep = 5;
        private const int SliderWidth = 280;
        private const int SliderHeight = 20;
        private const int SliderStartX = 100;
        private const int SliderStartY = 100;
        private const int SliderSpacingY = 60;
        private const int ToggleBtnY = 50;
        private const int ToggleBtnWidth = 40;
        private const int ToggleBtnHeight = 30;
        private const int SaveCancelY = 280;
        private static readonly Size ActionBtnSize = new Size(120, 40);

        // --- Farben & Schriftarten ---
        private static readonly Color PrimaryColor = Color.FromArgb(255, 214, 170);
        private static readonly Color AccentColor = Color.FromArgb(255, 149, 64);
        private static readonly Color DarkColor = Color.FromArgb(110, 78, 45);
        private static readonly Color BackgroundColor = Color.FromArgb(40, 25, 15);

        private static readonly Font TitleFont = new Font("Segoe UI", 18, FontStyle.Bold);
        private static readonly Font RegularFont = new Font("Segoe UI", 12);

        // --- Einstellungen als Model ---
        private class SettingsSnapshot
        {
            public bool Fullscreen;
            public int Brightness;
            public int MusicVolume;
            public int SoundVolume;

            public SettingsSnapshot Clone()
            {
                return (SettingsSnapshot)this.MemberwiseClone();
            }
        }

        private SettingsSnapshot tempSettings;
        private SettingsSnapshot originalSettings;

        // --- Enums statt Indexe ---
        private enum SliderType { Brightness = 0, Music = 1, Sound = 2 }
        private const int SliderCount = 3;

        // --- UI-Elemente ---
        private readonly Rectangle[] sliderRects = new Rectangle[SliderCount];
        private readonly Point[] sliderKnobs = new Point[SliderCount];
        private Rectangle[] toggleBtnRects = new Rectangle[2];
        private Rectangle saveRect, cancelRect;

        private int? draggedSlider = null;

        private Image backgroundImage;

        // --- Konstruktor ---
        public SettingsForm()
        {
            InitializeComponent();

            // Model-Initialisierung
            tempSettings = new SettingsSnapshot
            {
                Fullscreen = GameSettings.Fullscreen,
                Brightness = Math.Max(1, GameSettings.Brightness),
                MusicVolume = GameSettings.MusicVolume,
                SoundVolume = GameSettings.SoundVolume
            };
            originalSettings = tempSettings.Clone();

            // Form-Setup
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(FormWidth, FormHeight);
            this.BackColor = BackgroundColor;
            this.KeyPreview = true;

            // Ressourcen
            backgroundImage = CreateGradientBackground(this.Width, this.Height);

            // UI
            InitUI();
            RegisterEvents();

            // Registrierung bei globalen Settings (z.B. für Updates)
            GameSettings.RegisterForm(this);

            // Fokus auf Form für direkte Key-Eingaben
            this.Shown += (s, e) => this.Focus();
        }

        private void InitUI()
        {
            // Slider
            for (int i = 0; i < SliderCount; i++)
            {
                sliderRects[i] = new Rectangle(
                    SliderStartX,
                    SliderStartY + i * SliderSpacingY,
                    SliderWidth,
                    SliderHeight
                );
                sliderKnobs[i] = SliderKnobPos(sliderRects[i], GetSliderValue((SliderType)i));
            }

            // Toggle-Buttons für Vollbild
            toggleBtnRects[0] = new Rectangle(SliderStartX, ToggleBtnY, ToggleBtnWidth, ToggleBtnHeight);           // Links "<"
            toggleBtnRects[1] = new Rectangle(SliderStartX + SliderWidth - ToggleBtnWidth, ToggleBtnY, ToggleBtnWidth, ToggleBtnHeight); // Rechts ">"

            // Save/Cancel
            saveRect = new Rectangle(120, SaveCancelY, ActionBtnSize.Width, ActionBtnSize.Height);
            cancelRect = new Rectangle(260, SaveCancelY, ActionBtnSize.Width, ActionBtnSize.Height);
        }

        private void RegisterEvents()
        {
            this.Paint += SettingsForm_Paint;
            this.MouseDown += SettingsForm_MouseDown;
            this.MouseMove += SettingsForm_MouseMove;
            this.MouseUp += SettingsForm_MouseUp;
            this.KeyDown += SettingsForm_KeyDown;
        }

        // --- Getter für Modelwerte je Slider ---
        private int GetSliderValue(SliderType type)
        {
            switch (type)
            {
                case SliderType.Brightness: return tempSettings.Brightness;
                case SliderType.Music: return tempSettings.MusicVolume;
                case SliderType.Sound: return tempSettings.SoundVolume;
                default: return 0;
            }
        }

        private void SetSliderValue(SliderType type, int value)
        {
            switch (type)
            {
                case SliderType.Brightness:
                    tempSettings.Brightness = Math.Max(1, value);
                    break;
                case SliderType.Music:
                    tempSettings.MusicVolume = value;
                    SoundManager.SetMusicVolume(tempSettings.MusicVolume / 100.0f);
                    break;
                case SliderType.Sound:
                    tempSettings.SoundVolume = value;
                    break;
            }
        }

        // --- Zeichenlogik ---
        private void SettingsForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (backgroundImage != null)
                g.DrawImage(backgroundImage, 0, 0);

            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("EINSTELLUNGEN", TitleFont, new SolidBrush(PrimaryColor), this.Width / 2, 15, sf);

            // Toggle-UI für Vollbild
            DrawRoundedRectangle(g, new Rectangle(140, 50, 200, 30), CornerRadius, DarkColor);
            DrawGlowingText(g, tempSettings.Fullscreen ? "Vollbild" : "Fenster", RegularFont, PrimaryColor, 240, 54, true);
            DrawButton(g, toggleBtnRects[0], "<");
            DrawButton(g, toggleBtnRects[1], ">");

            // Slider
            DrawSlider(g, "HELLIGKEIT", tempSettings.Brightness, sliderRects[(int)SliderType.Brightness], sliderKnobs[(int)SliderType.Brightness]);
            DrawSlider(g, "MUSIK LAUTSTÄRKE", tempSettings.MusicVolume, sliderRects[(int)SliderType.Music], sliderKnobs[(int)SliderType.Music]);
            DrawSlider(g, "SOUND LAUTSTÄRKE", tempSettings.SoundVolume, sliderRects[(int)SliderType.Sound], sliderKnobs[(int)SliderType.Sound]);

            // Action-Buttons
            DrawActionButton(g, saveRect, "SPEICHERN", AccentColor);
            DrawActionButton(g, cancelRect, "ABBRECHEN", DarkColor);

            // Overlay (Helligkeit)
            ApplyBrightnessOverlay(g);
        }

        // === Zeichenmethoden (Platzhalter: deinen Code hier einsetzen oder wie gehabt lassen) ===
        private void DrawButton(Graphics g, Rectangle rect, string text)
        {
            DrawRoundedRectangle(g, rect, CornerRadius, DarkColor);
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush textBrush = new SolidBrush(PrimaryColor))
            {
                g.DrawString(text, RegularFont, textBrush,
                    new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
            }
        }

        private void DrawSlider(Graphics g, string title, int value, Rectangle sliderRect, Point knobPos)
        {
            DrawGlowingText(g, title, RegularFont, PrimaryColor, sliderRect.X, sliderRect.Y - 25, false);
            DrawRoundedRectangle(g, sliderRect, CornerRadius, DarkColor);

            int width = knobPos.X - sliderRect.X;
            Rectangle fillRect = new Rectangle(
                sliderRect.X,
                sliderRect.Y,
                Math.Max(0, width),
                sliderRect.Height);

            if (fillRect.Width > 0 && fillRect.Height > 0)
            {
                using (GraphicsPath path = CreateRoundedRectPath(fillRect, Math.Min(CornerRadius, fillRect.Width / 2)))
                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    fillRect,
                    AccentColor,
                    Color.FromArgb(200, AccentColor),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(fillBrush, path);
                }
            }

            Rectangle knobRect = new Rectangle(
                knobPos.X - SliderKnobSize / 2,
                knobPos.Y - SliderKnobSize / 2,
                SliderKnobSize,
                SliderKnobSize);

            using (GraphicsPath knobPath = new GraphicsPath())
            {
                knobPath.AddEllipse(knobRect);
                using (PathGradientBrush pgb = new PathGradientBrush(knobPath))
                {
                    pgb.CenterColor = Color.White;
                    pgb.SurroundColors = new Color[] { PrimaryColor };
                    g.FillPath(pgb, knobPath);
                }
                using (Pen p = new Pen(DarkColor, 1))
                {
                    g.DrawEllipse(p, knobRect);
                }
            }

            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Far })
            using (Brush textBrush = new SolidBrush(PrimaryColor))
            {
                g.DrawString(value + "%", RegularFont, textBrush,
                    sliderRect.Right + 40, sliderRect.Y, sf);
            }
        }

        private void DrawActionButton(Graphics g, Rectangle rect, string text, Color color)
        {
            using (GraphicsPath path = CreateRoundedRectPath(rect, CornerRadius))
            using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                rect,
                Color.FromArgb(255, color),
                Color.FromArgb(200, color),
                LinearGradientMode.Vertical))
            {
                g.FillPath(fillBrush, path);
                using (Pen p = new Pen(Color.FromArgb(100, Color.White), 1))
                {
                    g.DrawPath(p, path);
                }
            }
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(text, RegularFont, textBrush,
                    new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
            }
        }

        private void DrawGlowingText(Graphics g, string text, Font font, Color color, float x, float y, bool centered)
        {
            using (StringFormat sf = new StringFormat())
            {
                if (centered)
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                }
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    g.DrawString(text, font, shadowBrush, x + 1, y + 1, sf);
                }
                using (Brush textBrush = new SolidBrush(color))
                {
                    g.DrawString(text, font, textBrush, x, y, sf);
                }
            }
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle rect, int radius, Color color)
        {
            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using (GraphicsPath path = CreateRoundedRectPath(rect, r))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.X, rect.Y, diameter, diameter);

            // Obere linke Ecke
            if (diameter > 0)
                path.AddArc(arcRect, 180, 90);
            else
                path.AddLine(rect.X, rect.Y, rect.X, rect.Y);

            // Obere rechte Ecke
            arcRect.X = rect.Right - diameter;
            if (diameter > 0)
                path.AddArc(arcRect, 270, 90);
            else
                path.AddLine(rect.Right, rect.Y, rect.Right, rect.Y);

            // Untere rechte Ecke
            arcRect.Y = rect.Bottom - diameter;
            if (diameter > 0)
                path.AddArc(arcRect, 0, 90);
            else
                path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            // Untere linke Ecke
            arcRect.X = rect.X;
            if (diameter > 0)
                path.AddArc(arcRect, 90, 90);
            else
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom);

            path.CloseFigure();
            return path;
        }

        private void ApplyBrightnessOverlay(Graphics g)
        {
            float brightnessAlpha = (100 - tempSettings.Brightness) / 200.0f;
            if (brightnessAlpha > 0f)
            {
                int alpha = Math.Max(0, Math.Min(255, (int)(brightnessAlpha * 255)));
                using (SolidBrush overlay = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
                {
                    g.FillRectangle(overlay, ClientRectangle);
                }
            }
        }

        private static Point SliderKnobPos(Rectangle rect, int percent)
        {
            if (rect.Width <= 0)
                return new Point(rect.X, rect.Y + rect.Height / 2);

            int x = rect.X + (int)(rect.Width * percent / 100.0f);
            x = Math.Max(rect.X, Math.Min(rect.Right, x));
            int y = rect.Y + rect.Height / 2;
            return new Point(x, y);
        }

        private Image CreateGradientBackground(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return new Bitmap(1, 1);

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, height),
                Color.FromArgb(60, 40, 20),
                Color.FromArgb(20, 10, 5)))
            {
                g.FillRectangle(brush, 0, 0, width, height);
                using (HatchBrush hatchBrush = new HatchBrush(
                    HatchStyle.LightDownwardDiagonal,
                    Color.FromArgb(20, Color.White),
                    Color.Transparent))
                {
                    g.FillRectangle(hatchBrush, 0, 0, width, height);
                }
            }
            return bmp;
        }

        // --- Input-Handling ---
        private void SettingsForm_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < SliderCount; i++)
            {
                Rectangle knobRect = new Rectangle(sliderKnobs[i].X - SliderKnobSize / 2, sliderKnobs[i].Y - SliderKnobSize / 2, SliderKnobSize, SliderKnobSize);
                if (knobRect.Contains(e.Location))
                {
                    draggedSlider = i;
                    break;
                }
            }
            if (!draggedSlider.HasValue)
            {
                for (int i = 0; i < SliderCount; i++)
                {
                    if (sliderRects[i].Contains(e.Location))
                    {
                        UpdateSliderValue((SliderType)i, e.X);
                        draggedSlider = i;
                        break;
                    }
                }
            }
        }

        private void SettingsForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedSlider.HasValue)
                UpdateSliderValue((SliderType)draggedSlider.Value, e.X);

            bool overInteractive = false;
            foreach (Point knob in sliderKnobs)
            {
                Rectangle knobRect = new Rectangle(knob.X - SliderKnobSize / 2, knob.Y - SliderKnobSize / 2, SliderKnobSize, SliderKnobSize);
                if (knobRect.Contains(e.Location))
                {
                    overInteractive = true;
                    break;
                }
            }
            foreach (Rectangle btn in toggleBtnRects)
            {
                if (btn.Contains(e.Location))
                {
                    overInteractive = true;
                    break;
                }
            }
            if (!overInteractive && (saveRect.Contains(e.Location) || cancelRect.Contains(e.Location)))
                overInteractive = true;

            this.Cursor = overInteractive ? Cursors.Hand : Cursors.Default;
        }

        private void SettingsForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (draggedSlider.HasValue)
                draggedSlider = null;

            // Toggle Button
            if (toggleBtnRects[0].Contains(e.Location) || toggleBtnRects[1].Contains(e.Location))
            {
                tempSettings.Fullscreen = !tempSettings.Fullscreen;
                Invalidate();
            }
            if (saveRect.Contains(e.Location))
            {
                SaveSettings();
            }
            if (cancelRect.Contains(e.Location))
            {
                CancelAndRestore();
            }
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                CancelAndRestore();
            else if (e.KeyCode == Keys.Enter)
                SaveSettings();
        }

        private void UpdateSliderValue(SliderType sliderType, int xPos)
        {
            int idx = (int)sliderType;
            Rectangle sliderRect = sliderRects[idx];
            int newX = Math.Max(sliderRect.X, Math.Min(sliderRect.Right, xPos));
            float rel = sliderRect.Width > 0 ? (newX - sliderRect.X) / (float)sliderRect.Width : 0f;
            int percentage = (int)Math.Round(rel * 100 / SliderStep) * SliderStep;
            percentage = Math.Max(0, Math.Min(100, percentage));
            sliderKnobs[idx] = SliderKnobPos(sliderRect, percentage);
            SetSliderValue(sliderType, percentage);
            Invalidate();
        }

        // --- Settings speichern/wiederherstellen ---
        private void SaveSettings()
        {
            GameSettings.Fullscreen = tempSettings.Fullscreen;
            GameSettings.Brightness = tempSettings.Brightness;
            GameSettings.MusicVolume = tempSettings.MusicVolume;
            GameSettings.SoundVolume = tempSettings.SoundVolume;
            GameSettings.ApplySettings();
            this.Close();
        }

        private void CancelAndRestore()
        {
            // Nur Sound-Einstellung zurück, alles andere wird ja nicht gespeichert
            SoundManager.SetMusicVolume(originalSettings.MusicVolume / 100.0f);
            this.Close();
        }

        // --- Ressourcen säubern ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Paint -= SettingsForm_Paint;
                this.MouseDown -= SettingsForm_MouseDown;
                this.MouseMove -= SettingsForm_MouseMove;
                this.MouseUp -= SettingsForm_MouseUp;
                this.KeyDown -= SettingsForm_KeyDown;
                backgroundImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
