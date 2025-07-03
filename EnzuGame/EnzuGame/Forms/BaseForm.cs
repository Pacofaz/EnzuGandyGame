using System;
using System.Drawing;
using System.Windows.Forms;
using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    /// <summary>
    /// Basis-Formular für alle Spiel-Forms.
    /// Kümmert sich um Registrierung für globale Effekte (z.B. Helligkeit, Musiksteuerung).
    /// </summary>
    public partial class BaseForm : Form
    {
        /// <summary>
        /// Erstellt das Basis-Formular und registriert Standard-Events.
        /// </summary>
        public BaseForm()
        {
            // Bei Laden und Schließen Standard-Aktionen ausführen
            this.Load += BaseForm_Load;
            this.FormClosed += BaseForm_FormClosed;
        }

        /// <summary>
        /// Wird beim Laden der Form ausgelöst.
        /// Registriert das Form bei GameSettings für Helligkeitseffekte,
        /// sofern es **keine** SettingsForm ist.
        /// </summary>
        private void BaseForm_Load(object? sender, EventArgs e)
        {
            // Keine doppelten Helligkeitseffekte auf SettingsForm
            if (!(this is SettingsForm))
                GameSettings.RegisterForm(this);

            // Musik ggf. sicherstellen
            EnsureMusicIsPlaying();
        }

        /// <summary>
        /// Wird beim Schließen der Form ausgelöst.
        /// Meldet das Form wieder bei GameSettings ab.
        /// </summary>
        private void BaseForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            GameSettings.UnregisterForm(this);
        }

        /// <summary>
        /// Stellt sicher, dass die Hintergrundmusik läuft.
        /// Wenn keine Musik läuft, startet sie den Standard-Soundtrack.
        /// </summary>
        protected void EnsureMusicIsPlaying()
        {
            try
            {
                if (!SoundManager.IsMusicPlaying())
                {
                    SoundManager.PlayBackgroundMusic("Resources/soundtrack.wav");
                    SoundManager.SetMusicVolume(GameSettings.MusicVolume / 100.0f);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Sicherstellen der Musikwiedergabe: {ex.Message}");
            }
        }
    }
}
