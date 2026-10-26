using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Track
{
    /// <summary>
    /// Finish line trigger banner that records race completion,
    /// computes earned stars (1-3), and triggers the results screen.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class FinishLine : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelData currentLevelData;
        [SerializeField] private GameObject bannerVisual;

        private bool _isCrossed = false;

        public void SetLevelData(LevelData levelData)
        {
            currentLevelData = levelData;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCrossed) return;

            // Check if player bike crossed
            if (other.CompareTag(GameConstants.TagPlayer) || other.transform.root.CompareTag(GameConstants.TagPlayer))
            {
                CrossFinishLine();
            }
        }

        private void CrossFinishLine()
        {
            _isCrossed = true;

            float finalTime = 0f;
            var gm = GameManager.Instance;
            if (gm != null)
            {
                finalTime = gm.RaceTimer;
            }

            int stars = 1; // Base 1 star for finishing
            if (currentLevelData != null)
            {
                if (finalTime <= currentLevelData.star3Time)
                {
                    stars = 3;
                }
                else if (finalTime <= currentLevelData.star2Time)
                {
                    stars = 2;
                }

                // Record progress
                SaveSystem.Instance?.RecordLevelFinish(
                    currentLevelData.worldId,
                    currentLevelData.levelIndex,
                    finalTime,
                    stars
                );
            }

            Debug.Log($"[Dirt Dash] FINISH LINE CROSSED! Time: {finalTime:F2}s, Stars: {stars}");

            AudioManager.Instance?.PlayWinJingle();
            EventBus.Publish(new LevelFinishedEvent(finalTime, stars));
        }

        public void ResetFinishLine()
        {
            _isCrossed = false;
        }
    }
}
