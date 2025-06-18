using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using ZombieGame.Entities;
using ZombieGame.Managers;

namespace ZombieGame.Utils
{
    public static class UI
    {
        private static readonly Bitmap HealthBarOutlineImage;

        static UI()
        {
            var path = Path.Combine(Application.StartupPath, "Assets", "healthbar_outline.png");
            if (!File.Exists(path))
                throw new FileNotFoundException("healthbar_outline.png nicht gefunden.", path);

            HealthBarOutlineImage = new Bitmap(path);
        }

        /// <summary>
        /// Zeichnet das Spiel‐UI mit frei konfigurierbarer Healthbar, Outline und stilisierter Wave-/Alive-Info.
        /// Die Wave-Info erscheint oben zentriert in handschriftlicher, geglätteter Schrift.
        /// Health-Zahl bekommt nun ebenfalls eine schwarze Kontur.
        /// </summary>
        public static void DrawGame(
            Graphics g,
            Player p,
            WaveManager w,
            Size screen,

            // Healthbar-Füllung
            int barX = 100,
            int barY = 75,
            int barWidth = 210,
            int barHeight = 30,

            // Outline
            int outlineWidth = 350,
            int outlineHeight = 180,
            int outlineOffsetX = -42,
            int outlineOffsetY = -12,

            // Weapon-Icon
            int weaponX = 20,
            int weaponIconSize = 40,
            int weaponYOffset = 70,

            // Minimap
            int minimapSize = 150,
            int minimapOffsetRight = 20,
            int minimapOffsetTop = 20
        )
        {
            // 0) Anti-Aliasing aktivieren
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;


            // 1) Healthbar-Hintergrund & -Füllung
            g.FillRectangle(Brushes.Gray, barX, barY, barWidth, barHeight);
            g.FillRectangle(Brushes.Lime, barX, barY, 2 * p.Health, barHeight);

            // Health-Zahl mit Outline
            using (var healthFont = new Font("Segoe Print", 16, FontStyle.Regular))
            {
                string healthText = $"{p.Health}/100";
                float emSize = healthFont.Size * g.DpiY / 72f;
                var textPos = new PointF(barX + barWidth + 10, barY - 2);

                using (var path = new GraphicsPath())
                {
                    path.AddString(
                        healthText,
                        healthFont.FontFamily,
                        (int)healthFont.Style,
                        emSize,
                        textPos,
                        StringFormat.GenericDefault
                    );
                    using (var pen = new Pen(Color.Black, 4) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                    g.FillPath(Brushes.White, path);
                }
            }

            // 2) Outline über der Füllung
            int outlineX = barX + (barWidth - outlineWidth) / 2 + outlineOffsetX;
            int outlineY = barY - ((outlineHeight - barHeight) / 2) + outlineOffsetY;
            g.DrawImage(HealthBarOutlineImage, outlineX, outlineY, outlineWidth, outlineHeight);

            // 3) Wave- & Alive-Infos oben in der Mitte
            using (var waveFont = new Font("Segoe Print", 28, FontStyle.Regular))
            {
                float emSize = waveFont.Size * g.DpiY / 72f;

                // --- Wave ---
                string waveText = $"Wave: {w.Round}";
                var waveSize = g.MeasureString(waveText, waveFont);
                float waveXPos = (screen.Width - waveSize.Width) / 2;
                float waveYPos = 20f;
                using (var path = new GraphicsPath())
                {
                    path.AddString(
                        waveText,
                        waveFont.FontFamily,
                        (int)waveFont.Style,
                        emSize,
                        new PointF(waveXPos, waveYPos),
                        StringFormat.GenericDefault
                    );
                    using (var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                    g.FillPath(Brushes.White, path);
                }

                // --- Alive ---
                string aliveText = $"Alive: {w.AliveZombies}/{w.TotalZombiesThisWave}";
                var aliveSize = g.MeasureString(aliveText, waveFont);
                float aliveXPos = (screen.Width - aliveSize.Width) / 2;
                float aliveYPos = waveYPos + waveSize.Height + 8f;
                using (var path2 = new GraphicsPath())
                {
                    path2.AddString(
                        aliveText,
                        waveFont.FontFamily,
                        (int)waveFont.Style,
                        emSize,
                        new PointF(aliveXPos, aliveYPos),
                        StringFormat.GenericDefault
                    );
                    using (var pen2 = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen2, path2);
                    g.FillPath(Brushes.White, path2);
                }

                // --- Next Wave Timer (falls geplant) ---
                if (w.NextWaveScheduled)
                {
                    string timer = $"Next round in: {Math.Ceiling(w.NextWaveTimeRemaining)}s";
                    using (var fTimer = new Font("Segoe Print", 18, FontStyle.Regular))
                    using (var pathT = new GraphicsPath())
                    {
                        var sizeT = g.MeasureString(timer, fTimer);
                        float xT = (screen.Width - sizeT.Width) / 2;
                        float yT = aliveYPos + aliveSize.Height + 10f;
                        pathT.AddString(
                            timer,
                            fTimer.FontFamily,
                            (int)fTimer.Style,
                            fTimer.Size * g.DpiY / 72f,
                            new PointF(xT, yT),
                            StringFormat.GenericDefault
                        );
                        using (var penT = new Pen(Color.Black, 4) { LineJoin = LineJoin.Round })
                            g.DrawPath(penT, pathT);
                        g.FillPath(Brushes.White, pathT);
                    }
                }
            }

            // 4) Weapon-Icon
            int weaponY = screen.Height - weaponYOffset;
            g.FillRectangle(Brushes.Yellow, weaponX, weaponY, weaponIconSize, weaponIconSize);
            using (var f2 = new Font("Segoe Print", 16, FontStyle.Regular))
                g.DrawString(
                    (p.GetCurrentWeaponIndex() + 1).ToString(),
                    f2,
                    Brushes.Black,
                    weaponX + (weaponIconSize / 4),
                    weaponY + 2
                );

            // 5) Minimap
            int minimapX = screen.Width - minimapSize - minimapOffsetRight;
            int minimapY = minimapOffsetTop;
            g.FillRectangle(Brushes.Black, minimapX, minimapY, minimapSize, minimapSize);
            g.DrawRectangle(Pens.White, minimapX, minimapY, minimapSize, minimapSize);

            float scaleX = minimapSize / (float)w.Map.Width;
            float scaleY = minimapSize / (float)w.Map.Height;

            // – Spieler
            float px = minimapX + p.Position.X * scaleX;
            float py = minimapY + p.Position.Y * scaleY;
            g.FillEllipse(Brushes.White, px - 3, py - 3, 6, 6);

            // – Zombies
            foreach (var z in w.Zombies)
            {
                float zx = minimapX + z.Position.X * scaleX;
                float zy = minimapY + z.Position.Y * scaleY;
                g.FillEllipse(Brushes.Red, zx - 2, zy - 2, 4, 4);
            }
        }

        public static void DrawPause(Graphics g, Size screen)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var b = new SolidBrush(Color.FromArgb(160, Color.Black)))
                g.FillRectangle(b, 0, 0, screen.Width, screen.Height);

            using (var f = new Font("Segoe Print", 48, FontStyle.Regular))
            {
                string txt = "PAUSED";
                var sz = g.MeasureString(txt, f);
                g.DrawString(
                    txt,
                    f,
                    Brushes.White,
                    (screen.Width - sz.Width) / 2,
                    (screen.Height - sz.Height) / 2
                );
            }
        }

        public static void DrawInventory(Graphics g, Player p, Size screen)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int invW = 400, invH = 300;
            int invX = (screen.Width - invW) / 2;
            int invY = (screen.Height - invH) / 2;

            using (var b = new SolidBrush(Color.FromArgb(200, Color.Gray)))
                g.FillRectangle(b, invX, invY, invW, invH);

            g.DrawRectangle(Pens.White, invX, invY, invW, invH);

            using (var f = new Font("Segoe Print", 18, FontStyle.Regular))
            {
                var inv = p.GetInventory();
                for (int i = 0; i < inv.Count; i++)
                    g.DrawString(
                        $"{i + 1}: {inv[i]}",
                        f,
                        Brushes.White,
                        invX + 20,
                        invY + 20 + i * 30
                    );
            }
        }
    }
}
