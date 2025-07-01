using EnzuGame.Forms;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnzuGame.Klassen
{
    public static class GameSettings
    {
        // --- Konstanten ---
        private const string SettingsFilePath = "settings.xml";
        private const string MusicFilePath = "Resources/soundtrack.wav";
        private const int DefaultUnlockedLevel = 1;

        // --- Einstellungen ---
        public static bool Level2Unlocked { get; set; } = false;
        public static int UnlockedLevel { get; set; } = DefaultUnlockedLevel;
        public static bool Fullscreen { get; set; } = true;
        public static int Brightness { get; set; } = 80; // 0–100
        public static int MusicVolume { get; set; } = 60;
        public static int SoundVolume { get; set; } = 50;

        // --- Synchronisation für Thread-Sicherheit ---
        private static readonly object settingsLock = new object();

        // --- Forms- und Handler-Verwaltung ---
        private static List<Form> activeForms = new List<Form>();
        private static Dictionary<Form, PaintEventHandler> registeredPaintHandlers = new Dictionary<Form, PaintEventHandler>();

        // --- Registrierung des Hauptformulars für Vollbildverwaltung ---
        private static Form activeForm;
        private static FormWindowState previousWindowState;
        private static FormBorderStyle previousBorderStyle;
        private static Rectangle previousBounds;

        public static void Initialize(Form mainForm)
        {
            activeForm = mainForm;
            LoadSettings();

            // Brightness sicherstellen
            Brightness = Math.Max(1, Brightness);

            // Helligkeitseffekte aktivieren
            RegisterForm(mainForm);

            ApplySettings();

            // Hintergrundmusik starten (nur wenn sie noch nicht läuft)
            if (mainForm is EnzuGame.Forms.MainForm && !SoundManager.IsMusicPlaying())
            {
                SoundManager.PlayBackgroundMusic(MusicFilePath);
                SoundManager.SetMusicVolume(MusicVolume / 100.0f);
            }
        }

        public static void RegisterForm(Form form)
        {
            if (form == null) return;

            // Schon registriert?
            if (activeForms.Contains(form)) return;

            activeForms.Add(form);

            form.FormClosed -= FormClosed_Handler;
            form.FormClosed += FormClosed_Handler;

            // Handler verwalten
            if (registeredPaintHandlers.TryGetValue(form, out PaintEventHandler oldHandler))
                form.Paint -= oldHandler;

            PaintEventHandler newHandler = new PaintEventHandler(Form_Paint);
            registeredPaintHandlers[form] = newHandler;
            form.Paint += newHandler;

            if (form.IsHandleCreated && !form.IsDisposed)
                form.Invalidate();
        }

        private static void FormClosed_Handler(object sender, FormClosedEventArgs e)
        {
            if (sender is Form form)
                UnregisterForm(form);
        }

        public static void UnregisterForm(Form form)
        {
            if (form == null) return;

            form.FormClosed -= FormClosed_Handler;
            if (registeredPaintHandlers.TryGetValue(form, out PaintEventHandler handler))
            {
                try
                {
                    form.Paint -= handler;
                    registeredPaintHandlers.Remove(form);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Entfernen des Paint-Handlers: {ex.Message}");
                }
            }

            activeForms.Remove(form);
        }

        private static void Form_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Form form && !(form is EnzuGame.Forms.SettingsForm))
                ApplyBrightnessOverlay(e.Graphics, form.ClientRectangle);
        }

        /// <summary>
        /// Überlagert das Formular mit einem schwarzen, (teil-)transparenten Overlay je nach Brightness-Wert.
        /// 100 = kein Overlay, 0 = halbtransparentes Schwarz.
        /// </summary>
        public static void ApplyBrightnessOverlay(Graphics g, Rectangle rect)
        {
            // 0.0 (keine Abdunklung) bis 0.5 (halbtransparent)
            float brightnessAlpha = (100 - Brightness) / 200.0f;
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb((int)(brightnessAlpha * 255), Color.Black)))
                g.FillRectangle(overlay, rect);
        }

        public static void ApplySettings()
        {
            if (activeForm != null)
                ApplyFullscreenMode(Fullscreen);

            foreach (Form form in activeForms)
                if (form != null && form.IsHandleCreated && !form.IsDisposed)
                    form.Invalidate();

            // Musik nur starten, wenn sie noch nicht läuft!
            if (!SoundManager.IsMusicPlaying())
                SoundManager.PlayBackgroundMusic(MusicFilePath);

            SoundManager.SetMusicVolume(MusicVolume / 100.0f);

            SaveSettings();
        }

        public static void ToggleFullscreen()
        {
            Fullscreen = !Fullscreen;
            ApplyFullscreenMode(Fullscreen);
            SaveSettings();
        }

        private static void ApplyFullscreenMode(bool fullscreen)
        {
            if (activeForm == null) return;
            try
            {
                if (fullscreen)
                {
                    if (activeForm.FormBorderStyle != FormBorderStyle.None || activeForm.WindowState != FormWindowState.Maximized)
                    {
                        previousWindowState = activeForm.WindowState;
                        previousBorderStyle = activeForm.FormBorderStyle;
                        previousBounds = activeForm.Bounds;
                    }
                    activeForm.FormBorderStyle = FormBorderStyle.None;
                    activeForm.WindowState = FormWindowState.Normal;
                    activeForm.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (previousBorderStyle == FormBorderStyle.None)
                        previousBorderStyle = FormBorderStyle.Sizable;
                    activeForm.FormBorderStyle = previousBorderStyle;
                    activeForm.WindowState = FormWindowState.Normal;
                    if (previousBounds.Width > 0 && previousBounds.Height > 0)
                        activeForm.Bounds = previousBounds;
                    else
                        activeForm.ClientSize = new Size(640, 480);
                    if (previousWindowState != FormWindowState.Normal)
                        activeForm.WindowState = previousWindowState;
                }

                if (activeForm is EnzuGame.Forms.MainForm mainForm)
                {
                    var method = mainForm.GetType().GetMethod("RepositionUIElements", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method?.Invoke(mainForm, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Ändern des Vollbildmodus: {ex.Message}");
            }
        }

        public static string GetFullscreenText() => Fullscreen ? "Vollbild" : "Fenster";

        public static void SaveSettings()
        {
            lock (settingsLock)
            {
                try
                {
                    var settings = new SerializableSettings
                    {
                        Fullscreen = Fullscreen,
                        Brightness = Brightness,
                        MusicVolume = MusicVolume,
                        SoundVolume = SoundVolume,
                        UnlockedLevel = UnlockedLevel
                    };
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableSettings));
                    using (StreamWriter writer = new StreamWriter(SettingsFilePath))
                        serializer.Serialize(writer, settings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Speichern der Einstellungen: {ex.Message}");
                }
            }
        }

        public static void LoadSettings()
        {
            lock (settingsLock)
            {
                try
                {
                    if (!File.Exists(SettingsFilePath)) return;
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializableSettings));
                    using (StreamReader reader = new StreamReader(SettingsFilePath))
                    {
                        var settings = (SerializableSettings)serializer.Deserialize(reader);
                        if (settings == null)
                            throw new InvalidDataException("Deserialisierte Einstellungen sind null!");

                        Fullscreen = settings.Fullscreen;
                        Brightness = settings.Brightness;
                        MusicVolume = settings.MusicVolume;
                        SoundVolume = settings.SoundVolume;
                        UnlockedLevel = Math.Max(1, settings.UnlockedLevel);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Laden der Einstellungen: {ex.Message}");
                    // Datei löschen, falls sie defekt ist (optional)
                    try { File.Delete(SettingsFilePath); } catch { }
                }
            }
        }

        [Serializable]
        public class SerializableSettings
        {
            public bool Fullscreen { get; set; }
            public int Brightness { get; set; }
            public int MusicVolume { get; set; }
            public int SoundVolume { get; set; }
            public int UnlockedLevel { get; set; }
        }
    }
}
