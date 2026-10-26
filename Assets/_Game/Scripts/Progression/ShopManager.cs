using System;
using System.Collections.Generic;
using UnityEngine;
using DirtDash.Bike;
using DirtDash.Core;

namespace DirtDash.Progression
{
    /// <summary>
    /// Garage & Shop manager handling bike purchases, equipping, and stat previews.
    /// Supports at least 6 unique bikes with distinct speed, grip, and balance profiles.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("Available Bike Catalog")]
        [SerializeField] private List<BikeStats> bikeCatalog = new();

        public List<BikeStats> BikeCatalog => bikeCatalog;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsBikeOwned(string bikeId)
        {
            var save = SaveSystem.Instance;
            return save != null && save.Data.ownedBikeIds.Contains(bikeId);
        }

        public bool IsBikeEquipped(string bikeId)
        {
            var save = SaveSystem.Instance;
            return save != null && save.Data.selectedBikeId == bikeId;
        }

        public bool PurchaseBike(BikeStats stats)
        {
            if (stats == null) return false;
            var save = SaveSystem.Instance;
            if (save == null) return false;

            if (IsBikeOwned(stats.bikeId)) return true;

            if (save.SpendCoins(stats.coinPrice))
            {
                save.Data.ownedBikeIds.Add(stats.bikeId);
                save.Data.selectedBikeId = stats.bikeId; // auto-equip
                save.Save();
                AudioManager.Instance?.PlayWinJingle();
                Debug.Log($"[Dirt Dash] Purchased and equipped bike: {stats.displayName}");
                return true;
            }

            return false;
        }

        public void EquipBike(string bikeId)
        {
            var save = SaveSystem.Instance;
            if (save != null && IsBikeOwned(bikeId))
            {
                save.Data.selectedBikeId = bikeId;
                save.Save();
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        public BikeStats GetEquippedBikeStats()
        {
            var save = SaveSystem.Instance;
            string selectedId = save != null ? save.Data.selectedBikeId : "bike_dirtrunner";
            return bikeCatalog.Find(b => b.bikeId == selectedId) ?? (bikeCatalog.Count > 0 ? bikeCatalog[0] : null);
        }
    }
}
