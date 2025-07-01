using EnzuGame.Forms;
using System;
using System.Windows.Forms;

namespace EnzuGame
{
    /// <summary>
    /// Haupt-Einstiegspunkt für die EnzuGame-Anwendung.
    /// Initialisiert den ApplicationContext und steuert den Ablauf (Intro, MainMenu).
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Einstiegspunkt für die Anwendung (Single-Threaded Apartment).
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyAppContext());
        }

        /// <summary>
        /// Custom ApplicationContext für die Kontrolle des App-Lebenszyklus.
        /// Öffnet zuerst das Intro, danach das Hauptmenü.
        /// </summary>
        public sealed class MyAppContext : ApplicationContext
        {
            public MyAppContext()
            {
                var intro = new FormIntro();
                intro.FormClosed += (s, e) =>
                {
                    // Wenn das Intro zu Ende ist, öffne das Hauptmenü
                    var mainForm = new MainForm();
                    mainForm.FormClosed += (sender, args) => ExitThread();
                    mainForm.Show();
                };
                intro.Show();
            }
        }
    }
}
