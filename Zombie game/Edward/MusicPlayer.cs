using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ZombieGame.Utils
{
    public class MusicPlayer : IDisposable
    {
        private const string Alias = "background";
        private bool _isOpen;

        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        // Hilfsmethode, um Fehlercodes auszulesen und im Debug-Fenster zu zeigen
        private void Exec(string cmd)
        {
            var buf = new StringBuilder(128);
            int err = mciSendString(cmd, buf, buf.Capacity, IntPtr.Zero);
            if (err != 0)
            {
                // Fehlermeldung aus MCI abrufen
                var errBuf = new StringBuilder(128);
                mciSendString($"error {err}", errBuf, errBuf.Capacity, IntPtr.Zero);
                Debug.WriteLine($"MCI Error {err}: {errBuf}");
            }
        }

        /// <summary>
        /// Lädt und startet die MP3. Wenn loop=true, wird in Endlosschleife gespielt.
        /// </summary>
        public void Play(string filePath, bool loop = true)
        {
            // Sicherstellen, dass kein Alias offen ist
            Stop();

            // Datei öffnen
            Exec($"open \"{filePath}\" type mpegvideo alias {Alias}");
            // Lautstärke auf 50 % setzen (Werte 0–1000, hier also 500)
            Exec($"setaudio {Alias} volume to 500");

            // Abspielen (mit optionalem Repeat)
            if (loop)
                Exec($"play {Alias} repeat");
            else
                Exec($"play {Alias}");

            _isOpen = true;
        }

        /// <summary>
        /// Stoppt und schließt den Player.
        /// </summary>
        public void Stop()
        {
            if (!_isOpen) return;
            Exec($"stop {Alias}");
            Exec($"close {Alias}");
            _isOpen = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
