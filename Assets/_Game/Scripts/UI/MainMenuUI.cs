using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Main menu controller managing navigation to Single Player, Tasks, Settings, Shop,
    /// and locked Multiplayer / Tournament stubs.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button tasksButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button multiplayerButton;
        [SerializeField] private Button tournamentsButton;

        [Header("Modal Panels")]
        [SerializeField] private GameObject tasksPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject lockedNoticePopup;

        [Header("Player Economy Display")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI starsText;

        private void Start()
        {
            SetupButtons();
            UpdateEconomyHeader();
        }

        private void SetupButtons()
        {
            if (singlePlayerButton != null) singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
            if (tasksButton != null) tasksButton.onClick.AddListener(OnTasksClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
            if (multiplayerButton != null) multiplayerButton.onClick.AddListener(OnLockedFeatureClicked);
            if (tournamentsButton != null) tournamentsButton.onClick.AddListener(OnLockedFeatureClicked);
        }

        public void OnSinglePlayerClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("WorldSelect");
        }

        public void OnTasksClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (tasksPanel != null) tasksPanel.SetActive(true);
        }

        public void OnSettingsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnShopClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (shopPanel != null) shopPanel.SetActive(true);
        }

        public void OnLockedFeatureClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (lockedNoticePopup != null)
            {
                lockedNoticePopup.SetActive(true);
            }
        }

        public void ClosePanels()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (tasksPanel != null) tasksPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (lockedNoticePopup != null) lockedNoticePopup.SetActive(false);
            UpdateEconomyHeader();
        }

        public void UpdateEconomyHeader()
        {
            var save = SaveSystem.Instance;
            if (save != null)
            {
                if (coinsText != null) coinsText.text = $"{save.Data.coins}";
                if (starsText != null) starsText.text = $"{save.Data.totalStars} Stars";
            }
        }
    }
}
