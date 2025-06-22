using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EnzuGame
{
    public static class SoundManager
    {
        private static SoundPlayer musicPlayer;
        private static float musicVolume = 1.0f; // 0.0 bis 1.0
        private static string currentMusicPath;
        private static bool isMusicPlaying = false;

        // API für erweiterte Sound-Funktionalität
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

        public static void PlayBackgroundMusic(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;

                // Wenn bereits dieselbe Musik spielt, nicht neu starten
                if (isMusicPlaying && currentMusicPath == filePath)
                    return;

                currentMusicPath = filePath;

                // Vollständigen Pfad ermitteln und ausgeben
                string fullPath = Path.GetFullPath(filePath);
                Console.WriteLine($"Suche Musikdatei: {fullPath}");

                // Prüfen ob die Datei existiert
                if (!File.Exists(filePath))
                {
                    // Fehlermeldung in Console
                    Console.WriteLine($"Warnung: Musikdatei wurde nicht gefunden: {filePath}");

                    // Versuche alternative Pfade
                    string[] possiblePaths = new string[] {
                        Path.Combine("Resources", Path.GetFileName(filePath)),
                        Path.Combine(Application.StartupPath, "Resources", Path.GetFileName(filePath)),
                        Path.Combine(Environment.CurrentDirectory, "Resources", Path.GetFileName(filePath)),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", Path.GetFileName(filePath))
                    };

                    // Versuche jeden möglichen Pfad
                    foreach (string altPath in possiblePaths)
                    {
                        Console.WriteLine($"Versuche alternativen Pfad: {altPath}");
                        if (File.Exists(altPath))
                        {
                            filePath = altPath;
                            currentMusicPath = filePath;
                            Console.WriteLine($"Alternative Musikdatei gefunden: {filePath}");
                            break;
                        }
                    }

                    // Wenn keine Datei gefunden wurde
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("Keine Musikdatei gefunden in den Pfaden:");
                        foreach (string dir in Directory.GetDirectories(Environment.CurrentDirectory, "*", SearchOption.AllDirectories))
                        {
                            try { Console.WriteLine($"  - {dir}"); } catch { }
                        }

                        // Wenn keine Datei gefunden wurde, Systemsound abspielen
                        SystemSounds.Asterisk.Play();
                        MessageBox.Show($"Soundtrack konnte nicht gefunden werden!\nGesucht wurde in: {fullPath}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Musik anhalten und neu starten
                StopBackgroundMusic();

                // Musikdatei laden und abspielen
                musicPlayer.SoundLocation = filePath;
                musicPlayer.LoadCompleted += (s, e) => {
                    // Status aktualisieren wenn Musik geladen ist
                    isMusicPlaying = true;
                    Console.WriteLine($"Musik wird abgespielt: {filePath}");
                };
                musicPlayer.LoadAsync(); // Laden im Hintergrund
                musicPlayer.PlayLooping();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abspielen der Musik: {ex.Message}");
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
            // Wert zwischen 0 und 1 begrenzen
            musicVolume = Math.Max(0, Math.Min(1, volume));

            try
            {
                // Versuche die Systemlautstärke direkt anzupassen
                // Diese Methode ist nicht perfekt, beeinflusst aber die Gesamtlautstärke
                uint newVolume = (uint)(musicVolume * 0xFFFF);
                uint stereoVolume = (newVolume & 0xFFFF) | (newVolume << 16);
                waveOutSetVolume(IntPtr.Zero, stereoVolume);

                // Bei Lautstärke nahe 0 pausieren
                if (musicVolume <= 0.01f)
                {
                    StopBackgroundMusic();
                }
                else if (!string.IsNullOrEmpty(currentMusicPath) && File.Exists(currentMusicPath) && !isMusicPlaying)
                {
                    // Musik wieder starten wenn sie gestoppt wurde
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

        // Methode zum Aufräumen beim Beenden der Anwendung
        private static void OnApplicationExit(object sender, EventArgs e)
        {
            // Musik anhalten
            StopBackgroundMusic();
        }

        // Überprüft, ob die Musik aktuell spielt
        public static bool IsMusicPlaying()
        {
            return isMusicPlaying;
        }
    }
}
