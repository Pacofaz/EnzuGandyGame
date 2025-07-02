using EnzuGame.Klassen;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private const int ToggleGroupX = 140;
        private const int ToggleGroupY = ToggleBtnY;
        private const int ToggleGroupWidth = 200;
        private const int ToggleGroupHeight = 30;
        private const int ToggleTextX = 240;
        private const int ToggleTextY = 65;
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
            public bool Fullscreen { get; set; }
            public int Brightness { get; set; }
            public int MusicVolume { get; set; }
            public int SoundVolume { get; set; }

            public SettingsSnapshot Clone()
            {
                return (SettingsSnapshot)this.MemberwiseClone();
            }
        }

        private SettingsSnapshot tempSettings;
        private SettingsSnapshot originalSettings;

        private enum SliderType { Brightness = 0, Music = 1, Sound = 2 }
        private const int SliderCount = 3;

        // --- UI-Elemente ---
        private readonly Rectangle[] sliderRects = new Rectangle[SliderCount];
        private readonly Point[] sliderKnobs = new Point[SliderCount];
        private readonly Rectangle[] toggleBtnRects = new Rectangle[2];
        private Rectangle saveRect, cancelRect;

        private int? draggedSlider = null;

        private Image? backgroundImage = null;

        // --- Konstruktor ---
        public SettingsForm()
        {
            InitializeComponent();

            tempSettings = new SettingsSnapshot
            {
                Fullscreen = GameSettings.Fullscreen,
                Brightness = Math.Max(1, GameSettings.Brightness),
                MusicVolume = GameSettings.MusicVolume,
                SoundVolume = GameSettings.SoundVolume
            };
            originalSettings = tempSettings.Clone();

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(FormWidth, FormHeight);
            this.BackColor = BackgroundColor;
            this.KeyPreview = true;

            backgroundImage = CreateGradientBackground(this.Width, this.Height);

            InitUI();
            RegisterEvents();

            GameSettings.RegisterForm(this);

            this.Shown += (s, e) => this.Focus();
        }
        /// <summary>
        /// Initialisiert die UI-Elemente wie Slider, Toggle-Buttons und die Action-Buttons (Speichern/Abbrechen).
        /// Legt dabei die Positionen und Größen aller interaktiven Komponenten fest.
        /// </summary>

        private void InitUI()
        {
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

            toggleBtnRects[0] = new Rectangle(SliderStartX, ToggleBtnY, ToggleBtnWidth, ToggleBtnHeight);
            toggleBtnRects[1] = new Rectangle(SliderStartX + SliderWidth - ToggleBtnWidth, ToggleBtnY, ToggleBtnWidth, ToggleBtnHeight);

            saveRect = new Rectangle(120, SaveCancelY, ActionBtnSize.Width, ActionBtnSize.Height);
            cancelRect = new Rectangle(260, SaveCancelY, ActionBtnSize.Width, ActionBtnSize.Height);
        }
        /// <summary>
        /// Registriert alle relevanten Eventhandler für die Bedienung des Settings-Fensters.
        /// Dadurch reagieren Zeichenroutine, Maus- und Tastatureingaben korrekt.
        /// </summary>

        private void RegisterEvents()
        {
            this.Paint += SettingsForm_Paint;
            this.MouseDown += SettingsForm_MouseDown;
            this.MouseMove += SettingsForm_MouseMove;
            this.MouseUp += SettingsForm_MouseUp;
            this.KeyDown += SettingsForm_KeyDown;
        }
        /// <summary>
        /// Gibt den aktuellen Wert des angegebenen Sliders (Helligkeit, Musik, Sound) aus den temporären Einstellungen zurück.
        /// </summary>

        private int GetSliderValue(SliderType type)
        {
            return type switch
            {
                SliderType.Brightness => tempSettings.Brightness,
                SliderType.Music => tempSettings.MusicVolume,
                SliderType.Sound => tempSettings.SoundVolume,
                _ => 0,
            };
        }
        /// <summary>
        /// Setzt den Wert des angegebenen Sliders im temporären Einstellungen-Objekt.
        /// Führt bei Änderung der Musiklautstärke direkt eine Anpassung der Wiedergabelautstärke durch.
        /// </summary>

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
        /// <summary>
        /// Zeichnet das komplette User Interface des Settings-Fensters.
        /// Dazu gehören Hintergrund, Titel, alle Slider, Buttons und Overlays.
        /// Wird immer beim Aktualisieren des Fensters aufgerufen.
        /// </summary>

        private void SettingsForm_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (backgroundImage != null)
                g.DrawImage(backgroundImage, 0, 0);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("EINSTELLUNGEN", TitleFont, new SolidBrush(PrimaryColor), this.Width / 2, 15, sf);

            DrawRoundedRectangle(g, new Rectangle(ToggleGroupX, ToggleGroupY, ToggleGroupWidth, ToggleGroupHeight), CornerRadius, DarkColor);
            DrawGlowingText(g, tempSettings.Fullscreen ? "Vollbild" : "Fenster", RegularFont, PrimaryColor, ToggleTextX, ToggleTextY, true);
            DrawButton(g, toggleBtnRects[0], "<");
            DrawButton(g, toggleBtnRects[1], ">");

            DrawSlider(g, "HELLIGKEIT", tempSettings.Brightness, sliderRects[(int)SliderType.Brightness], sliderKnobs[(int)SliderType.Brightness]);
            DrawSlider(g, "MUSIK LAUTSTÄRKE", tempSettings.MusicVolume, sliderRects[(int)SliderType.Music], sliderKnobs[(int)SliderType.Music]);
            DrawSlider(g, "SOUND LAUTSTÄRKE", tempSettings.SoundVolume, sliderRects[(int)SliderType.Sound], sliderKnobs[(int)SliderType.Sound]);

            DrawActionButton(g, saveRect, "SPEICHERN", AccentColor);
            DrawActionButton(g, cancelRect, "ABBRECHEN", DarkColor);

            ApplyBrightnessOverlay(g);
        }
        /// <summary>
        /// Zeichnet einen einzelnen runden Button mit Text innerhalb des angegebenen Rechtecks.
        /// Wird für die Toggle-Buttons ("<", ">") im UI verwendet.
        /// </summary>

        private void DrawButton(Graphics g, Rectangle rect, string text)
        {
            DrawRoundedRectangle(g, rect, CornerRadius, DarkColor);
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var textBrush = new SolidBrush(PrimaryColor))
            {
                g.DrawString(text, RegularFont, textBrush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
            }
        }
        /// <summary>
        /// Zeichnet einen Slider mit Titel, farbigem Füllbalken, Knopf und Prozentwert.
        /// Wird für Helligkeit, Musiklautstärke und Soundlautstärke genutzt.
        /// </summary>

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

            using (var sf = new StringFormat { Alignment = StringAlignment.Far })
            using (var textBrush = new SolidBrush(PrimaryColor))
            {
                g.DrawString(value + "%", RegularFont, textBrush, sliderRect.Right + 40, sliderRect.Y, sf);
            }
        }
        /// <summary>
        /// Zeichnet einen großen, abgerundeten Button mit Farbverlauf und zentriertem Text.
        /// Wird für die "Speichern" und "Abbrechen" Aktionen verwendet.
        /// </summary>

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
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var textBrush = new SolidBrush(Color.White))
            {
                g.DrawString(text, RegularFont, textBrush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
            }
        }
        /// <summary>
        /// Zeichnet einen Text mit einem dezenten schwarzen Schatten, um einen leuchtenden (Glow-)Effekt zu erzeugen.
        /// Optional kann der Text zentriert werden.
        /// </summary>

        private void DrawGlowingText(Graphics g, string text, Font font, Color color, float x, float y, bool centered)
        {
            using (var sf = new StringFormat())
            {
                if (centered)
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                }
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    g.DrawString(text, font, shadowBrush, x + 1, y + 1, sf);
                }
                using (var textBrush = new SolidBrush(color))
                {
                    g.DrawString(text, font, textBrush, x, y, sf);
                }
            }
        }
        /// <summary>
        /// Zeichnet ein gefülltes Rechteck mit abgerundeten Ecken in der angegebenen Farbe.
        /// Der Radius wird automatisch an die Rechteckgröße angepasst.
        /// </summary>

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
        /// <summary>
        /// Erstellt und gibt einen GraphicsPath für ein Rechteck mit abgerundeten Ecken zurück.
        /// Der Radius bestimmt, wie stark die Ecken abgerundet werden.
        /// </summary>

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.X, rect.Y, diameter, diameter);

            if (diameter > 0)
                path.AddArc(arcRect, 180, 90);
            else
                path.AddLine(rect.X, rect.Y, rect.X, rect.Y);

            arcRect.X = rect.Right - diameter;
            if (diameter > 0)
                path.AddArc(arcRect, 270, 90);
            else
                path.AddLine(rect.Right, rect.Y, rect.Right, rect.Y);

            arcRect.Y = rect.Bottom - diameter;
            if (diameter > 0)
                path.AddArc(arcRect, 0, 90);
            else
                path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            arcRect.X = rect.X;
            if (diameter > 0)
                path.AddArc(arcRect, 90, 90);
            else
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom);

            path.CloseFigure();
            return path;
        }
        /// <summary>
        /// Überlagert das gesamte Fenster mit einer halbtransparenten, schwarzen Fläche –
        /// je nach Helligkeitseinstellung des Nutzers.
        /// Je dunkler die Einstellung, desto stärker der Overlay.
        /// </summary>

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
        /// <summary>
        /// Berechnet die genaue Position des Schiebereglers (Knopf) auf dem Slider anhand eines Prozentwertes.
        /// Gibt den Mittelpunkt des Knopfes zurück.
        /// </summary>

        private static Point SliderKnobPos(Rectangle rect, int percent)
        {
            if (rect.Width <= 0)
                return new Point(rect.X, rect.Y + rect.Height / 2);

            int x = rect.X + (int)(rect.Width * percent / 100.0f);
            x = Math.Max(rect.X, Math.Min(rect.Right, x));
            int y = rect.Y + rect.Height / 2;
            return new Point(x, y);
        }
        /// <summary>
        /// Erstellt einen Hintergrund mit vertikalem Farbverlauf und optionalem diagonalen Muster.
        /// Gibt das fertige Bild (Bitmap) zurück.
        /// </summary>

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
        /// <summary>
        /// Eventhandler für Mausklick: Prüft, ob auf einen Slider-Knopf oder die Slider-Leiste geklickt wurde.
        /// Setzt ggf. den aktiven (zu ziehenden) Slider.
        /// </summary>

        private void SettingsForm_MouseDown(object? sender, MouseEventArgs e)
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
        /// <summary>
        /// Eventhandler für Mausbewegung:  
        /// Aktualisiert ggf. den Sliderwert beim Ziehen und ändert den Cursor,
        /// wenn die Maus über interaktiven Elementen schwebt (Slider, Buttons).
        /// </summary>

        private void SettingsForm_MouseMove(object? sender, MouseEventArgs e)
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
        /// <summary>
        /// Eventhandler für das Loslassen der Maustaste.
        /// Beendet das Slider-Dragging und prüft, ob auf einen Button (Toggle, Speichern, Abbrechen) geklickt wurde.
        /// Löst entsprechende Aktionen aus.
        /// </summary>

        private void SettingsForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (draggedSlider.HasValue)
                draggedSlider = null;

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
        /// <summary>
        /// Eventhandler für Tastatureingaben.
        /// ESC schließt das Fenster und verwirft Änderungen, ENTER speichert die Einstellungen.
        /// </summary>

        private void SettingsForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                CancelAndRestore();
            else if (e.KeyCode == Keys.Enter)
                SaveSettings();
        }
        /// <summary>
        /// Berechnet und setzt den neuen Wert eines Sliders anhand der aktuellen Mausposition.
        /// Aktualisiert die Position des Knopfs, speichert den Wert und zeichnet das UI neu.
        /// </summary>

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
        /// <summary>
        /// Überträgt alle temporären Einstellungen in die globalen GameSettings.
        /// Wendet die Änderungen an und schließt das Settings-Fenster.
        /// </summary>

        private void SaveSettings()
        {
            GameSettings.Fullscreen = tempSettings.Fullscreen;
            GameSettings.Brightness = tempSettings.Brightness;
            GameSettings.MusicVolume = tempSettings.MusicVolume;
            GameSettings.SoundVolume = tempSettings.SoundVolume;
            GameSettings.ApplySettings();
            this.Close();
        }
        /// <summary>
        /// Stellt die ursprüngliche Musiklautstärke wieder her und schließt das Fenster, 
        /// ohne die restlichen temporären Änderungen zu speichern.
        /// </summary>

        private void CancelAndRestore()
        {
            SoundManager.SetMusicVolume(originalSettings.MusicVolume / 100.0f);
            this.Close();
        }
        /// <summary>
        /// Gibt alle verwendeten Ressourcen frei und deregistriert alle Eventhandler.
        /// Wird beim Schließen oder Entsorgen des Fensters automatisch aufgerufen.
        /// </summary>

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
