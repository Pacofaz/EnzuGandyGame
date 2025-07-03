using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    public partial class level3Form : Form
    {
        #region Konstanten & Felder

        private const int GridSize = 9;
        private const int BlueCandyType = 1;
        private const int BlueCandiesGoal = 50;

        private Image? backgroundImg;
        private Image? gridImg;
        private int[,] grid = new int[GridSize, GridSize];
        private readonly Random rnd = new();

        private readonly List<FallingCandy> animatedCandies = new();
        private bool resolving = false;
        private System.Windows.Forms.Timer? animationTimer;

        private Point? selectedCell = null;
        private Point? hoverCell = null;

        private int blueCandiesCollected = 0;
        private bool levelCompleted = false;

        private Button? nextButton;

        private readonly string levelObjective = "Ziel: Sammle 50 blaue Candies!";

        #endregion

        #region Konstruktor & Initialisierung

        public level3Form()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Text = "Level 3";
            ClientSize = new Size(500, 320);

            // Ressourcen laden und Spielfeld initialisieren
            LoadResources();
            InitializeGrid();

            // Event-Handler für Zeichnen und Maus
            Paint += OnPaint;
            Resize += (s, e) => Invalidate();
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
            MouseLeave += (s, e) => { hoverCell = null; Invalidate(); };

            // Animations-Timer initialisieren
            animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
            animationTimer.Tick += OnAnimationTick;

            // "Weiter"-Button einrichten
            nextButton = new Button
            {
                Text = "Weiter",
                Visible = false,
                Location = new Point(20, 250),
                Size = new Size(120, 30)
            };
            nextButton.Click += (s, e) => Close();
            Controls.Add(nextButton);
        }

        private void LoadResources()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "levels");
            backgroundImg = Image.FromFile(Path.Combine(basePath, "lvl1_background.png"));
            gridImg = Image.FromFile(Path.Combine(basePath, "Spielfeld2.png"));
        }

        private void InitializeGrid()
        {
            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                    grid[row, col] = rnd.Next(1, 5);
        }

        #endregion

        #region Paint & Zeichnen

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawBackground(g);
            DrawSidebar(g);
            DrawGrid(g);
            DrawCandies(g);
            DrawAnimatedCandies(g);
            DrawHoverEffect(g);
        }

        private void DrawBackground(Graphics g)
        {
            if (backgroundImg != null)
                g.DrawImage(backgroundImg, 0, 0, ClientSize.Width, ClientSize.Height);
        }

        private void DrawSidebar(Graphics g)
        {
            const int textBoxWidth = 200;
            const int textPadding = 10;

            using var bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20));
            g.FillRectangle(bgBrush, 0, 0, textBoxWidth, ClientSize.Height);

            using var font = new Font("Segoe UI", 12, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            string text = $"{levelObjective}\nGesammelt: {blueCandiesCollected}/{BlueCandiesGoal}";
            if (levelCompleted) text += "\nLEVEL GESCHAFFT!";

            RectangleF rect = new RectangleF(textPadding, textPadding, textBoxWidth - 2 * textPadding, ClientSize.Height);
            g.DrawString(text, font, textBrush, rect);
        }

        private void DrawGrid(Graphics g)
        {
            int textBoxWidth = 200;
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            if (gridImg != null)
                g.DrawImage(gridImg, offsetX - gridMargin, offsetY - gridMargin, gridImgSize, gridImgSize);
        }

        private void DrawCandies(Graphics g)
        {
            int textBoxWidth = 200;
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            float candySize = cellSize * 0.7f;

            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                {
                    if (grid[row, col] == 0) continue;
                    if (animatedCandies.Any(f => f.ToRow == row && f.ToCol == col)) continue;

                    float cx = offsetX + (col + 0.5f) * cellSize;
                    float cy = offsetY + (row + 0.5f) * cellSize;
                    DrawCandy(g, grid[row, col], cx, cy, candySize);
                }
        }

        private void DrawAnimatedCandies(Graphics g)
        {
            int textBoxWidth = 200;
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            float candySize = cellSize * 0.7f;

            foreach (var candy in animatedCandies)
            {
                float cx = offsetX + (candy.ToCol + 0.5f) * cellSize;
                float cy = offsetY + (candy.Y + 0.5f) * cellSize;
                DrawCandy(g, candy.Type, cx, cy, candySize);
            }
        }

        private void DrawCandy(Graphics g, int type, float cx, float cy, float size)
        {
            float px = cx - size / 2, py = cy - size / 2;
            Color candyColor = type switch
            {
                1 => Color.DeepSkyBlue,
                2 => Color.Magenta,
                3 => Color.Orange,
                4 => Color.LimeGreen,
                _ => Color.Gray
            };

            using var brush = new SolidBrush(candyColor);
            g.FillEllipse(brush, px, py, size, size);

            using var pen = new Pen(Color.FromArgb(80, 0, 0, 0), 2);
            g.DrawEllipse(pen, px, py, size, size);
        }

        private void DrawHoverEffect(Graphics g)
        {
            if (!hoverCell.HasValue) return;

            int textBoxWidth = 200;
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            float candySize = cellSize * 0.7f;

            int col = hoverCell.Value.X, row = hoverCell.Value.Y;
            float cx = offsetX + (col + 0.5f) * cellSize;
            float cy = offsetY + (row + 0.5f) * cellSize;
            float px = cx - candySize / 2, py = cy - candySize / 2;
            using var pen = new Pen(Color.Yellow, 4);
            g.DrawEllipse(pen, px + 2, py + 2, candySize - 4, candySize - 4);
        }

        #endregion

        #region Eingabe-Handling

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                selectedCell = new Point(col, row);
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (levelCompleted || resolving) return;
            if (!selectedCell.HasValue || !GetCellFromPoint(e.Location, out int row, out int col)) return;

            var from = selectedCell.Value;
            var to = new Point(col, row);

            // Prüfe, ob Nachbarfeld
            if (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == 1)
            {
                Swap(from.Y, from.X, to.Y, to.X);
                var matches = FindMatches();

                if (matches.Count > 0)
                {
                    int blueMatches = matches.Count(pt => grid[pt.Y, pt.X] == BlueCandyType);
                    blueCandiesCollected += blueMatches;

                    foreach (var pt in matches)
                        grid[pt.Y, pt.X] = 0;

                    AnimateCandiesFall();
                    resolving = true;
                    animationTimer?.Start();
                }
                else
                {
                    Swap(from.Y, from.X, to.Y, to.X); // Rücktausch
                }
            }
            selectedCell = null;
            Invalidate();
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                hoverCell = new Point(col, row);
            else
                hoverCell = null;
            Invalidate();
        }

        private bool GetCellFromPoint(Point p, out int row, out int col)
        {
            int textBoxWidth = 200;
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;

            float x = p.X - offsetX, y = p.Y - offsetY;
            col = (int)(x / cellSize); row = (int)(y / cellSize);
            return col >= 0 && col < GridSize && row >= 0 && row < GridSize;
        }

        #endregion

        #region Spiel- und Animationslogik

        private void OnAnimationTick(object? sender, EventArgs e)
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

            // Wenn Animation fertig
            if (!isAnimating && animatedCandies.Count > 0)
            {
                foreach (var candy in animatedCandies)
                    grid[candy.ToRow, candy.ToCol] = candy.Type;
                animatedCandies.Clear();
                resolving = false;
                animationTimer?.Stop();
                StartResolve();
            }

            Invalidate();
        }

        private void StartResolve()
        {
            var toClear = FindMatches();
            if (toClear.Count > 0)
            {
                int blueMatches = toClear.Count(pt => grid[pt.Y, pt.X] == BlueCandyType);
                blueCandiesCollected += blueMatches;

                foreach (var pt in toClear)
                    grid[pt.Y, pt.X] = 0;
                AnimateCandiesFall();
                resolving = true;
                animationTimer?.Start();
            }
            else
            {
                if (blueCandiesCollected >= BlueCandiesGoal && !levelCompleted)
                {
                    levelCompleted = true;
                    if (nextButton != null) nextButton.Visible = true;

                    // Hier Level 4 freischalten und speichern!
                    GameSettings.Level4Unlocked = true;
                    GameSettings.SaveSettings();
                }
            }
            Invalidate();
        }

        private void AnimateCandiesFall()
        {
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
                            Speed = 0.25f + (emptyCount * 0.13f)
                        });
                        grid[row, col] = 0;
                    }
                }
                for (int i = 0; i < emptyCount; i++)
                {
                    int candyType = rnd.Next(1, 5);
                    animatedCandies.Add(new FallingCandy
                    {
                        Type = candyType,
                        FromRow = -1 - i,
                        ToRow = i,
                        FromCol = col,
                        ToCol = col,
                        Y = -1 - i,
                        Speed = 0.32f
                    });
                }
            }
        }

        private List<Point> FindMatches()
        {
            var matches = new List<Point>();

            // Horizontal prüfen
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

            // Vertikal prüfen
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

        #endregion

        #region Hilfsklassen

        private class FallingCandy
        {
            public int Type;
            public int FromRow, FromCol;
            public int ToRow, ToCol;
            public float Y;
            public float Speed;
        }

        #endregion
    }
}
