using System.Collections.Generic;

namespace ZombieGame.Utils
{
    public class Inventory
    {
        public Dictionary<string, int> Ammo { get; }
        public List<string> Weapons { get; }
        public int HealthPacks { get; private set; }

        public Inventory()
        {
            Ammo = new Dictionary<string, int>();
            Weapons = new List<string>();
            HealthPacks = 0;
        }

        public void AddWeapon(string weapon)
        {
            if (!Weapons.Contains(weapon))
            {
                Weapons.Add(weapon);
                if (!Ammo.ContainsKey(weapon))
                    Ammo[weapon] = 0;
            }
        }

        public void AddAmmo(string weapon, int amount)
        {
            if (!Ammo.ContainsKey(weapon))
                Ammo[weapon] = 0;
            Ammo[weapon] += amount;
        }

        public void AddHealthPacks(int count) => HealthPacks += count;

        public bool UseHealthPack()
        {
            if (HealthPacks <= 0) return false;
            HealthPacks--;
            return true;
        }

        public void Clear()
        {
            Weapons.Clear();
            Ammo.Clear();
            HealthPacks = 0;
        }
    }
}
