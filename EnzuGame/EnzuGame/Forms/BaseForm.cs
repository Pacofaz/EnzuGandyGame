using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    public partial class BaseForm : Form

    {
        public BaseForm()
        {
            // Event-Registrierung bei Erstellung
            this.Load += BaseForm_Load;
            this.FormClosed += BaseForm_FormClosed;
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            // Bei Form-Laden für Helligkeitseffekte registrieren
            // Nur registrieren, wenn es sich nicht um eine SettingsForm handelt
            if (!(this is SettingsForm))
            {
                GameSettings.RegisterForm(this);
            }

            // Stellen Sie sicher, dass die Musik weiterhin spielt
            EnsureMusicIsPlaying();
        }

        private void BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Form bei GameSettings abmelden
            GameSettings.UnregisterForm(this);
        }

        /// <summary>
        /// Stellt sicher, dass die Hintergrundmusik spielt
        /// </summary>
        protected void EnsureMusicIsPlaying()
        {
            try
            {
                // Prüfen ob Musik bereits spielt
                if (!SoundManager.IsMusicPlaying())
                {
                    // Standard-Soundtrack abspielen
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
