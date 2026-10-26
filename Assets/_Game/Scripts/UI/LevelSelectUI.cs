using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Level Select screen featuring a grid of 8 level buttons for the active world.
    /// Shows level number, 0-3 star rating, best time, and locked/unlocked state.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI worldTitleText;
        [SerializeField] private Button backButton;

        [Header("8 Level Buttons")]
        [SerializeField] private Button[] levelButtons = new Button[8];
        [SerializeField] private TextMeshProUGUI[] levelNumberTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] levelStarsTexts = new TextMeshProUGUI[8];
        [SerializeField] private TextMeshProUGUI[] levelBestTimeTexts = new TextMeshProUGUI[8];

        public static int SelectedLevelIndex = 1;

        private void Start()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBack);

            int worldId = WorldSelectUI.SelectedWorldId;
            if (worldTitleText != null)
            {
                string worldName = worldId == 1 ? "World 1: Hills" : (worldId == 2 ? "World 2: Desert" : "World 3: Arctic");
                worldTitleText.text = worldName;
            }

            PopulateLevelGrid(worldId);
        }

        private void PopulateLevelGrid(int worldId)
        {
            var save = SaveSystem.Instance;

            for (int i = 0; i < 8; i++)
            {
                int levelNum = i + 1;
                var record = save != null ? save.GetLevelRecord(worldId, levelNum) : new LevelRecord();

                // Button interaction and labels
                if (i < levelButtons.Length && levelButtons[i] != null)
                {
                    bool isUnlocked = record.isUnlocked || (worldId == 1 && levelNum == 1);
                    levelButtons[i].interactable = isUnlocked;

                    int capturedLevel = levelNum;
                    levelButtons[i].onClick.AddListener(() => LaunchLevel(worldId, capturedLevel));
                }

                if (i < levelNumberTexts.Length && levelNumberTexts[i] != null)
                {
                    levelNumberTexts[i].text = $"Stage {levelNum}";
                }

                if (i < levelStarsTexts.Length && levelStarsTexts[i] != null)
                {
                    string starStr = "";
                    for (int s = 0; s < 3; s++)
                    {
                        starStr += (s < record.starsEarned) ? "★" : "☆";
                    }
                    levelStarsTexts[i].text = starStr;
                }

                if (i < levelBestTimeTexts.Length && levelBestTimeTexts[i] != null)
                {
                    levelBestTimeTexts[i].text = (record.bestTimeSeconds < 900f) ? $"{record.bestTimeSeconds:F2}s" : "--:--";
                }
            }
        }

        public void LaunchLevel(int worldId, int levelNum)
        {
            AudioManager.Instance?.PlayButtonClick();
            SelectedLevelIndex = levelNum;
            SceneManager.LoadScene("Game");
        }

        public void OnBack()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("WorldSelect");
        }
    }
}
