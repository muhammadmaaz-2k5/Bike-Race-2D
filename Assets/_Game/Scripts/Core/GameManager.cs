using UnityEngine;
using UnityEngine.SceneManagement;

namespace DirtDash.Core
{
    /// <summary>
    /// Central game state manager for Dirt Dash.
    /// Handles persistent session state, level loading, race restarts, and pause state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isRaceActive = false;
        [SerializeField] private bool isPaused = false;
        [SerializeField] private float raceTimer = 0f;

        public bool IsRaceActive => isRaceActive;
        public bool IsPaused => isPaused;
        public float RaceTimer => raceTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // Only persist across scenes if marked root
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            if (isRaceActive && !isPaused)
            {
                raceTimer += Time.deltaTime;
            }
        }

        /// <summary>
        /// Starts or resumes time-trial race timing.
        /// </summary>
        public void StartRace()
        {
            isRaceActive = true;
            raceTimer = 0f;
        }

        /// <summary>
        /// Restarts the currently active race/scene quickly on crash or retry.
        /// </summary>
        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;
            isPaused = false;
            isRaceActive = false;
            raceTimer = 0f;

            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        public void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
