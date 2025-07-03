using System.Drawing;
using ZombieGame.Entities;

namespace ZombieGame.Utils
{
    // Die Klasse Camera steuert die Ansicht auf die Spielwelt basierend auf der Spielerposition
    public class Camera
    {
        // Obere linke Ecke des sichtbaren Bereichs in Weltkoordinaten
        public PointF Position { get; private set; }
        // Zoom-Faktor: Werte >1 vergrößern, <1 verkleinern
        public float Zoom { get; set; } = 1.5f;

        // Größe des Anzeigefensters in Pixeln
        private readonly Size _screenSize;
        // Referenz auf den Spieler, der im Zentrum der Kamera bleiben soll
        private readonly Player _player;

        /// <summary>
        /// Initialisiert eine neue Kamera mit Bildschirmgröße und Spielerreferenz.
        /// </summary>
        /// <param name="screenSize">Größe des Viewports in Pixeln.</param>
        /// <param name="player">Spieler-Entity, der verfolgt wird.</param>
        public Camera(Size screenSize, Player player)
        {
            _screenSize = screenSize;
            _player = player;
        }

        /// <summary>
        /// Aktualisiert die Kameraposition so, dass der Spieler im Zentrum bleibt.
        /// </summary>
        public void Update()
        {
            // Halbe Breite und Höhe des sichtbaren Bereichs in Weltkoordinaten, angepasst an den Zoom
            float halfW = (_screenSize.Width / Zoom) / 2f;
            float halfH = (_screenSize.Height / Zoom) / 2f;

            // Berechne neue obere linke Ecke basierend auf Spielerposition minus der halben Bildschirmgröße
            Position = new PointF(
                _player.Position.X - halfW,
                _player.Position.Y - halfH
            );
        }
    }
}
