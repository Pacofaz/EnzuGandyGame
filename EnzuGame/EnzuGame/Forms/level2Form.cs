using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using EnzuGame.Klassen;

namespace EnzuGame.Forms
{
    /// <summary>
    /// Level 2: Ziel ist es, 30 grüne Candies zu sammeln.  
    /// Der Code enthält Spiellogik, Animationen, Eingaben und UI.
    /// </summary>
    public class Level2Form : Form
    {
        // --- Ressourcen & Spielfeld ---
        private Image backgroundImg;
        private Image gridImg;
        private const int GridSize = 9;
        private int[,] grid; // Spielfeld-Array (9x9)
        private readonly Random rnd = new();

        // --- Animation & Candy-Bewegung ---
        private readonly List<FallingCandy> animatedCandies = new();
        private bool resolving = false;
        private readonly System.Windows.Forms.Timer animationTimer;

        // --- Eingabe (Maus) ---
        private Point? selectedCell = null;
        private Point? hoverCell = null;

        // --- UI/State ---
        private readonly string levelObjective = "Ziel: Sammle 30 grüne Candies!";
        private int greenCandiesCollected = 0;
        private const int greenCandiesGoal = 30;
        private bool levelCompleted = false;
        private readonly Button nextButton;

        /// <summary>
        /// Initialisiert das Level, lädt Grafiken und setzt alles auf Start.
        /// </summary>
        public Level2Form()
        {
            DoubleBuffered = true;
            Text = "Level 2";
            this.ClientSize = new Size(500, 320);

            // --- Grafiken laden ---
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "levels");
            backgroundImg = Image.FromFile(Path.Combine(basePath, "lvl1_background.png")); // Passe ggf. an
            gridImg = Image.FromFile(Path.Combine(basePath, "Spielfeld2.png"));

            // --- Grid initialisieren ---
            grid = new int[GridSize, GridSize];
            FillRandomGrid();

            // --- Event-Handler registrieren ---
            this.Paint += Level2Form_Paint;
            this.Resize += (s, e) => this.Invalidate();
            this.MouseDown += Level2Form_MouseDown;
            this.MouseUp += Level2Form_MouseUp;
            this.MouseMove += Level2Form_MouseMove;
            this.MouseLeave += (s, e) => { hoverCell = null; Invalidate(); };

            // --- Animationstimer ---
            animationTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;

            // --- Weiter-Button (Level-Ende) ---
            nextButton = new Button
            {
                Text = "Weiter",
                Visible = false,
                Location = new Point(20, 250),
                Size = new Size(120, 30)
            };
            nextButton.Click += (s, e) => { this.Close(); };
            Controls.Add(nextButton);
        }

        /// <summary>
        /// Füllt das Spielfeld mit zufälligen Candies (Typ 1-4).
        /// </summary>
        private void FillRandomGrid()
        {
            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                    grid[row, col] = rnd.Next(1, 5);
        }

        /// <summary>
        /// Zeichnet Hintergrund, Spielfeld, UI und Candies.
        /// </summary>
        private void Level2Form_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Hintergrundbild
            g.DrawImage(backgroundImg, 0, 0, ClientSize.Width, ClientSize.Height);

            // Linke Textbox (Ziel, Status)
            int textBoxWidth = 200, textPadding = 10;
            using (var bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
                g.FillRectangle(bgBrush, 0, 0, textBoxWidth, ClientSize.Height);

            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                string text = levelObjective + $"\nGesammelt: {greenCandiesCollected}/{greenCandiesGoal}";
                if (levelCompleted) text += "\nLEVEL GESCHAFFT!";
                RectangleF rect = new RectangleF(textPadding, textPadding, textBoxWidth - 2 * textPadding, ClientSize.Height);
                g.DrawString(text, font, textBrush, rect);
            }

            // Spielfeld-Bereich berechnen
            int gridMargin = 4;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;
            float candySize = cellSize * 0.7f;

            // Grid-Bild
            g.DrawImage(gridImg, offsetX - gridMargin, offsetY - gridMargin, gridImgSize, gridImgSize);

            // Feststehende Candies zeichnen
            for (int row = 0; row < GridSize; row++)
                for (int col = 0; col < GridSize; col++)
                {
                    if (grid[row, col] == 0) continue;
                    if (animatedCandies.Any(f => f.ToRow == row && f.ToCol == col)) continue;
                    float cx = offsetX + (col + 0.5f) * cellSize;
                    float cy = offsetY + (row + 0.5f) * cellSize;
                    DrawCandy(g, grid[row, col], cx, cy, candySize);
                }

            // Animierte Candies
            foreach (var candy in animatedCandies)
            {
                float cx = offsetX + (candy.ToCol + 0.5f) * cellSize;
                float cy = offsetY + (candy.Y + 0.5f) * cellSize;
                DrawCandy(g, candy.Type, cx, cy, candySize);
            }

            // Hover-Highlight
            if (hoverCell.HasValue)
            {
                int col = hoverCell.Value.X, row = hoverCell.Value.Y;
                float cx = offsetX + (col + 0.5f) * cellSize;
                float cy = offsetY + (row + 0.5f) * cellSize;
                DrawHover(g, cx, cy, candySize);
            }
        }

        /// <summary>
        /// Zeichnet ein Candy an die Zielposition.
        /// </summary>
        private void DrawCandy(Graphics g, int typ, float cx, float cy, float size)
        {
            float px = cx - size / 2, py = cy - size / 2;
            Color candyColor = CandyColor(typ);

            using (var brush = new SolidBrush(candyColor))
                g.FillEllipse(brush, px, py, size, size);

            using (var pen = new Pen(Color.FromArgb(80, 0, 0, 0), 2))
                g.DrawEllipse(pen, px, py, size, size);
        }

        /// <summary>
        /// Zeichnet einen Hover-Effekt um die Zelle.
        /// </summary>
        private void DrawHover(Graphics g, float cx, float cy, float size)
        {
            float px = cx - size / 2, py = cy - size / 2;
            using (var pen = new Pen(Color.Yellow, 4))
                g.DrawEllipse(pen, px + 2, py + 2, size - 4, size - 4);
        }

        /// <summary>
        /// Gibt die passende Farbe für den Candy-Typ zurück.
        /// </summary>
        private Color CandyColor(int typ) => typ switch
        {
            1 => Color.DeepSkyBlue,
            2 => Color.Magenta,
            3 => Color.Orange,
            4 => Color.LimeGreen, // Das ist das grüne Candy!
            _ => Color.Gray
        };

        // ----------- MOUSE EVENTS ------------

        private void Level2Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                selectedCell = new Point(col, row);
        }

        private void Level2Form_MouseUp(object? sender, MouseEventArgs e)
        {
            if (levelCompleted || resolving) return;
            if (!selectedCell.HasValue || !GetCellFromPoint(e.Location, out int row, out int col)) return;

            var from = selectedCell.Value;
            var to = new Point(col, row);

            // Prüfe, ob die Zellen benachbart sind
            if (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == 1)
            {
                Swap(from.Y, from.X, to.Y, to.X);
                var matches = FindMatches();

                if (matches.Count > 0)
                {
                    // Zähle, wie viele grüne Candies getroffen wurden
                    int greenMatches = matches.Count(pt => grid[pt.Y, pt.X] == 4);
                    greenCandiesCollected += greenMatches;

                    foreach (var pt in matches)
                        grid[pt.Y, pt.X] = 0;

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

        private void Level2Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (GetCellFromPoint(e.Location, out int row, out int col))
                hoverCell = new Point(col, row);
            else
                hoverCell = null;
            Invalidate();
        }

        /// <summary>
        /// Liefert die Zeile/Spalte für eine Mausposition im Grid.
        /// </summary>
        private bool GetCellFromPoint(Point p, out int row, out int col)
        {
            int gridMargin = 4;
            int textBoxWidth = 200;
            int gridImgSize = Math.Min(ClientSize.Width - textBoxWidth, ClientSize.Height);
            int gridPixelSize = gridImgSize - 2 * gridMargin;
            float cellSize = gridPixelSize / (float)GridSize;
            int offsetX = textBoxWidth + gridMargin;
            int offsetY = (ClientSize.Height - gridImgSize) / 2 + gridMargin;

            float x = p.X - offsetX, y = p.Y - offsetY;
            col = (int)(x / cellSize); row = (int)(y / cellSize);
            return col >= 0 && col < GridSize && row >= 0 && row < GridSize;
        }

        // ----------- ANIMATION & SPIELLOGIK -----------

        /// <summary>
        /// Steuert die Candy-Fall-Animation. Übergibt neue Positionen nach Abschluss.
        /// </summary>
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

            // Animation beendet: Grid übernehmen und nächste Runde prüfen
            if (!isAnimating && animatedCandies.Count > 0)
            {
                foreach (var candy in animatedCandies)
                    grid[candy.ToRow, candy.ToCol] = candy.Type;
                animatedCandies.Clear();
                resolving = false;
                animationTimer.Stop();
                StartResolve();
            }

            Invalidate();
        }

        /// <summary>
        /// Prüft nach Animation auf weitere Matches oder das Levelziel.
        /// </summary>
        private void StartResolve()
        {
            var toClear = FindMatches();
            if (toClear.Count > 0)
            {
                int greenMatches = toClear.Count(pt => grid[pt.Y, pt.X] == 4);
                greenCandiesCollected += greenMatches;

                foreach (var pt in toClear)
                    grid[pt.Y, pt.X] = 0;
                AnimateCandiesFall();
                resolving = true;
                animationTimer.Start();
            }
            else
            {
                if (greenCandiesCollected >= greenCandiesGoal && !levelCompleted)
                {
                    levelCompleted = true;
                    nextButton.Visible = true;
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Animiert alle fallenden Candies nach Match.
        /// </summary>
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

        /// <summary>
        /// Sucht alle 3er- oder größeren Reihen (horizontal/vertikal).
        /// </summary>
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

        /// <summary>
        /// Tauscht zwei Candies im Grid.
        /// </summary>
        private void Swap(int r1, int c1, int r2, int c2)
        {
            int tmp = grid[r1, c1];
            grid[r1, c1] = grid[r2, c2];
            grid[r2, c2] = tmp;
        }

        /// <summary>
        /// Hilfsklasse für animierte Candies (Falling).
        /// </summary>
        private class FallingCandy
        {
            public int Type;
            public int FromRow, FromCol;
            public int ToRow, ToCol;
            public float Y;
            public float Speed;
        }
    }
}
