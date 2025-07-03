// Core/GameController.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using ZombieGame.Entities;

namespace ZombieGame.Core
{
    public enum GameState { StartMenu, Playing, Shop, GameOver }

    public class GameManager
    {
        public GameState State { get; private set; } = GameState.StartMenu;
        public Player Player { get; }
        public List<Entity> Entities { get; } = new();
        public List<Projectile> Projectiles { get; } = new();
        private readonly WaveManager waveMgr;
        private readonly ShopManager shopMgr;

        public GameManager()
        {
            Player = new Player(new PointF(400, 300), 5f, Entities, Projectiles);
            waveMgr = new WaveManager(Entities, Player, Projectiles);
            shopMgr = new ShopManager(Player);
        }

        public void ChangeState(GameState ns) => State = ns;

        public void Update()
        {
            switch (State)
            {
                case GameState.Playing:
                    Player.Update();
                    Entities.ForEach(e => e.Update());
                    Projectiles.ForEach(p => p.Update());
                    Entities.RemoveAll(e => e is Zombie z && z.IsDead);
                    Projectiles.RemoveAll(p => p.IsExpired);
                    waveMgr.Update();
                    break;
                case GameState.Shop:
                    shopMgr.Update();
                    break;
            }
        }

        public void Draw(Graphics g)
        {
            if (State == GameState.StartMenu) return;
            Entities.ForEach(e => e.Draw(g));
            Projectiles.ForEach(p => p.Draw(g));
            Player.Draw(g);
            if (State == GameState.Shop) shopMgr.Draw(g);
        }
    }

    public class WaveManager
    {
        private readonly List<Entity> ents;
        private readonly Player player;
        private readonly List<Projectile> projs;
        private int wave, spawned;

        public WaveManager(List<Entity> ents, Player player, List<Projectile> projs)
            => (this.ents, this.player, this.projs) = (ents, player, projs);

        public void Update()
        {
            if (ents.FindAll(e => e is Zombie).Count == 0)
            {
                wave++; spawned = 0;
            }
            if (spawned < wave * 5)
            {
                ents.Add(new Zombie(RandomPos(), player, projs));
                spawned++;
            }
        }

        private PointF RandomPos()
            => new(Random.Shared.Next(0, 800), Random.Shared.Next(0, 600));
    }

    public class ShopManager
    {
        private readonly Player player;
        public ShopManager(Player p) => player = p;
        public void Update() { /* Input-Handling */ }
        public void Draw(Graphics g)
        {
            g.DrawString($"Gold: {player.Gold}", new Font("Arial", 16), Brushes.Yellow, 10, 10);
            // Buttons & Items...
        }
    }
}
