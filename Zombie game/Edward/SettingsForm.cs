using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZombieGame
{
    public class SettingsForm : Form
    {
        private CheckBox chkFullscreen;
        private Button btnOk;
        private Button btnCancel;

        public bool FullscreenEnabled { get; set; }

        public SettingsForm()
        {
            InitializeComponent();
            this.MinimumSize = new Size(440, 250);
        }

        private void InitializeComponent()
        {
            Text = "Einstellungen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(480, 250);
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;

            chkFullscreen = new CheckBox
            {
                Text = "Vollbildmodus",
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(32, 36)
            };
            Controls.Add(chkFullscreen);

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(140, 50),
            };
            Controls.Add(btnOk);

            btnCancel = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(140, 50),
            };
            Controls.Add(btnCancel);

            this.Resize += (s, e) => LayoutButtons();
            LayoutButtons();

            Load += SettingsForm_Load;
            KeyDown += SettingsForm_KeyDown;
        }

        private void LayoutButtons()
        {
            int spacing = 40;
            int y = ClientSize.Height - btnOk.Height - 40;
            btnOk.Location = new Point((ClientSize.Width - btnOk.Width * 2 - spacing) / 2, y);
            btnCancel.Location = new Point(btnOk.Right + spacing, y);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            chkFullscreen.Checked = FullscreenEnabled;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            FullscreenEnabled = chkFullscreen.Checked;
            base.OnFormClosing(e);
        }
    }
}
