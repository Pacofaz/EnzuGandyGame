using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZombieGame
{
    public class GameForm : Form
    {
        private readonly Game _game;
        private readonly Timer _timer;

        public GameForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            DoubleBuffered = true;

            var screenSize = Screen.PrimaryScreen.Bounds.Size;
            _game = new Game(screenSize);

            KeyPreview = true;
            KeyDown += (s, e) => _game.OnKeyDown(e);
            KeyUp += (s, e) => _game.OnKeyUp(e);
            MouseDown += OnMouseDown;
            MouseMove += (s, e) => _game.OnMouseMove(e);

            _timer = new Timer { Interval = 16 };
            _timer.Tick += (s, e) => { _game.Update(); Invalidate(); };
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _game.Draw(e.Graphics);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            // Erst GameOver-Buttons prüfen, dann das eigentliche Shooting
            if (_game.HandleMouseClick(e)) return;
            _game.OnMouseDown(e);
        }
    }
}
