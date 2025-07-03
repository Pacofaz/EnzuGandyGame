// SettingsForm.cs
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

        /// <summary>
        /// Vor dem Aufruf von ShowDialog setzen, danach hier abfragen.
        /// </summary>
        public bool FullscreenEnabled { get; set; }

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Einstellungen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(300, 150);
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;

            chkFullscreen = new CheckBox
            {
                Text = "Vollbildmodus",
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(chkFullscreen);

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(50, 80),
                Size = new Size(80, 30)
            };
            Controls.Add(btnOk);

            btnCancel = new Button
            {
                Text = "Abbrechen",
                DialogResult = DialogResult.Cancel,
                Location = new Point(160, 80),
                Size = new Size(80, 30)
            };
            Controls.Add(btnCancel);

            Load += SettingsForm_Load;
            KeyDown += SettingsForm_KeyDown;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Initialzustand übernehmen
            chkFullscreen.Checked = FullscreenEnabled;
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
