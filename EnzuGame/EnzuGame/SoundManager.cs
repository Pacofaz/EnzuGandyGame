using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EnzuGame
{
    public static class SoundManager
    {
        private static SoundPlayer musicPlayer;
        private static float musicVolume = 1.0f; // 0.0 bis 1.0
        private static string currentMusicPath;
        private static bool isMusicPlaying = false;

        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        static SoundManager()
        {
            try
            {
                musicPlayer = new SoundPlayer();
                Application.ApplicationExit += OnApplicationExit;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Initialisieren des Sound-Systems: {ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Musik im Loop (z.B. für MainMenu)
        public static void PlayBackgroundMusic(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;

                if (isMusicPlaying && currentMusicPath == filePath)
                    return;

                currentMusicPath = filePath;

                string fullPath = Path.GetFullPath(filePath);
                if (!File.Exists(filePath))
                {
                    SystemSounds.Asterisk.Play();
                    MessageBox.Show($"Soundtrack konnte nicht gefunden werden!\nGesucht wurde in: {fullPath}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                StopBackgroundMusic();

                musicPlayer.SoundLocation = filePath;
                musicPlayer.Load();
                musicPlayer.PlayLooping();
                isMusicPlaying = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WAV-Datei kann nicht abgespielt werden:\n{ex.Message}", "Audiofehler");
                isMusicPlaying = false;
            }
        }

        // Nur einmal abspielen (z.B. für Intro)
        public static void PlaySoundOnce(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;

                string fullPath = Path.GetFullPath(filePath);
                if (!File.Exists(filePath))
                {
                    SystemSounds.Asterisk.Play();
                    MessageBox.Show(
                        $"Soundtrack konnte nicht gefunden werden!\nGesucht wurde in: {fullPath}",
                        "Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                musicPlayer.Stop();
                musicPlayer.SoundLocation = filePath;
                musicPlayer.Load();
                musicPlayer.Play(); // <-- Nur einmal abspielen!
                isMusicPlaying = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WAV-Datei kann nicht abgespielt werden:\n{ex.Message}", "Audiofehler");
                isMusicPlaying = false;
            }
        }

        public static void StopBackgroundMusic()
        {
            try
            {
                if (musicPlayer != null)
                {
                    musicPlayer.Stop();
                    isMusicPlaying = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Stoppen der Musik: {ex.Message}");
            }
        }

        public static void SetMusicVolume(float volume)
        {
            musicVolume = Math.Max(0, Math.Min(1, volume));
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
                Console.WriteLine($"Fehler beim Ändern der Lautstärke: {ex.Message}");
            }
        }

        public static float GetMusicVolume()
        {
            return musicVolume;
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            StopBackgroundMusic();
        }

        public static bool IsMusicPlaying()
        {
            return isMusicPlaying;
        }
    }
}
