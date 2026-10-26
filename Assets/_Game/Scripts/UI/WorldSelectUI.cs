using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// World Select screen presenting swipeable cards for Hills, Desert, and Arctic worlds.
    /// Displays earned stars / 24 per world and enforces star unlock thresholds.
    /// </summary>
    public class WorldSelectUI : MonoBehaviour
    {
        [Header("World Cards")]
        [SerializeField] private Button world1Button; // Hills (Unlocked by default)
        [SerializeField] private Button world2Button; // Desert (Requires 12 stars)
        [SerializeField] private Button world3Button; // Arctic (Requires 24 stars)

        [Header("World Star Texts")]
        [SerializeField] private TextMeshProUGUI world1StarsText;
        [SerializeField] private TextMeshProUGUI world2StarsText;
        [SerializeField] private TextMeshProUGUI world3StarsText;

        [Header("Back Navigation")]
        [SerializeField] private Button backButton;

        public static int SelectedWorldId = 1;

        private void Start()
        {
            SetupButtons();
            RefreshWorldCards();
        }

        private void SetupButtons()
        {
            if (world1Button != null) world1Button.onClick.AddListener(() => OpenWorld(1));
            if (world2Button != null) world2Button.onClick.AddListener(() => OpenWorld(2));
            if (world3Button != null) world3Button.onClick.AddListener(() => OpenWorld(3));
            if (backButton != null) backButton.onClick.AddListener(OnBack);
        }

        private void RefreshWorldCards()
        {
            var save = SaveSystem.Instance;
            int totalStars = save != null ? save.Data.totalStars : 0;

            int w1Stars = CountStarsForWorld(1);
            int w2Stars = CountStarsForWorld(2);
            int w3Stars = CountStarsForWorld(3);

            if (world1StarsText != null) world1StarsText.text = $"HILLS\n{w1Stars} / 24 Stars";
            if (world2StarsText != null) world2StarsText.text = totalStars >= 12 ? $"DESERT\n{w2Stars} / 24 Stars" : "DESERT\n[Locked - 12 Stars]";
            if (world3StarsText != null) world3StarsText.text = totalStars >= 24 ? $"ARCTIC\n{w3Stars} / 24 Stars" : "ARCTIC\n[Locked - 24 Stars]";

            if (world2Button != null) world2Button.interactable = totalStars >= 12;
            if (world3Button != null) world3Button.interactable = totalStars >= 24;
        }

        private int CountStarsForWorld(int worldId)
        {
            var save = SaveSystem.Instance;
            if (save == null) return 0;

            int count = 0;
            for (int i = 1; i <= 8; i++)
            {
                var record = save.GetLevelRecord(worldId, i);
                count += record.starsEarned;
            }
            return count;
        }

        public void OpenWorld(int worldId)
        {
            AudioManager.Instance?.PlayButtonClick();
            SelectedWorldId = worldId;
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnBack()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
