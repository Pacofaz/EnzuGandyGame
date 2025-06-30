using EnzuGame.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnzuGame
{
    static class Program
    {
        // In Program.cs
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyAppContext());
        }

        // Neue Klasse im Projekt anlegen
        public class MyAppContext : ApplicationContext
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