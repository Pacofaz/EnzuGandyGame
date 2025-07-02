using System;
using System.Drawing;

namespace EnzuGame.Klassen
{
    public class LevelButton
    {
        public int LevelNumber { get; }
        public Rectangle Bounds { get; set; }
        public Func<bool> IsUnlocked { get; }
        public Image? NormalImage { get; }
        public Image? HoverImage { get; }
        public Image? ClickedImage { get; }
        public Image? LockedImage { get; }

        public LevelButton(
            int number,
            Rectangle bounds,
            Func<bool> unlocked,
            Image? normal,
            Image? hover,
            Image? clicked,
            Image? locked)
        {
            LevelNumber = number;
            Bounds = bounds;
            IsUnlocked = unlocked;
            NormalImage = normal;
            HoverImage = hover;
            ClickedImage = clicked;
            LockedImage = locked;
        }

        public void Draw(Graphics g, bool hovered, bool pressed)
        {
            Image? toDraw = null;

            if (IsUnlocked())
            {
                if (pressed && ClickedImage != null)
                    toDraw = ClickedImage;
                else if (hovered && HoverImage != null)
                    toDraw = HoverImage;
                else
                    toDraw = NormalImage;

                if (toDraw != null)
                    g.DrawImage(toDraw, Bounds);
            }
            else // locked
            {
                if (LockedImage != null)
                    g.DrawImage(LockedImage, Bounds);
                else if (NormalImage != null)
                    g.DrawImage(NormalImage, Bounds);
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
