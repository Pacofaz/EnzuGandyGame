using Edward;
using System;
using System.Collections.Generic;
using System.Drawing;
using ZombieGame.Entities;
using ZombieGame.Utils;

namespace ZombieGame.Managers
{
    /// <summary>
    /// Verwalter für Gegnerwellen (Runden): Spawnt Zombies (inkl. Fernkampf), zählt Kills, regelt Pausen und Übergänge.
    /// </summary>
    public class WaveManager
    {
        private readonly List<Zombie> _zombies;
        private readonly Map _map;
        private readonly Player _player;
        private readonly Random _rnd = new Random();

        // Wellen-Infos und öffentliche Properties für UI
        public int Round { get; private set; }
        public int TotalZombiesThisWave => Round * 10;
        public int AliveZombies => _zombies.Count;
        public IReadOnlyList<Zombie> Zombies => _zombies;
        public Map Map => _map;

        private int _toSpawn;
        private int _kills;

        // Delay/Timing-Felder für Spawn- und Pausenlogik
        private int _initialDelayCounter;
        private bool _initialDelayDone;
        private int _spawnIntervalCounter;
        private const int SpawnIntervalFrames = 10;
        private int _nextWaveDelayCounter;
        private bool _nextWaveScheduled;
        private readonly List<Projectile> _projectiles; // Für Range-Zombies

        // Zeitkonstanten (Frames @ 60FPS)
        private const int InitialDelayFrames = 60 * 1;
        private const int NextWaveDelayFrames = 60 * 5;

        // Für das UI: Status und Restzeit bis nächste Welle
        public bool NextWaveScheduled => _nextWaveScheduled;
        public float NextWaveTimeRemaining =>
            !_nextWaveScheduled
                ? 0f
                : (NextWaveDelayFrames - _nextWaveDelayCounter) / 60f;

        /// <summary>
        /// Konstruktor: bekommt Listen, Map, Player und Projektil-Referenz.
        /// </summary>
        public WaveManager(List<Zombie> zombies, Map map, Player player, List<Projectile> projectiles)
        {
            _zombies = zombies;
            _map = map;
            _player = player;
            _projectiles = projectiles;
            StartNextWave();
        }

        /// <summary>
        /// Startet eine neue Welle: zählt hoch, setzt Spawnzähler und Resets.
        /// </summary>
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

        /// <summary>
        /// Hauptupdate: Spawnt Zombies, regelt Delays, prüft auf Rundenende und triggert neue Welle.
        /// </summary>
        public void Update()
        {
            // Tote entfernen und Kills zählen
            for (int i = _zombies.Count - 1; i >= 0; i--)
            {
                if (_zombies[i].IsDead)
                {
                    _zombies.RemoveAt(i);
                    _kills++;
                }
            }

            // Initialer Delay vor Wellenbeginn
            if (!_initialDelayDone)
            {
                if (_initialDelayCounter < InitialDelayFrames)
                    _initialDelayCounter++;
                else
                    _initialDelayDone = true;
            }

            // Zombies kontinuierlich spawnen
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

            // Wenn alles tot und gespawnt: Nächste Welle planen nach Delay
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

        /// <summary>
        /// Spawnt einen Zombie am Kartenrand (25% Chance Range-Zombie, Rest Nahkampf).
        /// Platziert Gegner nie direkt auf Spieler.
        /// </summary>
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

            if (_rnd.NextDouble() < 0.25)
                _zombies.Add(new ZombieRange(pos, _player, _projectiles));
            else
                _zombies.Add(new Zombie(pos, _player));
        }
    }
}
