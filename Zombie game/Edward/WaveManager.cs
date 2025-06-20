using Edward;
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

        public int Round { get; private set; }
        public int TotalZombiesThisWave => Round * 10;
        public int AliveZombies => _zombies.Count;
        public IReadOnlyList<Zombie> Zombies => _zombies;
        public Map Map => _map;

        private int _toSpawn;
        private int _kills;

        private int _initialDelayCounter;
        private bool _initialDelayDone;

        private int _spawnIntervalCounter;
        private const int SpawnIntervalFrames = 10;  // alle ~0.17s ein Zombie

        private int _nextWaveDelayCounter;
        private bool _nextWaveScheduled;
        private readonly List<Projectile> _projectiles; // <-- NEU!


        private const int InitialDelayFrames = 60 * 1;   // 5s @60FPS
        private const int NextWaveDelayFrames = 60 * 5;  // 10s @60FPS

        public bool NextWaveScheduled => _nextWaveScheduled;
        public float NextWaveTimeRemaining =>
            !_nextWaveScheduled
                ? 0f
                : (NextWaveDelayFrames - _nextWaveDelayCounter) / 60f;


        public WaveManager(List<Zombie> zombies, Map map, Player player, List<Projectile> projectiles)
        {
            _zombies = zombies;
            _map = map;
            _player = player;
            _projectiles = projectiles; // NEU!
            StartNextWave();
        }

        private void StartNextWave()
        {
            Round++;
            _toSpawn = TotalZombiesThisWave;
            _kills = 0;
            _initialDelayCounter = 0;
            _initialDelayDone = false;
            _spawnIntervalCounter = 0;
            _nextWaveDelayCounter = 0;
            _nextWaveScheduled = false;
        }

        public void Update()
        {
            // 1) Tote entfernen und zählen
            for (int i = _zombies.Count - 1; i >= 0; i--)
            {
                if (_zombies[i].IsDead)
                {
                    _zombies.RemoveAt(i);
                    _kills++;
                }
            }

            // 2) Initial-Delay vor erstem Spawn
            if (!_initialDelayDone)
            {
                if (_initialDelayCounter < InitialDelayFrames)
                    _initialDelayCounter++;
                else
                    _initialDelayDone = true;
            }

            // 3) kontinuierliches Spawnen
            if (_initialDelayDone && _toSpawn > 0)
            {
                if (_spawnIntervalCounter < SpawnIntervalFrames)
                    _spawnIntervalCounter++;
                else
                {
                    SpawnOne();
                    _spawnIntervalCounter = 0;
                    _toSpawn--;
                }
            }

            // 4) Nach Wellenende: Next-Wave-Delay
            if (_toSpawn <= 0 && _zombies.Count == 0)
            {
                if (!_nextWaveScheduled)
                    _nextWaveScheduled = true;
                else if (_nextWaveDelayCounter < NextWaveDelayFrames)
                    _nextWaveDelayCounter++;
                else
                    StartNextWave();
            }
        }

        private void SpawnOne()
        {
            var size = new SizeF(28, 28);
            PointF pos;
            RectangleF rect;
            do
            {
                int edge = _rnd.Next(4);
                float x = (float)_rnd.NextDouble() * (_map.Width - size.Width);
                float y = (float)_rnd.NextDouble() * (_map.Height - size.Height);
                switch (edge)
                {
                    case 0: y = 0; break;
                    case 1: y = _map.Height - size.Height; break;
                    case 2: x = 0; break;
                    default: x = _map.Width - size.Width; break;
                }
                pos = new PointF(x, y);
                rect = new RectangleF(pos, size);
            }
            while (rect.IntersectsWith(new RectangleF(_player.Position, _player.Size)));

            // NEU: 25% Chance für Range-Zombie, Rest normale Zombies
            if (_rnd.NextDouble() < 0.25)
                _zombies.Add(new ZombieRange(pos, _player, _projectiles));
            else
                _zombies.Add(new Zombie(pos, _player));
        }

    }
}
