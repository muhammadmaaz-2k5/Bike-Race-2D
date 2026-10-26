using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirtDash.Core;
using DirtDash.Cameras;

namespace DirtDash.UI
{
    /// <summary>
    /// In-game HUD controller managing race timer, pause button, camera zoom button,
    /// touch-to-start banner, control tutorial banner, and float popups (+Flip, +Coins).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("UI Text & Labels")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private GameObject touchToStartBanner;
        [SerializeField] private GameObject tutorialBanner;
        [SerializeField] private TextMeshProUGUI bannerNotificationText;
        [SerializeField] private GameObject notificationPanel;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button zoomButton;
        [SerializeField] private Button quickShopButton;

        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject resultsScreenPanel;

        [Header("Camera Reference")]
        [SerializeField] private CameraFollow cameraFollow;

        private float _bannerTimer = 0f;
        private bool _hasStartedRace = false;

        private void OnEnable()
        {
            EventBus.Subscribe<BikeFlipEvent>(OnFlip);
            EventBus.Subscribe<LevelFinishedEvent>(OnFinished);
            EventBus.Subscribe<BikeCrashedEvent>(OnCrashed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BikeFlipEvent>(OnFlip);
            EventBus.Unsubscribe<LevelFinishedEvent>(OnFinished);
            EventBus.Unsubscribe<BikeCrashedEvent>(OnCrashed);
        }

        private void Start()
        {
            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }

            SetupButtons();
            ShowTouchToStart();
            UpdateCoinsDisplay();
        }

        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (zoomButton != null)
            {
                zoomButton.onClick.AddListener(OnZoomClicked);
            }

            if (quickShopButton != null)
            {
                quickShopButton.onClick.AddListener(OnQuickShopClicked);
            }
        }

        private void Update()
        {
            // Touch to start race detection
            if (!_hasStartedRace)
            {
                if (Input.anyKeyDown || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    StartRace();
                }
            }

            // Update timer
            var gm = GameManager.Instance;
            if (gm != null && gm.IsRaceActive && timerText != null)
            {
                bool fourDigits = SaveSystem.Instance == null || SaveSystem.Instance.Data.fourDigitTimes;
                if (fourDigits)
                {
                    int minutes = (int)(gm.RaceTimer / 60f);
                    float seconds = gm.RaceTimer % 60f;
                    timerText.text = $"{minutes:00}:{seconds:00.00}";
                }
                else
                {
                    timerText.text = $"{gm.RaceTimer:F1}s";
                }
            }

            // Notification banner fade out
            if (_bannerTimer > 0f)
            {
                _bannerTimer -= Time.deltaTime;
                if (_bannerTimer <= 0f && notificationPanel != null)
                {
                    notificationPanel.SetActive(false);
                }
            }
        }

        public void StartRace()
        {
            _hasStartedRace = true;
            if (touchToStartBanner != null) touchToStartBanner.SetActive(false);
            if (tutorialBanner != null) tutorialBanner.SetActive(false);

            GameManager.Instance?.StartRace();
            AudioManager.Instance?.StartEngineSound();
        }

        private void ShowTouchToStart()
        {
            if (touchToStartBanner != null) touchToStartBanner.SetActive(true);
            if (tutorialBanner != null)
            {
                tutorialBanner.SetActive(true);
                var tutText = tutorialBanner.GetComponentInChildren<TextMeshProUGUI>();
                if (tutText != null)
                {
                    tutText.text = Application.isMobilePlatform
                        ? Localization.Get("ui_tutorial_mobile")
                        : Localization.Get("ui_tutorial_desktop");
                }
            }
        }

        private void OnPauseClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            GameManager.Instance?.SetPaused(true);
        }

        private void OnZoomClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            cameraFollow?.ToggleFarView();
        }

        private void OnQuickShopClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            // Open garage / shop
        }

        private void OnFlip(BikeFlipEvent evt)
        {
            string flipName = evt.IsBackflip ? "BACKFLIP!" : "FRONTFLIP!";
            ShowNotification($"+{flipName} (+{evt.BonusScore} Coins)", 2.0f);
            SaveSystem.Instance?.AddCoins(evt.BonusScore);
            UpdateCoinsDisplay();
        }

        private void OnCrashed(BikeCrashedEvent evt)
        {
            ShowNotification($"CRASHED! Restarting...", 1.5f);
        }

        private void OnFinished(LevelFinishedEvent evt)
        {
            AudioManager.Instance?.StopEngineSound();
            if (resultsScreenPanel != null)
            {
                resultsScreenPanel.SetActive(true);
            }
        }

        public void ShowNotification(string text, float duration = 2.0f)
        {
            if (bannerNotificationText != null)
            {
                bannerNotificationText.text = text;
            }
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(true);
            }
            _bannerTimer = duration;
        }

        public void UpdateCoinsDisplay()
        {
            if (coinsText != null && SaveSystem.Instance != null)
            {
                coinsText.text = $"{SaveSystem.Instance.Data.coins}";
            }
        }
    }
}
