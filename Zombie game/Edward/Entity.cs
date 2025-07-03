using System.Drawing;

namespace ZombieGame.Entities
{
    /// <summary>
    /// Basisklasse für alle spielbaren Entitäten (z.B. Spieler, Zombies, Kugeln).
    /// Definiert gemeinsame Eigenschaften und abstrakte Methoden für Bewegung und Zeichnung.
    /// </summary>
    public abstract class Entity
    {
        // Aktuelle Position der Entität in Weltkoordinaten (obere linke Ecke)
        public PointF Position;
        // Größe der Entität in Weltkoordinaten (Breite, Höhe)
        public SizeF Size;
        // Geschwindigkeit, kann in Unterklassen zur Bewegung genutzt werden
        public float Speed;

        /// <summary>
        /// Konstruktor für Basiseigenschaften aller Entitäten.
        /// </summary>
        /// <param name="pos">Startposition (obere linke Ecke) in Weltkoordinaten.</param>
        /// <param name="speed">Grundgeschwindigkeit (z.B. für Bewegungs-Updates).</param>
        /// <param name="size">Größe der Entität (Breite, Höhe) in Pixeln.</param>
        protected Entity(PointF pos, float speed, SizeF size)
        {
            Position = pos;
            Speed = speed;
            Size = size;
        }

        /// <summary>
        /// Methode zur Aktualisierung des Zustands der Entität,
        /// muss in abgeleiteten Klassen implementiert werden (z.B. Bewegung).
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Zeichnet die Entität auf dem Bildschirm. Abgeleitete Klassen implementieren hier ihre Render-Logik.
        /// </summary>
        /// <param name="g">Graphics-Objekt zum Zeichnen.</param>
        public abstract void Draw(Graphics g);
    }
}
