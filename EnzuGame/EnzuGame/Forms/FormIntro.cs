#nullable enable

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EnzuGame.Forms
{
    /// <summary>
    /// Intro-Formular, das ein Video im Vollbild abspielt und bei Ende automatisch schließt.
    /// Optional wird ein Overlay angezeigt.
    /// </summary>
    public partial class FormIntro : Form
    {
        private readonly System.Windows.Forms.Timer introTimer = new System.Windows.Forms.Timer();
        private SkipOverlayForm? overlayForm; // Overlay ist optional, daher nullable
        private readonly string skipText = ""; // Kein Text erforderlich

        public FormIntro()
        {
            InitializeComponent();

            // Setze die Form auf den Bildschirm, auf dem die Maus ist
            var screen = Screen.FromPoint(Cursor.Position);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = screen.Bounds.Location;
            this.Size = screen.Bounds.Size;
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            // Musik fürs Intro
            SoundManager.SetMusicVolume(0.2f);

            if (!SoundManager.IsMusicPlaying())
            {
                string musicPath = Path.Combine(Application.StartupPath, "Resources", "intro.wav");
                SoundManager.PlaySoundOnce(musicPath);
            }

            string videoPath = Path.Combine(Application.StartupPath, "Resources", "intro.mp4");
            if (!File.Exists(videoPath))
            {
                MessageBox.Show("Intro-Video nicht gefunden:\n" + videoPath);
                CloseIntro();
                return;
            }

            // Video abspielen (MediaPlayer-Control muss im Designer platziert sein!)
            axWindowsMediaPlayer1.URL = videoPath;
            axWindowsMediaPlayer1.uiMode = "none";
            axWindowsMediaPlayer1.stretchToFit = true;
            axWindowsMediaPlayer1.Dock = DockStyle.Fill;
            axWindowsMediaPlayer1.Ctlcontrols.play();

            // Intro wird nach 6 Sekunden automatisch geschlossen
            introTimer.Interval = 6000;
            introTimer.Tick += (s, e) =>
            {
                introTimer.Stop();
                CloseIntro();
            };
            introTimer.Start();

            // Overlay anzeigen, wenn das Form geladen ist
            this.Load += (s, e) =>
            {
                overlayForm = new SkipOverlayForm(this, skipText);
                overlayForm.Show();
            };
            this.FormClosed += (s, e) =>
            {
                overlayForm?.Close();
            };
            this.Move += (s, e) => SyncOverlayPosition();
            this.Resize += (s, e) => SyncOverlayPosition();
        }

        /// <summary>
        /// Beendet das Intro und stellt die Lautstärke wieder her.
        /// </summary>
        private void CloseIntro()
        {
            try { axWindowsMediaPlayer1.Ctlcontrols.stop(); } catch { }
            SoundManager.SetMusicVolume(1.0f);

            overlayForm?.Close();
            this.Close();
        }

        /// <summary>
        /// Hält das Overlay immer synchron mit der Intro-Form.
        /// </summary>
        private void SyncOverlayPosition()
        {
            if (overlayForm != null && !overlayForm.IsDisposed)
            {
                overlayForm.Size = this.Size;
                overlayForm.Location = this.Location;
                overlayForm.Invalidate();
            }
        }

        /// <summary>
        /// Overlay-Fenster, das optional Text anzeigt.
        /// </summary>
        private class SkipOverlayForm : Form
        {
            private readonly string overlayText;

            public SkipOverlayForm(Form parent, string text)
            {
                this.overlayText = text;
                var screen = Screen.FromControl(parent);
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.BackColor = Color.Magenta;
                this.TransparencyKey = Color.Magenta;
                this.StartPosition = FormStartPosition.Manual;
                this.Size = screen.Bounds.Size;
                this.Location = screen.Bounds.Location;
                this.Owner = parent;
                this.DoubleBuffered = true;
                this.Paint += SkipOverlayForm_Paint;
            }

            /// <summary>
            /// Zeichnet ggf. den Overlay-Text.
            /// </summary>
            private void SkipOverlayForm_Paint(object? sender, PaintEventArgs e)
            {
                if (string.IsNullOrEmpty(overlayText)) return;
                using (var font = new Font("Segoe UI", 22, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    SizeF textSize = e.Graphics.MeasureString(overlayText, font);
                    float x = (this.ClientSize.Width - textSize.Width) / 2f;
                    float y = this.ClientSize.Height - textSize.Height - 30;

                    // Schatten
                    for (int dx = -2; dx <= 2; dx++)
                        for (int dy = -2; dy <= 2; dy++)
                            if (dx != 0 || dy != 0)
                                e.Graphics.DrawString(overlayText, font, Brushes.Black, x + dx, y + dy);

                    // Weißer Text
                    e.Graphics.DrawString(overlayText, font, Brushes.White, x, y);
                }
            }
        }
    }
}
