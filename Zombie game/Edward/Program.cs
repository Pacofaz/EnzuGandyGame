using System;
using System.Windows.Forms;

namespace ZombieGame
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Zeige nur das Startmenü
            using (var menu = new StartMenuForm())
            {
                if (menu.ShowDialog() != DialogResult.OK)
                    // Exit oder ESC → Programm beenden
                    return;
            }

            // Direkt ins Spiel
            Application.Run(new GameForm());
        }
    }
}
