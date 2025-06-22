using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EnzuGame.Klassen
{
    public class ImageButton : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image NormalImage { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image HoverImage { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image ClickedImage { get; set; }

        private bool isHovered = false;
        private bool isPressed = false;

        public ImageButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            this.MouseEnter += (s, e) => { isHovered = true; Invalidate(); };
            this.MouseLeave += (s, e) => { isHovered = false; isPressed = false; Invalidate(); };
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = true; Invalidate(); } };
            this.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) { isPressed = false; Invalidate(); } };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Image img = NormalImage;
            if (isPressed && ClickedImage != null) img = ClickedImage;
            else if (isHovered && HoverImage != null) img = HoverImage;
            if (img != null)
                e.Graphics.DrawImage(img, 0, 0, Width, Height);
        }
    }
}
