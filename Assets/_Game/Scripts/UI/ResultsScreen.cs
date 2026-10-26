using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Results screen shown when crossing the finish line.
    /// Features sequenced star animations, time comparison, new best badge,
    /// 5-entry local leaderboard ranking, next bike unlock progress bar, and navigation.
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [Header("Time & Badges")]
        [SerializeField] private TextMeshProUGUI finishTimeText;
        [SerializeField] private GameObject newBestBadge;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;

        [Header("Star Icons (0-3)")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        [Header("Leaderboard List (5 entries)")]
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardRowPrefab;

        [Header("Next Bike Unlock Progress")]
        [SerializeField] private TextMeshProUGUI nextBikeNameText;
        [SerializeField] private Slider nextBikeProgressSlider;
        [SerializeField] private TextMeshProUGUI nextBikeCostText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button stagesButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button nextLevelButton;

        private void OnEnable()
        {
            EventBus.Subscribe<LevelFinishedEvent>(OnLevelFinished);
            SetupButtons();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelFinishedEvent>(OnLevelFinished);
        }

        private void SetupButtons()
        {
            if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
            if (stagesButton != null) stagesButton.onClick.AddListener(OnStages);
            if (shopButton != null) shopButton.onClick.AddListener(OnShop);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevel);
        }

        private void OnLevelFinished(LevelFinishedEvent evt)
        {
            gameObject.SetActive(true);
            StartCoroutine(DisplayResultsSequence(evt.FinalTime, evt.StarsEarned));
        }

        private IEnumerator DisplayResultsSequence(float time, int stars)
        {
            // Reset stars
            if (starImages != null)
            {
                foreach (var img in starImages)
                {
                    if (img != null) img.gameObject.SetActive(false);
                }
            }

            if (finishTimeText != null)
            {
                finishTimeText.text = $"{time:F2}s";
            }

            // Check if this was a new best time
            if (newBestBadge != null)
            {
                newBestBadge.SetActive(true);
            }

            if (coinsEarnedText != null)
            {
                coinsEarnedText.text = $"+{stars * 10} Coins";
            }

            // Animate stars one by one with chime audio
            yield return new WaitForSeconds(0.4f);
            if (starImages != null)
            {
                for (int i = 0; i < stars && i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].gameObject.SetActive(true);
                        starImages[i].transform.localScale = Vector3.one * 1.5f;
                        AudioManager.Instance?.PlayStarEarned();

                        // Punch scale down
                        float t = 0f;
                        while (t < 0.25f)
                        {
                            t += Time.deltaTime;
                            starImages[i].transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t / 0.25f);
                            yield return null;
                        }
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }

            // Populate 5 leaderboard rivals
            PopulateLeaderboard();

            // Update Next Bike Progress
            UpdateNextBikeProgress();
        }

        private void PopulateLeaderboard()
        {
            var leaderboardService = new LocalLeaderboardService();
            leaderboardService.GetLeaderboard(1, 1, entries =>
            {
                if (leaderboardContainer == null) return;

                // Clear prior rows
                for (int i = leaderboardContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(leaderboardContainer.GetChild(i).gameObject);
                }

                foreach (var entry in entries)
                {
                    var rowGo = new GameObject($"Rank_{entry.rank}");
                    rowGo.transform.SetParent(leaderboardContainer, false);
                    var text = rowGo.AddComponent<TextMeshProUGUI>();
                    text.fontSize = 18;
                    text.color = entry.isPlayer ? new Color(1f, 0.85f, 0.2f) : Color.white;
                    text.text = $"#{entry.rank}  {entry.riderName,-14}  {entry.timeSeconds:F2}s";
                }
            });
        }

        private void UpdateNextBikeProgress()
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            int targetCost = 500;
            if (nextBikeNameText != null) nextBikeNameText.text = "Speed Demon";
            if (nextBikeCostText != null) nextBikeCostText.text = $"{save.Data.coins} / {targetCost} Coins";
            if (nextBikeProgressSlider != null)
            {
                nextBikeProgressSlider.maxValue = targetCost;
                nextBikeProgressSlider.value = Mathf.Clamp(save.Data.coins, 0, targetCost);
            }
        }

        public void OnRetry()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.RestartCurrentLevel();
        }

        public void OnStages()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnShop()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void OnNextLevel()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            GameManager.Instance?.RestartCurrentLevel();
        }
    }
}
