using System;
using System.Drawing;

namespace EnzuGame.Klassen
{
    /// <summary>
    /// Stellt einen auswählbaren Level-Button im Level-Auswahlmenü dar.
    /// Zeichnet sich je nach Status (normal, hovered, clicked, locked) unterschiedlich.
    /// </summary>
    public class LevelButton
    {
        /// <summary>Die Levelnummer, die der Button repräsentiert.</summary>
        public int LevelNumber { get; }

        /// <summary>Bildschirmbereich, in dem der Button angezeigt wird.</summary>
        public Rectangle Bounds { get; set; }

        /// <summary>Delegate, das prüft, ob das Level freigeschaltet ist.</summary>
        public Func<bool> IsUnlocked { get; }

        /// <summary>Bild für normalen Zustand.</summary>
        public Image NormalImage { get; }
        /// <summary>Bild bei Mouse-Over.</summary>
        public Image HoverImage { get; }
        /// <summary>Bild beim Klicken.</summary>
        public Image ClickedImage { get; }
        /// <summary>Bild für gesperrtes Level.</summary>
        public Image LockedImage { get; }

        /// <summary>
        /// Konstruktor für einen LevelButton mit allen Darstellungen.
        /// </summary>
        public LevelButton(
            int number,
            Rectangle bounds,
            Func<bool> unlocked,
            Image normal,
            Image hover,
            Image clicked,
            Image locked)
        {
            LevelNumber = number;
            Bounds = bounds;
            IsUnlocked = unlocked;
            NormalImage = normal;
            HoverImage = hover;
            ClickedImage = clicked;
            LockedImage = locked;
        }

        /// <summary>
        /// Zeichnet den Button im passenden Style.
        /// </summary>
        public void Draw(Graphics g, bool hovered, bool pressed)
        {
            if (IsUnlocked())
            {
                // Freigeschaltet: Je nach Interaktion andere Grafik
                Image? toDraw = NormalImage;
                if (pressed && ClickedImage != null)
                    toDraw = ClickedImage;
                else if (hovered && HoverImage != null)
                    toDraw = HoverImage;

                if (toDraw != null)
                    g.DrawImage(toDraw, Bounds);
            }
            else
            {
                // Gesperrt: locked-Bild, ansonsten Fallback
                if (LockedImage != null)
                {
                    g.DrawImage(LockedImage, Bounds);
                }
                else if (NormalImage != null)
                {
                    g.DrawImage(NormalImage, Bounds);
                }
                else
                {
                    using var pen = new Pen(Color.Red, 2);
                    g.DrawRectangle(pen, Bounds);
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("?", SystemFonts.DefaultFont, Brushes.Red, Bounds, sf);
                }
            }
        }
    }
}
