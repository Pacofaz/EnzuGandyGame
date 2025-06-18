using System;
using System.Drawing;
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

            using (var menu = new StartMenuForm())
            {
                var result = menu.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    // Benutzer hat "Exit" gewählt oder ESC gedrückt
                    return;
                }
            }

            Application.Run(new GameForm());
        }
    }

    public class GameForm : Form
    {
        private readonly Game _game;
        private readonly Timer _timer;

        public GameForm()
        {
            // Rahmenloser Vollbildmodus
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            DoubleBuffered = true;
            Text = "Zombie Survival";

            // Spiel initialisieren mit voller Bildschirmgröße
            var screenSize = Screen.PrimaryScreen.Bounds.Size;
            _game = new Game(screenSize);

            // Key- und Maus-Events
            KeyPreview = true;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseDown += OnMouseDown;

            // Game-Loop Timer (~60 FPS)
            _timer = new Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                _game.Update();
                Invalidate();
            };
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _game.Draw(e.Graphics);
        }

        private void OnKeyDown(object sender, KeyEventArgs e) => _game.OnKeyDown(e);
        private void OnKeyUp(object sender, KeyEventArgs e) => _game.OnKeyUp(e);
        private void OnMouseDown(object sender, MouseEventArgs e) => _game.OnMouseDown(e);
    }
}