using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZombieGame.Entities
{
    // Die Klasse Bullet repräsentiert ein Projektil im Spiel, das sich in einer Richtung bewegt 
    public class Bullet : Entity
    {
        // dx und dy geben die Bewegungsrichtung des Projektils an (normalisierte Vektorkomponenten)
        private readonly float dirX, dirY;
        // Schaden, den das Projektil verursacht
        private readonly int damage;

        /// <summary>
        /// Initialisiert eine neue Instanz von Bullet.
        /// </summary>
        /// <param name="startPos">Ausgangsposition des Projektils.</param>
        /// <param name="dx">Richtungsvektor X-Komponente.</param>
        /// <param name="dy">Richtungsvektor Y-Komponente.</param>
        /// <param name="damage">Schadenswert des Projektils.</param>
        public Bullet(PointF startPos, float dx, float dy, int damage)
            : base(
                  // Ausgangsposition leicht verschoben, damit der Mittelpunkt der 8x8-Pixel-Ellipse stimmt
                  new PointF(startPos.X - 4, startPos.Y - 4),
                  0f,             // Rotation (für Kugeln nicht benötigt)
                  new SizeF(8, 8) // Größe des Projektils in Pixeln
              )
        {
            dirX = dx;
            dirY = dy;
            this.damage = damage;
        }

        /// <summary>
        /// Aktualisiert die Position der Kugel basierend auf der Richtung und der Geschwindigkeit.
        /// </summary>
        public override void Update()
        {
            // Bewegt die Kugel um dirX * Geschwindigkeit und dirY * Geschwindigkeit
            Position = new PointF(
                Position.X + dirX * 15f, // 15 Pixel pro Update-Schritt in X-Richtung
                Position.Y + dirY * 15f  // 15 Pixel pro Update-Schritt in Y-Richtung
            );
        }

        /// <summary>
        /// Zeichnet das Projektil mit einem leuchtenden Effekt und Kern.
        /// </summary>
        public override void Draw(Graphics g)
        {
            // Rechteck, das die Ellipse umgibt
            var rect = new RectangleF(Position.X, Position.Y, Size.Width, Size.Height);

            // 1) Leuchtender Außen-Glow
            using (var glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(rect);
                using (var glowBrush = new PathGradientBrush(glowPath))
                {
                    // Transparenter Kern, roter Rand
                    glowBrush.CenterColor = Color.FromArgb(0, 255, 50, 50);
                    glowBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 50, 50) };
                    glowBrush.FocusScales = new PointF(0.8f, 0.8f);

                    // Etwas vergrößertes Rechteck für den äußeren Glow
                    var inflated = rect;
                    inflated.Inflate(12, 12);
                    using (var inflatedPath = new GraphicsPath())
                    {
                        inflatedPath.AddEllipse(inflated);
                        g.FillPath(glowBrush, inflatedPath);
                    }
                }
            }

            // 2) Ringförmiger Übergangseffekt um den Kern
            using (var ringPath = new GraphicsPath())
            {
                var ringRect = rect;
                ringRect.Inflate(4, 4);
                ringPath.AddEllipse(ringRect);
                using (var ringBrush = new PathGradientBrush(ringPath))
                {
                    // Halbtransparenter roter Kern, zu transparentem Rand übergehend
                    ringBrush.CenterColor = Color.FromArgb(120, 255, 50, 50);
                    ringBrush.SurroundColors = new[] { Color.FromArgb(0, 255, 50, 50) };
                    ringBrush.FocusScales = new PointF(0.5f, 0.5f);
                    g.FillPath(ringBrush, ringPath);
                }
            }

            // 3) Roter Kern der Kugel
            using (var coreBrush = new SolidBrush(Color.FromArgb(255, 255, 50, 50)))
            {
                g.FillEllipse(coreBrush, rect);
            }
        }

        /// <summary>
        /// Prüft, ob die Kugel außerhalb der Spielkarte liegt.
        /// </summary>
        /// <param name="mapWidth">Breite der Karte in Pixeln.</param>
        /// <param name="mapHeight">Höhe der Karte in Pixeln.</param>
        /// <returns>True, wenn die Kugel außerhalb liegt.</returns>
        public bool IsOffMap(int mapWidth, int mapHeight)
        {
            return Position.X < 0 || Position.Y < 0
                || Position.X > mapWidth || Position.Y > mapHeight;
        }

        /// <summary>
        /// Gibt den durch das Projektil verursachten Schaden zurück.
        /// </summary>
        public int GetDamage() => damage;
    }
}
