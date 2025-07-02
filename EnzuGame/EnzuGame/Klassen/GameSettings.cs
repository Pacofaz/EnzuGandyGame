using EnzuGame.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EnzuGame.Klassen
{
    /// <summary>
    /// Verwaltet globale Spieleinstellungen, speichert/liest sie in XML und
    /// synchronisiert sie mit UI-Formularen und dem SoundManager.
    /// </summary>
    public static class GameSettings
    {
        // --- Konfigurationswerte & Defaults ---
        private const string SettingsFilePath = "settings.xml";
        private const string MusicFilePath = "Resources/soundtrack.wav";
        private const int DefaultUnlockedLevel = 1;

        // --- Persistente Einstellungen (Property-Style für Serialization) ---
        public static bool Level2Unlocked { get; set; } = false;
        public static bool Level3Unlocked { get; set; } = false;
        public static bool Level4Unlocked { get; set; } = false;

        public static int UnlockedLevel { get; set; } = DefaultUnlockedLevel;
        public static bool Fullscreen { get; set; } = true;
        public static int Brightness { get; set; } = 80; // 1–100
        public static int MusicVolume { get; set; } = 60; // 0–100
        public static int SoundVolume { get; set; } = 50; // 0–100

        // --- Synchronisation & UI-State ---
        private static readonly object settingsLock = new object();
        private static readonly List<Form> activeForms = new List<Form>();
        private static readonly Dictionary<Form, PaintEventHandler> registeredPaintHandlers = new();

        // --- Für Vollbildumschaltung (Restore nach Windowed) ---
        private static Form? activeForm;
        private static FormWindowState previousWindowState;
        private static FormBorderStyle previousBorderStyle;
        private static Rectangle previousBounds;

        /// <summary>
        /// Initialisiert das Setting-System und lädt (optional) Einstellungen aus Datei.
        /// </summary>
        public static void Initialize(Form mainForm)
        {
            activeForm = mainForm;
            LoadSettings();
            Brightness = Math.Max(1, Brightness); // Nie zu dunkel

            RegisterForm(mainForm);
            ApplySettings();

            // Hintergrundmusik starten (nur MainForm)
            if (mainForm is MainForm && !SoundManager.IsMusicPlaying())
            {
                SoundManager.PlayBackgroundMusic(MusicFilePath);
                SoundManager.SetMusicVolume(MusicVolume / 100.0f);
            }
        }

        /// <summary>
        /// Registriert ein Form für Brightness-Overlay und Auto-Update bei Setting-Änderungen.
        /// </summary>
        public static void RegisterForm(Form form)
        {
            if (form == null || activeForms.Contains(form))
                return;

            activeForms.Add(form);
            form.FormClosed -= FormClosed_Handler;
            form.FormClosed += FormClosed_Handler;

            // Paint-Handler für Brightness
            if (registeredPaintHandlers.TryGetValue(form, out var oldHandler))
                form.Paint -= oldHandler;

            PaintEventHandler newHandler = Form_Paint;
            registeredPaintHandlers[form] = newHandler;
            form.Paint += newHandler;

            if (form.IsHandleCreated && !form.IsDisposed)
                form.Invalidate();
        }

        /// <summary>
        /// Entfernt das Form aus der Brightness- und Handler-Verwaltung.
        /// </summary>
        public static void UnregisterForm(Form form)
        {
            if (form == null)
                return;

            form.FormClosed -= FormClosed_Handler;
            if (registeredPaintHandlers.TryGetValue(form, out var handler))
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

        /// <summary>
        /// Zeichnet das Helligkeits-Overlay auf jedem Form (außer SettingsForm).
        /// </summary>
        private static void Form_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is Form form && !(form is SettingsForm))
                ApplyBrightnessOverlay(e.Graphics, form.ClientRectangle);
        }

        /// <summary>
        /// Event: Form wurde geschlossen, deregistriert das Form.
        /// </summary>
        private static void FormClosed_Handler(object? sender, FormClosedEventArgs e)
        {
            if (sender is Form form)
                UnregisterForm(form);
        }

        /// <summary>
        /// Überlagert einen Bereich mit einem halbtransparenten Overlay je nach Brightness-Wert.
        /// </summary>
        public static void ApplyBrightnessOverlay(Graphics g, Rectangle rect)
        {
            float brightnessAlpha = (100 - Brightness) / 200.0f; // max. 0.5f
            using (SolidBrush overlay = new(Color.FromArgb((int)(brightnessAlpha * 255), Color.Black)))
                g.FillRectangle(overlay, rect);
        }

        /// <summary>
        /// Überträgt die Einstellungen auf UI, Musik & Speicher.
        /// </summary>
        public static void ApplySettings()
        {
            if (activeForm != null)
                ApplyFullscreenMode(Fullscreen);

            foreach (Form form in activeForms)
                if (form != null && form.IsHandleCreated && !form.IsDisposed)
                    form.Invalidate();

            if (!SoundManager.IsMusicPlaying())
                SoundManager.PlayBackgroundMusic(MusicFilePath);

            SoundManager.SetMusicVolume(MusicVolume / 100.0f);

            SaveSettings();
        }

        /// <summary>
        /// Schaltet den Vollbildmodus um und speichert sofort.
        /// </summary>
        public static void ToggleFullscreen()
        {
            Fullscreen = !Fullscreen;
            ApplyFullscreenMode(Fullscreen);
            SaveSettings();
        }

        /// <summary>
        /// Setzt das aktuelle Form in (oder aus) den Vollbildmodus.
        /// </summary>
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
                    activeForm.WindowState = FormWindowState.Normal; // Für sauber reset
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

                if (activeForm is MainForm mainForm)
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

        /// <summary>
        /// Speichert alle Einstellungen als XML.
        /// </summary>
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
                    XmlSerializer serializer = new(typeof(SerializableSettings));
                    using StreamWriter writer = new(SettingsFilePath);
                    serializer.Serialize(writer, settings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Speichern der Einstellungen: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Lädt alle Einstellungen aus XML (oder übernimmt Defaults).
        /// </summary>
        public static void LoadSettings()
        {
            lock (settingsLock)
            {
                try
                {
                    if (!File.Exists(SettingsFilePath)) return;
                    XmlSerializer serializer = new(typeof(SerializableSettings));
                    using StreamReader reader = new(SettingsFilePath);
                    var settings = serializer.Deserialize(reader) as SerializableSettings;
                    if (settings == null)
                        throw new InvalidDataException("Deserialisierte Einstellungen sind null!");

                    Fullscreen = settings.Fullscreen;
                    Brightness = settings.Brightness;
                    MusicVolume = settings.MusicVolume;
                    SoundVolume = settings.SoundVolume;
                    UnlockedLevel = Math.Max(1, settings.UnlockedLevel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Laden der Einstellungen: {ex.Message}");
                    try { File.Delete(SettingsFilePath); } catch { }
                }
            }
        }

        /// <summary>
        /// Einstellungen, die gespeichert werden.
        /// </summary>
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
