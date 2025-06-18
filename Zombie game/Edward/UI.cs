// File: Utils/UI.cs
using System.Drawing;
using ZombieGame.Entities;
using ZombieGame.Managers;

namespace ZombieGame.Utils
{
    public static class UI
    {
        public static void DrawGame(Graphics g, Player p, WaveManager w, Size screen)
        {
            // Healthbar
            g.FillRectangle(Brushes.Gray, 20, 20, 200, 20);
            g.FillRectangle(Brushes.Lime, 20, 20, 2 * p.Health, 20);
            using (var f = new Font("Arial", 16))
                g.DrawString($"{p.Health}/100", f, Brushes.White, 230, 18);

            // Welle + Zombies
            using (var f = new Font("Arial", 18))
            {
                g.DrawString("Wave: " + w.Round, f, Brushes.White, 20, 50);
                g.DrawString("Alive: " + w.AliveZombies, f, Brushes.White, 20, 75);
            }

            // Weapon-Icon
            g.FillRectangle(Brushes.Yellow, 20, screen.Height - 70, 40, 40);
            using (var f2 = new Font("Arial", 16))
                g.DrawString((p.GetCurrentWeaponIndex() + 1).ToString(),
                             f2, Brushes.Black, 28, screen.Height - 68);

            // Minimap
            const int minimapSize = 150;
            int minimapX = screen.Width - minimapSize - 20;
            int minimapY = 20;
            g.FillRectangle(Brushes.Black, minimapX, minimapY, minimapSize, minimapSize);
            g.DrawRectangle(Pens.White, minimapX, minimapY, minimapSize, minimapSize);

            float scaleX = minimapSize / (float)w.Map.Width;
            float scaleY = minimapSize / (float)w.Map.Height;

            // Spieler
            float px = minimapX + p.Position.X * scaleX;
            float py = minimapY + p.Position.Y * scaleY;
            g.FillEllipse(Brushes.White, px - 3, py - 3, 6, 6);

            // Zombies
            foreach (var z in w.Zombies)
            {
                float zx = minimapX + z.Position.X * scaleX;
                float zy = minimapY + z.Position.Y * scaleY;
                g.FillEllipse(Brushes.Red, zx - 2, zy - 2, 4, 4);
            }
        }

        public static void DrawPause(Graphics g, Size screen)
        {
            using (var b = new SolidBrush(Color.FromArgb(160, Color.Black)))
                g.FillRectangle(b, 0, 0, screen.Width, screen.Height);
            using (var f = new Font("Arial", 48, FontStyle.Bold))
            {
                string txt = "PAUSED";
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, Brushes.White,
                    (screen.Width - sz.Width) / 2, (screen.Height - sz.Height) / 2);
            }
        }

        public static void DrawInventory(Graphics g, Player p, Size screen)
        {
            using (var b = new SolidBrush(Color.FromArgb(200, Color.Gray)))
                g.FillRectangle(b,
                    (screen.Width - 400) / 2, (screen.Height - 300) / 2,
                    400, 300);
            g.DrawRectangle(Pens.White,
                (screen.Width - 400) / 2, (screen.Height - 300) / 2,
                400, 300);
            using (var f = new Font("Arial", 18))
            {
                var inv = p.GetInventory();
                for (int i = 0; i < inv.Count; i++)
                    g.DrawString($"{i + 1}: {inv[i]}", f, Brushes.White,
                        (screen.Width - 400) / 2 + 20,
                        (screen.Height - 300) / 2 + 20 + i * 30);
            }
        }
    }
}
