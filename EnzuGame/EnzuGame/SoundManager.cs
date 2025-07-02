using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EnzuGame
{
    /// <summary>
    /// Verwaltet die Musikwiedergabe im gesamten Spiel.
    /// Stellt Methoden zum Starten, Stoppen, Abspielen und zur Lautstärkeanpassung bereit.
    /// Arbeitet WAV-basiert und unterstützt Endlos-Wiedergabe und One-Shot-Sounds.
    /// </summary>
    public static class SoundManager
    {
        private static readonly SoundPlayer musicPlayer = new SoundPlayer();
        private static float musicVolume = 1.0f; // Bereich: 0.0 - 1.0
        private static string currentMusicPath = string.Empty;
        private static bool isMusicPlaying = false;

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        // Initialisiert statische Ressourcen und registriert das Exit-Event
        static SoundManager()
        {
            Application.ApplicationExit += OnApplicationExit;
        }

        /// <summary>
        /// Startet ein Musikstück als Endlos-Loop.
        /// </summary>
        /// <param name="filePath">Pfad zur WAV-Datei</param>
        public static void PlayBackgroundMusic(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string fullPath = Path.GetFullPath(filePath);

            if (isMusicPlaying && string.Equals(currentMusicPath, fullPath, StringComparison.OrdinalIgnoreCase))
                return;

            if (!File.Exists(fullPath))
            {
                LogWarning($"Musikdatei nicht gefunden: {fullPath}");
                return;
            }

            StopBackgroundMusic();

            try
            {
                musicPlayer.SoundLocation = fullPath;
                musicPlayer.Load();
                musicPlayer.PlayLooping();
                isMusicPlaying = true;
                currentMusicPath = fullPath;
            }
            catch (Exception ex)
            {
                LogError($"WAV-Datei kann nicht abgespielt werden: {ex.Message}");
                isMusicPlaying = false;
            }
        }

        /// <summary>
        /// Spielt einen Soundtrack nur einmal ab (z.B. für Intros).
        /// </summary>
        /// <param name="filePath">Pfad zur WAV-Datei</param>
        public static void PlaySoundOnce(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string fullPath = Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                LogWarning($"Sounddatei nicht gefunden: {fullPath}");
                return;
            }

            try
            {
                musicPlayer.Stop();
                musicPlayer.SoundLocation = fullPath;
                musicPlayer.Load();
                musicPlayer.Play(); // Nur einmal!
                isMusicPlaying = true;
                currentMusicPath = fullPath;
            }
            catch (Exception ex)
            {
                LogError($"WAV-Datei kann nicht abgespielt werden: {ex.Message}");
                isMusicPlaying = false;
            }
        }

        /// <summary>
        /// Stoppt die aktuell laufende Musik.
        /// </summary>
        public static void StopBackgroundMusic()
        {
            try
            {
                musicPlayer?.Stop();
            }
            catch (Exception ex)
            {
                LogError($"Fehler beim Stoppen der Musik: {ex.Message}");
            }
            finally
            {
                isMusicPlaying = false;
            }
        }

        /// <summary>
        /// Setzt die globale Musiklautstärke. (0.0 = stumm, 1.0 = max)
        /// Bei 0 wird die Musik gestoppt.
        /// </summary>
        /// <param name="volume">Lautstärke (0.0 - 1.0)</param>
        public static void SetMusicVolume(float volume)
        {
            musicVolume = Math.Max(0.0f, Math.Min(1.0f, volume));
            try
            {
                uint newVolume = (uint)(musicVolume * 0xFFFF);
                uint stereoVolume = (newVolume & 0xFFFF) | (newVolume << 16);
                waveOutSetVolume(IntPtr.Zero, stereoVolume);

                if (musicVolume <= 0.01f)
                {
                    StopBackgroundMusic();
                }
                else if (!string.IsNullOrEmpty(currentMusicPath) && File.Exists(currentMusicPath) && !isMusicPlaying)
                {
                    PlayBackgroundMusic(currentMusicPath);
                }
            }
            catch (Exception ex)
            {
                LogError($"Fehler beim Ändern der Lautstärke: {ex.Message}");
            }
        }

        /// <summary>
        /// Gibt die aktuelle Musiklautstärke zurück.
        /// </summary>
        public static float GetMusicVolume() => musicVolume;

        /// <summary>
        /// Gibt zurück, ob aktuell Musik abgespielt wird.
        /// </summary>
        public static bool IsMusicPlaying() => isMusicPlaying;

        /// <summary>
        /// Wird beim Schließen der Anwendung aufgerufen. Stoppt die Musik.
        /// </summary>
        private static void OnApplicationExit(object sender, EventArgs e)
        {
            StopBackgroundMusic();
        }

        /// <summary>
        /// Schreibt eine Warnung in die Konsole und gibt optional einen System-Sound aus.
        /// </summary>
        private static void LogWarning(string message)
        {
            SystemSounds.Asterisk.Play();
            Console.WriteLine("[SoundManager-Warnung] " + message);
        }

        /// <summary>
        /// Schreibt einen Fehler in die Konsole und gibt optional einen System-Sound aus.
        /// </summary>
        private static void LogError(string message)
        {
            SystemSounds.Hand.Play();
            Console.WriteLine("[SoundManager-Fehler] " + message);
        }
    }
}
