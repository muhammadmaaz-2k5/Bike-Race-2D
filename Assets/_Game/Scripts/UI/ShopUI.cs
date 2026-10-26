using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirtDash.Progression;
using DirtDash.Bike;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Garage / Shop UI interface displaying bike preview stats, coin costs,
    /// and purchase/equip action buttons.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Bike Catalog References")]
        [SerializeField] private ShopManager shopManager;

        [Header("Preview UI Labels")]
        [SerializeField] private TextMeshProUGUI bikeNameText;
        [SerializeField] private TextMeshProUGUI bikeDescText;
        [SerializeField] private TextMeshProUGUI speedStatText;
        [SerializeField] private TextMeshProUGUI torqueStatText;
        [SerializeField] private TextMeshProUGUI gripStatText;
        [SerializeField] private TextMeshProUGUI agilityStatText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI playerCoinsText;

        [Header("Action Buttons")]
        [SerializeField] private Button actionButton; // "BUY" or "EQUIP" or "EQUIPPED"
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [SerializeField] private Button nextBikeButton;
        [SerializeField] private Button prevBikeButton;
        [SerializeField] private Button closeButton;

        private int _currentBikeIndex = 0;

        private void OnEnable()
        {
            if (shopManager == null) shopManager = ShopManager.Instance;
            SetupListeners();
            RefreshView();
        }

        private void SetupListeners()
        {
            if (nextBikeButton != null) nextBikeButton.onClick.AddListener(NextBike);
            if (prevBikeButton != null) prevBikeButton.onClick.AddListener(PrevBike);
            if (actionButton != null) actionButton.onClick.AddListener(OnActionButtonClicked);
            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void RefreshView()
        {
            if (shopManager == null || shopManager.BikeCatalog.Count == 0) return;

            _currentBikeIndex = Mathf.Clamp(_currentBikeIndex, 0, shopManager.BikeCatalog.Count - 1);
            BikeStats bike = shopManager.BikeCatalog[_currentBikeIndex];

            if (bikeNameText != null) bikeNameText.text = bike.displayName;
            if (bikeDescText != null) bikeDescText.text = bike.description;

            // Stats comparison
            if (speedStatText != null) speedStatText.text = $"Speed: {bike.maxSpeed:F0} m/s";
            if (torqueStatText != null) torqueStatText.text = $"Power: {bike.motorTorque:F0} Nm";
            if (gripStatText != null) gripStatText.text = $"Tire Grip: {bike.tireFriction:F2}";
            if (agilityStatText != null) agilityStatText.text = $"Air Agility: {bike.airRotationTorque:F0}";

            // Economy & Action Button State
            bool isOwned = shopManager.IsBikeOwned(bike.bikeId);
            bool isEquipped = shopManager.IsBikeEquipped(bike.bikeId);

            var save = SaveSystem.Instance;
            if (playerCoinsText != null && save != null) playerCoinsText.text = $"{save.Data.coins}";

            if (isEquipped)
            {
                if (actionButtonText != null) actionButtonText.text = "EQUIPPED";
                if (priceText != null) priceText.text = "Active Bike";
                if (actionButton != null) actionButton.interactable = false;
            }
            else if (isOwned)
            {
                if (actionButtonText != null) actionButtonText.text = "EQUIP";
                if (priceText != null) priceText.text = "Owned";
                if (actionButton != null) actionButton.interactable = true;
            }
            else
            {
                if (actionButtonText != null) actionButtonText.text = "BUY";
                if (priceText != null) priceText.text = $"{bike.coinPrice} Coins";
                if (actionButton != null)
                {
                    actionButton.interactable = save != null && save.Data.coins >= bike.coinPrice;
                }
            }
        }

        public void NextBike()
        {
            if (shopManager == null) return;
            _currentBikeIndex = (_currentBikeIndex + 1) % shopManager.BikeCatalog.Count;
            AudioManager.Instance?.PlayButtonClick();
            RefreshView();
        }

        public void PrevBike()
        {
            if (shopManager == null) return;
            _currentBikeIndex = (_currentBikeIndex - 1 + shopManager.BikeCatalog.Count) % shopManager.BikeCatalog.Count;
            AudioManager.Instance?.PlayButtonClick();
            RefreshView();
        }

        private void OnActionButtonClicked()
        {
            if (shopManager == null) return;
            BikeStats bike = shopManager.BikeCatalog[_currentBikeIndex];

            if (shopManager.IsBikeOwned(bike.bikeId))
            {
                shopManager.EquipBike(bike.bikeId);
            }
            else
            {
                shopManager.PurchaseBike(bike);
            }

            RefreshView();
        }
    }
}
