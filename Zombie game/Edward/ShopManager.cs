// Datei: Managers/ShopManager.cs
using System;
using System.Collections.Generic;
using ZombieGame.Entities;

namespace ZombieGame.Managers
{
    public class ShopManager
    {
        public class ShopItem
        {
            public string Name { get; }
            public int Price { get; }
            public Action<Player> OnPurchase { get; }

            public ShopItem(string name, int price, Action<Player> onPurchase)
            {
                Name = name;
                Price = price;
                OnPurchase = onPurchase;
            }
        }

        private readonly List<ShopItem> _items;
        public IReadOnlyList<ShopItem> Items => _items;

        public ShopManager()
        {
            _items = new List<ShopItem>
            {
                new ShopItem("Health Pack", 50, p => p.Inventory.AddHealthPacks(1)),
                new ShopItem("Pistol Ammo (10)", 20, p => p.Inventory.AddAmmo("Pistol", 10)),
                new ShopItem("Rifle Ammo (10)", 40, p => p.Inventory.AddAmmo("Rifle", 10)),
                new ShopItem("Shotgun", 200, p => p.AddWeapon("Shotgun"))
            };
        }
    }
}
