using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuGame.Klassen
{
    /// <summary>
    /// Ein Control, das einen Button mit verschiedenen Bildern (Normal, Hover, Klick) darstellt.
    /// </summary>
    public class ImageButton : Control
    {
        /// <summary>Standardbild im normalen Zustand.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image NormalImage { get; set; } = null!;

        /// <summary>Bild bei Mouse-Over.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image HoverImage { get; set; } = null!;

        /// <summary>Bild, wenn der Button gedrückt ist.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image ClickedImage { get; set; } = null!;

        private bool isHovered = false;
        private bool isPressed = false;

        public ImageButton()
        {
            // Optimierte Zeichenstile aktivieren
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // Maus-Events für Hover/Klick-Status
            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = true; Invalidate(); } };
            MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = false; Invalidate(); } };
        }

        /// <summary>
        /// Zeichnet den Button abhängig vom aktuellen Status (normal, hovered, gedrückt).
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Image? toDraw = NormalImage;
            if (isPressed && ClickedImage != null)
                toDraw = ClickedImage;
            else if (isHovered && HoverImage != null)
                toDraw = HoverImage;

            if (toDraw != null)
                e.Graphics.DrawImage(toDraw, 0, 0, Width, Height);
        }
    }
}
