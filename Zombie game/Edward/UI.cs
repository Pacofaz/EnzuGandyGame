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
    /// <summary>
    /// Statische Klasse für das komplette UI-Rendering (Healthbar, Welle, Hotbar, Minimap, Pause, Inventar, etc.).
    /// </summary>
    public static class UI
    {
        private static readonly Bitmap HealthBarOutlineImage;

        /// <summary>
        /// Lädt benötigte UI-Assets einmalig (z.B. Healthbar-Overlay).
        /// </summary>
        static UI()
        {
            var path = Path.Combine(Application.StartupPath, "Assets", "healthbar_outline.png");
            if (!File.Exists(path))
                throw new FileNotFoundException("healthbar_outline.png nicht gefunden.", path);
            HealthBarOutlineImage = new Bitmap(path);
        }

        /// <summary>
        /// Zentraler UI-Renderer für das Spiel: Healthbar, Runde, Wellen, Hotbar, Money, Minimap, uvm.
        /// Werte und Positionen sind teilweise als Parameter steuerbar.
        /// </summary>
        public static void DrawGame(
            Graphics g,
            Player p,
            WaveManager w,
            Size screen,
            // Healthbar-Position und Größe
            int barX = 100, int barY = 75, int barWidth = 210, int barHeight = 30,
            // Outline-Overlay
            int outlineWidth = 350, int outlineHeight = 180, int outlineOffsetX = -42, int outlineOffsetY = -12,
            // Weapon icon
            int weaponX = 20, int weaponIconSize = 40, int weaponYOffset = 70,
            // Minimap
            int minimapSize = 150, int minimapOffsetRight = 20, int minimapOffsetTop = 20
        )
        {
            // --- Healthbar ---
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.FillRectangle(Brushes.Gray, barX, barY, barWidth, barHeight);
            g.FillRectangle(Brushes.Lime, barX, barY, 2 * p.Health, barHeight);

            // Healthbar-Text mit Outline
            using (var fH = new Font("Segoe Print", 16, FontStyle.Regular))
            {
                string txt = $"{p.Health}/100";
                float em = fH.Size * g.DpiY / 72f;
                var pos = new PointF(barX + barWidth + 10, barY - 2);
                using (var path = new GraphicsPath())
                {
                    path.AddString(txt, fH.FontFamily, (int)fH.Style, em, pos, StringFormat.GenericDefault);
                    using (var pen = new Pen(Color.Black, 4) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                    g.FillPath(Brushes.White, path);
                }
            }
            // Healthbar-Overlay
            int ox = barX + (barWidth - outlineWidth) / 2 + outlineOffsetX;
            int oy = barY - ((outlineHeight - barHeight) / 2) + outlineOffsetY;
            g.DrawImage(HealthBarOutlineImage, ox, oy, outlineWidth, outlineHeight);

            // --- Wave-Info, Alive-Counter, Countdown ---
            using (var fW = new Font("Segoe Print", 28, FontStyle.Regular))
            {
                float em = fW.Size * g.DpiY / 72f;
                string wtxt = $"Wave: {w.Round}";
                var szW = g.MeasureString(wtxt, fW);
                var pW = new PointF((screen.Width - szW.Width) / 2, 20f);
                using (var path = new GraphicsPath())
                {
                    path.AddString(wtxt, fW.FontFamily, (int)fW.Style, em, pW, StringFormat.GenericDefault);
                    using (var pen = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                    g.FillPath(Brushes.White, path);
                }
                string atxt = $"Alive: {w.AliveZombies}/{w.TotalZombiesThisWave}";
                var szA = g.MeasureString(atxt, fW);
                var pA = new PointF(pW.X, pW.Y + szW.Height + 8f);
                using (var path2 = new GraphicsPath())
                {
                    path2.AddString(atxt, fW.FontFamily, (int)fW.Style, em, pA, StringFormat.GenericDefault);
                    using (var pen2 = new Pen(Color.Black, 6) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen2, path2);
                    g.FillPath(Brushes.White, path2);
                }
                if (w.NextWaveScheduled)
                {
                    string tmr = $"Next round in: {Math.Ceiling(w.NextWaveTimeRemaining)}s";
                    using (var fT = new Font("Segoe Print", 18, FontStyle.Regular))
                    using (var pth = new GraphicsPath())
                    {
                        float emT = fT.Size * g.DpiY / 72f;
                        var szT = g.MeasureString(tmr, fT);
                        var posT = new PointF((screen.Width - szT.Width) / 2, pA.Y + szA.Height + 10f);
                        pth.AddString(tmr, fT.FontFamily, (int)fT.Style, emT, posT, StringFormat.GenericDefault);
                        using (var penT = new Pen(Color.Black, 4) { LineJoin = LineJoin.Round })
                            g.DrawPath(penT, pth);
                        g.FillPath(Brushes.White, pth);
                    }
                }
            }

            // --- Aktuelle Waffenanzeige (links unten, simple Nummer + gelber Kasten) ---
            int wY = screen.Height - weaponYOffset;
            g.FillRectangle(Brushes.Yellow, weaponX, wY, weaponIconSize, weaponIconSize);
            using (var fw = new Font("Segoe Print", 16, FontStyle.Regular))
                g.DrawString((p.GetCurrentWeaponIndex() + 1).ToString(), fw, Brushes.Black, weaponX + weaponIconSize / 4, wY + 2);

            // --- Hotbar mit bis zu 5 Slots und Inventar ---
            var inv = p.GetInventory();
            int slots = 5, ss = 50, sp = 10;
            int totW = slots * ss + (slots - 1) * sp;
            int startX = (screen.Width - totW) / 2, startY = screen.Height - ss - 20;
            using (var fs = new Font("Segoe Print", 14, FontStyle.Regular))
            {
                for (int i = 0; i < slots; i++)
                {
                    var r = new Rectangle(startX + i * (ss + sp), startY, ss, ss);
                    g.FillRectangle(Brushes.DimGray, r);
                    using (var pen = new Pen(i == p.GetCurrentWeaponIndex() ? Color.Yellow : Color.White, 2))
                        g.DrawRectangle(pen, r);
                    g.DrawString((i + 1).ToString(), fs, Brushes.White, r.X + 4, r.Y + 4);
                    if (i < inv.Count)
                    {
                        string nm = inv[i];
                        var sz = g.MeasureString(nm, fs);
                        g.DrawString(nm, fs, Brushes.White, r.X + (ss - sz.Width) / 2, r.Y + ss - sz.Height - 4);
                    }
                }
            }

            // --- Minimap (rechts oben) mit Spieler & Zombies ---
            int mX = screen.Width - minimapSize - minimapOffsetRight;
            int mY = minimapOffsetTop;
            g.FillRectangle(Brushes.Black, mX, mY, minimapSize, minimapSize);
            g.DrawRectangle(Pens.White, mX, mY, minimapSize, minimapSize);
            float scX = minimapSize / (float)w.Map.Width, scY = minimapSize / (float)w.Map.Height;
            g.FillEllipse(Brushes.White, mX + p.Position.X * scX - 3, mY + p.Position.Y * scY - 3, 6, 6);
            foreach (var z in w.Zombies)
                g.FillEllipse(Brushes.Red, mX + z.Position.X * scX - 2, mY + z.Position.Y * scY - 2, 4, 4);

            // --- Money unter Minimap mit Outline ---
            int moneyX = mX;
            int moneyY = minimapOffsetTop + minimapSize + 8;
            using (var fm = new Font("Segoe Print", 16, FontStyle.Regular))
            {
                string moneyTxt = $"Money: ${p.GetMoney()}";
                float em = fm.Size * g.DpiY / 72f;
                var pos = new PointF(moneyX, moneyY);
                using (var path = new GraphicsPath())
                {
                    path.AddString(moneyTxt, fm.FontFamily, (int)fm.Style, em, pos, StringFormat.GenericDefault);
                    using (var pen = new Pen(Color.Black, 4) { LineJoin = LineJoin.Round })
                        g.DrawPath(pen, path);
                    g.FillPath(Brushes.White, path);
                }
            }
        }

        /// <summary>
        /// Zeichnet den Pause-Bildschirm (dunkles Overlay + „PAUSED“).
        /// </summary>
        public static void DrawPause(Graphics g, Size screen)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (var o = new SolidBrush(Color.FromArgb(160, Color.Black)))
                g.FillRectangle(o, 0, 0, screen.Width, screen.Height);
            using (var f = new Font("Segoe Print", 48, FontStyle.Regular))
            {
                string txt = "PAUSED";
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, Brushes.White, (screen.Width - sz.Width) / 2, (screen.Height - sz.Height) / 2);
            }
        }

        /// <summary>
        /// Zeichnet das Inventar (zentriertes Overlay mit Waffen/Items).
        /// </summary>
        public static void DrawInventory(Graphics g, Player p, Size screen)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int iw = 400, ih = 300;
            int ix = (screen.Width - iw) / 2, iy = (screen.Height - ih) / 2;
            using (var bg = new SolidBrush(Color.FromArgb(200, Color.Gray)))
                g.FillRectangle(bg, ix, iy, iw, ih);
            g.DrawRectangle(Pens.White, ix, iy, iw, ih);
            using (var f = new Font("Segoe Print", 18, FontStyle.Regular))
            {
                var lst = p.GetInventory();
                for (int i = 0; i < lst.Count; i++)
                    g.DrawString($"{i + 1}: {lst[i]}", f, Brushes.White, ix + 20, iy + 20 + i * 30);
            }
        }
    }
}
