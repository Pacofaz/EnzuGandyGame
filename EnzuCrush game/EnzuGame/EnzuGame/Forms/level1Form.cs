using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    public partial class Level1Form : Form
    {
        private Image backgroundImg;
        private Image gridImg;
        private const int GridSize = 9;
        private int[,] grid;
        private Random rnd = new Random();

        // Animation & Falling
        private List<FallingCandy> animatedCandies = new List<FallingCandy>();
        private bool resolving = false;
        private System.Windows.Forms.Timer animationTimer;

        // Mouse Interaction
        private Point? selectedCell = null;
        private Point? hoverCell = null;

        // UI/State
        private string levelObjective = "Ziel: Erreiche 200 Punkte!";
        private int score = 0;
        private bool levelCompleted = false;
        private Button nextButton;

        public Level1Form()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Text = "Level 1";
            this.ClientSize = new Size(500, 320);

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "levels");
            backgroundImg = Image.FromFile(Path.Combine(basePath, "lvl1_background.png"));
            gridImg = Image.FromFile(Path.Combine(basePath, "Spielfeld2.png"));

            grid = new int[GridSize, GridSize];
            FillRandomGrid();

            this.Paint += Level1Form_Paint;
            this.Resize += (s, e) => this.Invalidate();
            this.MouseDown += Level1Form_MouseDown;
            this.MouseUp += Level1Form_MouseUp;
            this.MouseMove += Level1Form_MouseMove;
            this.MouseLeave += (s, e) => { hoverCell = null; Invalidate(); };

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS for smooth animation
            animationTimer.Tick += AnimationTimer_Tick;

            nextButton = new Button();
            nextButton.Text = "Weiter";
            nextButton.Visible = false;
            nextButton.Location = new Point(20, 250);
            nextButton.Size = new Size(120, 30);
            nextButton.Click += (s, e) =>
            {
                this.Hide();
                GameSettings.SaveSettings();
                var levelSelect = new LevelSelectForm();
                levelSelect.Show();
            };
            Controls.Add(nextButton);
        }

        private void FillRandomGrid()
        {
            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                    grid[row, col] = rnd.Next(1, 5);
        }

        private void Level1Form_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Hintergrund
            if (backgroundImg != null)
                g.DrawImage(backgroundImg, 0, 0, ClientSize.Width, ClientSize.Height);

            // Text-Box
            int textBoxWidth = 160;
            int textPadding = 10;
            using (var bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
                g.FillRectangle(bgBrush, 0, 0, textBoxWidth, ClientSize.Height);

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                string text = levelObjective + $"\nPunkte: {score}";
                if (levelCompleted)
                    text += "\nLEVEL GESCHAFFT!";
                RectangleF rect = new RectangleF(textPadding, textPadding, textBoxWidth - 2 * textPadding, ClientSize.Height);
                g.DrawString(text, font, textBrush, rect);
            }

            // Grid-Layout
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            float candySize = cellSize * 0.7f;

            // Grid-Bild
            if (gridImg != null)
                g.DrawImage(gridImg, offsetX - gridMargin, offsetY - gridMargin, gridImgSize, gridImgSize);

            // Erst feststehende Candies zeichnen
            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                {
                    if (grid[row, col] == 0) continue;
                    if (animatedCandies.Any(f => f.ToRow == row && f.ToCol == col))
                        continue; // Dort landet gleich ein animiertes Candy, also hier nicht zeichnen
                    float cx = offsetX + (col + 0.5f) * cellSize;
                    float cy = offsetY + (row + 0.5f) * cellSize;
                    DrawCandy(g, grid[row, col], cx, cy, candySize);
                }

            // Dann die animierten Candies zeichnen (fallen/schweben)
            foreach (var candy in animatedCandies)
            {
                float cx = offsetX + (candy.ToCol + 0.5f) * cellSize;
                float cy = offsetY + (candy.Y + 0.5f) * cellSize;
                DrawCandy(g, candy.Type, cx, cy, candySize);
            }

            // Hover-Zelle
            if (hoverCell.HasValue)
            {
                int col = hoverCell.Value.X, row = hoverCell.Value.Y;
                float cx = offsetX + (col + 0.5f) * cellSize;
                float cy = offsetY + (row + 0.5f) * cellSize;
                DrawHover(g, cx, cy, candySize);
            }
        }

        private void DrawCandy(Graphics g, int typ, float cx, float cy, float size)
        {
            float px = cx - size / 2, py = cy - size / 2;
            Color candyColor = CandyColor(typ);

            using (var brush = new SolidBrush(candyColor))
                g.FillEllipse(brush, px, py, size, size);

            using (var pen = new Pen(Color.FromArgb(80, 0, 0, 0), 2))
                g.DrawEllipse(pen, px, py, size, size);
        }

        private void DrawHover(Graphics g, float cx, float cy, float size)
        {
            float px = cx - size / 2, py = cy - size / 2;
            using (var pen = new Pen(Color.Yellow, 4))
                g.DrawEllipse(pen, px + 2, py + 2, size - 4, size - 4);
        }

        private Color CandyColor(int typ) => typ switch
        {
            1 => Color.DeepSkyBlue,
            2 => Color.Magenta,
            3 => Color.Orange,
            4 => Color.LimeGreen,
            _ => Color.Gray
        };

        // --- Mouse Interaktion ---

        private void Level1Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                selectedCell = new Point(col, row);
        }

        private void Level1Form_MouseUp(object? sender, MouseEventArgs e)
        {
            if (levelCompleted || resolving) return;

            if (!selectedCell.HasValue || !GetCellFromPoint(e.Location, out int row, out int col)) return;

            var from = selectedCell.Value;
            var to = new Point(col, row);

            if (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == 1)
            {
                Swap(from.Y, from.X, to.Y, to.X);
                var matches = FindMatches();

                if (matches.Count > 0)
                {
                    foreach (var pt in matches)
                        grid[pt.Y, pt.X] = 0;
                    score += matches.Count * 5; // weniger Punkte pro Match
                    AnimateCandiesFall();
                    resolving = true;
                    animationTimer.Start();
                }
                else
                {
                    Swap(from.Y, from.X, to.Y, to.X); // Rücktausch
                }
            }
            selectedCell = null;
            Invalidate();
        }

        private void Level1Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                hoverCell = new Point(col, row);
            else
                hoverCell = null;
            Invalidate();
        }

        private bool GetCellFromPoint(Point p, out int row, out int col)
        {
            int gridMargin = 4;
            int textBoxWidth = 160;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;

            float x = p.X - offsetX, y = p.Y - offsetY;
            col = (int)(x / cellSize); row = (int)(y / cellSize);
            return col >= 0 && col < GridSize && row >= 0 && row < GridSize;
        }

        // --- ANIMATIONSLOGIK ---

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            bool isAnimating = false;
            foreach (var candy in animatedCandies)
            {
                float targetY = candy.ToRow;
                if (candy.Y < targetY)
                {
                    candy.Y += candy.Speed;
                    if (candy.Y > targetY)
                        candy.Y = targetY;
                    isAnimating = true;
                }
            }

            // Animation fertig? Dann ins Grid übernehmen
            if (!isAnimating && animatedCandies.Count > 0)
            {
                foreach (var candy in animatedCandies)
                    grid[candy.ToRow, candy.ToCol] = candy.Type;
                animatedCandies.Clear();
                resolving = false;
                animationTimer.Stop();
                StartResolve(); // Nächster Match-/Fall-Vorgang starten
            }

            Invalidate();
        }

        private void StartResolve()
        {
            var toClear = FindMatches();

            if (score >= 200 && !levelCompleted)
            {
                levelCompleted = true;
                GameSettings.Level2Unlocked = true; // Level 2 explizit freischalten
                GameSettings.UnlockedLevel = Math.Max(GameSettings.UnlockedLevel, 2); // falls du das parallel brauchst
                GameSettings.SaveSettings();
                nextButton.Visible = true;
            }

            Invalidate();
        }


        private void AnimateCandiesFall()
        {
            // Pro Spalte: Alle Lücken von unten nach oben suchen und Candies animieren
            for (int col = 0; col < GridSize; col++)
            {
                int emptyCount = 0;
                for (int row = GridSize - 1; row >= 0; row--)
                {
                    if (grid[row, col] == 0)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        animatedCandies.Add(new FallingCandy
                        {
                            Type = grid[row, col],
                            FromRow = row,
                            ToRow = row + emptyCount,
                            FromCol = col,
                            ToCol = col,
                            Y = row,
                            Speed = 0.25f + (emptyCount * 0.13f) // höhere Lücke = schneller
                        });
                        grid[row, col] = 0; // Candy "schwebt" jetzt
                    }
                }
                // Neue Candies oben einfügen (von oben reinfallen)
                for (int i = 0; i < emptyCount; i++)
                {
                    int candyType = rnd.Next(1, 5);
                    animatedCandies.Add(new FallingCandy
                    {
                        Type = candyType,
                        FromRow = -1 - i, // Von weiter oben starten
                        ToRow = i,
                        FromCol = col,
                        ToCol = col,
                        Y = -1 - i,
                        Speed = 0.32f
                    });
                }
            }
        }

        // --- GAME LOGIK ---

        private List<Point> FindMatches()
        {
            var matches = new List<Point>();
            // Horizontal
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize - 2; col++)
                {
                    int val = grid[row, col];
                    if (val != 0 && val == grid[row, col + 1] && val == grid[row, col + 2])
                    {
                        matches.Add(new Point(col, row));
                        matches.Add(new Point(col + 1, row));
                        matches.Add(new Point(col + 2, row));
                    }
                }
            }
            // Vertikal
            for (int col = 0; col < GridSize; col++)
            {
                for (int row = 0; row < GridSize - 2; row++)
                {
                    int val = grid[row, col];
                    if (val != 0 && val == grid[row + 1, col] && val == grid[row + 2, col])
                    {
                        matches.Add(new Point(col, row));
                        matches.Add(new Point(col, row + 1));
                        matches.Add(new Point(col, row + 2));
                    }
                }
            }
            return matches.Distinct().ToList();
        }

        private void Swap(int r1, int c1, int r2, int c2)
        {
            int tmp = grid[r1, c1];
            grid[r1, c1] = grid[r2, c2];
            grid[r2, c2] = tmp;
        }

        // --- Hilfsklasse für Animation ---
        private class FallingCandy
        {
            public int Type;
            public int FromRow, FromCol;
            public int ToRow, ToCol;
            public float Y; // aktuelle Position in Zellen (float, damit smooth)
            public float Speed; // pro Tick
        }
    }
}
