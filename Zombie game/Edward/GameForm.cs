using System;
using System.Drawing;
using System.Windows.Forms;
using ZombieGame;

namespace ZombieGame
{
    public class GameForm : Form
    {
        private readonly Game _game;
        private readonly Timer _timer;

        public GameForm()
        {
            // Vollbild
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            KeyPreview = true;

            // Game-Logik initialisieren
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            _game = new Game(screenSize);

            // Timer für Update/Draw
            _timer = new Timer { Interval = 16 }; // ca. 60 FPS
            _timer.Tick += (s, e) =>
            {
                _game.Update();
                Invalidate();
            };
            _timer.Start();

            // Input-Events weiterleiten
            this.KeyDown += (s, e) => _game.OnKeyDown(e);
            this.KeyUp += (s, e) => _game.OnKeyUp(e);
            this.MouseDown += (s, e) =>
            {
                if (!_game.HandleMouseClick(e))
                    _game.OnMouseDown(e);
            };
            this.MouseMove += (s, e) => _game.OnMouseMove(e);

            // Beenden bei ESC im GameOver oder Pause
            this.FormClosed += (s, e) => Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _game.Draw(e.Graphics);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GameForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "GameForm";
            this.Load += new System.EventHandler(this.GameForm_Load);
            this.ResumeLayout(false);

        }

        private void GameForm_Load(object sender, EventArgs e)
        {

        }
    }
}
