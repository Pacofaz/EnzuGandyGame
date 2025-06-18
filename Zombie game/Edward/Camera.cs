using System.Drawing;
using ZombieGame.Entities;

namespace ZombieGame.Utils
{
    public class Camera
    {
        public PointF Position { get; private set; }
        public float Zoom { get; set; } = 1.5f; 

        private readonly Size _screenSize;
        private readonly Player _player;

        public Camera(Size screenSize, Player player)
        {
            _screenSize = screenSize;
            _player = player;
        }

        public void Update()
        {
            float halfW = (_screenSize.Width / Zoom) / 2f;
            float halfH = (_screenSize.Height / Zoom) / 2f;

            Position = new PointF(
                _player.Position.X - halfW,
                _player.Position.Y - halfH
            );
        }
    }
}
