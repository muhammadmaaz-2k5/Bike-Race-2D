using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Pause menu dialog with Resume, Restart, Level Select, and Home navigation.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button homeButton;

        private void Start()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelect);
            if (homeButton != null) homeButton.onClick.AddListener(OnHome);
        }

        public void OnResume()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.SetPaused(false);
            gameObject.SetActive(false);
        }

        public void OnRestart()
        {
            AudioManager.Instance?.PlayButtonClick();
            GameManager.Instance?.RestartCurrentLevel();
        }

        public void OnLevelSelect()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene("LevelSelect");
        }

        public void OnHome()
        {
            AudioManager.Instance?.PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
