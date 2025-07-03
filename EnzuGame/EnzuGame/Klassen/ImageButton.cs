using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuGame.Klassen
{
    /// <summary>
    /// Ein Control, das einen Button mit verschiedenen Bildern (Normal, Hover, Klick) darstellt.
    /// Unterstützt Tastatur-Highlight (IsHovered) und Klick per Code (PerformClick).
    /// </summary>
    public class ImageButton : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image NormalImage { get; set; } = null!;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image HoverImage { get; set; } = null!;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image ClickedImage { get; set; } = null!;

        private bool isHovered = false;
        private bool isPressed = false;
        private bool keyboardHovered = false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHovered
        {
            get => keyboardHovered;
            set
            {
                if (keyboardHovered != value)
                {
                    keyboardHovered = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Löst das Click-Event per Code aus (z.B. für Tastatursteuerung).
        /// </summary>
        public void PerformClick()
        {
            OnClick(EventArgs.Empty);
        }

        public ImageButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = true; Invalidate(); } };
            MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = false; Invalidate(); } };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Image? toDraw = NormalImage;
            if (isPressed && ClickedImage != null)
                toDraw = ClickedImage;
            else if ((isHovered || IsHovered) && HoverImage != null)
                toDraw = HoverImage;

            if (toDraw != null)
                e.Graphics.DrawImage(toDraw, 0, 0, Width, Height);
        }
    }
}
