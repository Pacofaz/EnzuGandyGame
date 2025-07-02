using EnzuGame.Klassen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EnzuGame.Forms
{
    public partial class LevelSelectForm : BaseForm
    {
        private Image? backgroundImg;
        private readonly List<LevelButton> levelButtons = new();
        private LevelButton? hoveredButton;
        private LevelButton? pressedButton;
        private Rectangle backButtonRect;
        private bool backButtonHovered = false, backButtonPressed = false;

        private const float WidthFactor = 0.3f;
        private const float HeightFactor = 0.15f;
        private readonly Size WindowedSize = new(1024, 768);

        public LevelSelectForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Text = "Levelauswahl";

            ConfigureWindow();
            LoadResources();

            Resize += OnResize;
            Paint += OnPaint;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;

            OnResize(this, EventArgs.Empty);
        }

        private void ConfigureWindow()
        {
            if (GameSettings.Fullscreen)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                ClientSize = WindowedSize;
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private Image? SafeLoad(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var img = Image.FromFile(path);
                    Debug.WriteLine($"{path} geladen, Größe: {img.Width}x{img.Height}");
                    return img;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fehler beim Laden von {path}: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Bild fehlt: {path}");
            }
            return null;
        }

        private void LoadResources()
        {
            try
            {
                GameSettings.LoadSettings();
                backgroundImg = SafeLoad("Resources/levels/background.png");

                int maxLevel = 4; // Passe das an, wenn du mehr Level hast!
                levelButtons.Clear();
                for (int i = 1; i <= maxLevel; i++)
                {
                    string basePath = $"Resources/levels/level{i}";
                    Image? normalImg = SafeLoad($"{basePath}.png");
                    Image? hoverImg = SafeLoad($"{basePath}_hover.png");
                    Image? clickedImg = SafeLoad($"{basePath}_clicked.png");
                    Image? lockedImg = (i == 1) ? null : SafeLoad($"{basePath}_locked.png");

                    // Unlock-Logik für mehrere Level dynamisch erweitern:
                    Func<bool> unlocked = i switch
                    {
                        1 => () => true,
                        2 => () => GameSettings.Level2Unlocked,
                        3 => () => GameSettings.Level3Unlocked,
                        4 => () => GameSettings.Level4Unlocked,
                        _ => () => false
                    };

                    levelButtons.Add(
                        new LevelButton(
                            i,
                            Rectangle.Empty,
                            unlocked,
                            normalImg,
                            hoverImg,
                            clickedImg,
                            lockedImg
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading resources: " + ex.Message);
            }
        }

        private void OnResize(object? sender, EventArgs e)
        {
            int count = levelButtons.Count;
            int spacing = 20;
            int btnWidth = (int)(ClientSize.Width * WidthFactor);
            int btnHeight = (int)(ClientSize.Height * HeightFactor);

            int totalHeight = count * btnHeight + (count - 1) * spacing;
            int startX = (ClientSize.Width - btnWidth) / 2;
            int startY = (ClientSize.Height - totalHeight) / 2;

            for (int i = 0; i < count; i++)
            {
                levelButtons[i].Bounds = new Rectangle(
                    startX,
                    startY + i * (btnHeight + spacing),
                    btnWidth,
                    btnHeight
                );
            }

            int backBtnWidth = btnWidth / 2;
            int backBtnHeight = 50;
            backButtonRect = new Rectangle(
                startX + (btnWidth - backBtnWidth) / 2,
                startY + totalHeight + spacing,
                backBtnWidth,
                backBtnHeight
            );

            Invalidate();
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            if (backgroundImg != null)
                g.DrawImage(backgroundImg, 0, 0, Width, Height);

            foreach (var btn in levelButtons)
                btn.Draw(g, btn == hoveredButton, btn == pressedButton);

            DrawBackButton(g);
        }

        private void DrawBackButton(Graphics g)
        {
            Color baseColor = Color.FromArgb(220, 120, 90, 30);
            if (backButtonPressed)
                baseColor = Color.FromArgb(255, 200, 100, 30);
            else if (backButtonHovered)
                baseColor = Color.FromArgb(230, 180, 120, 50);

            using (var b = new SolidBrush(baseColor))
                g.FillRectangle(b, backButtonRect);

            using (var pen = new Pen(Color.FromArgb(100, 80, 40), 3))
                g.DrawRectangle(pen, backButtonRect);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("Zurück", new Font("Segoe UI", 16, FontStyle.Bold), Brushes.White, backButtonRect, sf);
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            LevelButton? prevHoveredButton = hoveredButton;
            bool prevBackHover = backButtonHovered;

            hoveredButton = levelButtons.Find(btn => btn.Bounds.Contains(e.Location) && btn.IsUnlocked());
            backButtonHovered = backButtonRect.Contains(e.Location);

            if (hoveredButton != prevHoveredButton || backButtonHovered != prevBackHover)
                Invalidate();
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            if (hoveredButton != null || backButtonHovered)
            {
                hoveredButton = null;
                backButtonHovered = false;
                Invalidate();
            }
        }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            pressedButton = (hoveredButton?.IsUnlocked() ?? false) ? hoveredButton : null;
            backButtonPressed = backButtonHovered;
            Invalidate();
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (pressedButton != null && pressedButton == hoveredButton && pressedButton.IsUnlocked())
            {
                int level = pressedButton.LevelNumber;
                OpenLevel(level);
            }
            else if (backButtonPressed && backButtonHovered)
            {
                this.Close();
            }
            pressedButton = null;
            backButtonPressed = false;
            Invalidate();
        }

        private void OpenLevel(int level)
        {
            Form? levelForm = level switch
            {
                1 => new Level1Form(),
                2 => new Level2Form(),
                3 => new level3Form(),
                4 => new level4Form(),

                _ => null
            };

            if (levelForm != null)
                levelForm.ShowDialog(this);
        }
    }
}
