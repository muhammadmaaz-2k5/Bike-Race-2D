using System.Collections.Generic;
using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Ghost
{
    /// <summary>
    /// Records player position and rotation at fixed intervals during a race.
    /// If the player sets a new best time, saves the trajectory frames to SaveSystem.
    /// </summary>
    public class GhostRecorder : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Rigidbody2D bikeBody;
        [SerializeField] private float sampleInterval = 0.05f; // 20 Hz recording

        private readonly List<GhostFrame> _currentRecording = new();
        private float _sampleTimer = 0f;
        private bool _isRecording = false;

        public List<GhostFrame> CurrentRecording => _currentRecording;

        private void OnEnable()
        {
            EventBus.Subscribe<LevelFinishedEvent>(OnLevelFinished);
            EventBus.Subscribe<BikeCrashedEvent>(OnBikeCrashed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelFinishedEvent>(OnLevelFinished);
            EventBus.Unsubscribe<BikeCrashedEvent>(OnBikeCrashed);
        }

        public void StartRecording(Rigidbody2D body)
        {
            bikeBody = body;
            _currentRecording.Clear();
            _sampleTimer = 0f;
            _isRecording = true;
        }

        private void Update()
        {
            if (!_isRecording || bikeBody == null) return;

            var gm = GameManager.Instance;
            if (gm == null || !gm.IsRaceActive) return;

            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= sampleInterval)
            {
                _sampleTimer = 0f;
                _currentRecording.Add(new GhostFrame
                {
                    time = gm.RaceTimer,
                    posX = bikeBody.position.x,
                    posY = bikeBody.position.y,
                    rotZ = bikeBody.rotation
                });
            }
        }

        private void OnLevelFinished(LevelFinishedEvent evt)
        {
            _isRecording = false;
        }

        private void OnBikeCrashed(BikeCrashedEvent evt)
        {
            _isRecording = false;
        }

        public void SaveIfBestTime(int worldId, int levelIndex, float finishTime)
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            var record = save.GetLevelRecord(worldId, levelIndex);
            if (finishTime <= record.bestTimeSeconds)
            {
                record.ghostFrames = new List<GhostFrame>(_currentRecording);
                save.Save();
                Debug.Log($"[Dirt Dash] Saved new Ghost recording with {_currentRecording.Count} frames.");
            }
        }
    }
}
