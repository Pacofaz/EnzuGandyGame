
using System;
using System.Collections.Generic;
using System.Drawing;
using ZombieGame.Entities;
using ZombieGame.Utils;

namespace ZombieGame.Managers
{
    public class WaveManager
    {
        private readonly List<Zombie> _zombies;
        private readonly Map _map;
        private readonly Player _player;
        private readonly Random _rnd = new Random();
        private int _roundCount;
        private int _toSpawn;
        private int _kills;

        public WaveManager(List<Zombie> zombies, Map map, Player player)
        {
            _zombies = zombies;
            _map = map;
            _player = player;
            NextWave();
        }

        private void NextWave()
        {
            _roundCount++;
            _kills = 0;
            _toSpawn = _roundCount * 10;
        }

        public void Update()
        {
            for (int i = _zombies.Count - 1; i >= 0; i--)
            {
                if (_zombies[i].IsDead)
                {
                    _zombies.RemoveAt(i);
                    _kills++;
                }
            }

            if (_toSpawn > 0 && _zombies.Count < _roundCount * 10)
            {
                float x = (float)_rnd.NextDouble() * _map.Width;
                float y = (float)_rnd.NextDouble() * _map.Height;
                _zombies.Add(new Zombie(new PointF(x, y), _player));
                _toSpawn--;
            }

            if (_toSpawn == 0 && _zombies.Count == 0)
                NextWave();
        }

        public IReadOnlyList<Zombie> Zombies => _zombies;
        public Map Map => _map;
        public int Round => _roundCount;
        public int AliveZombies => _zombies.Count;
    }
}
