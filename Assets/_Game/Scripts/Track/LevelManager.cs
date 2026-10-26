using System.Collections.Generic;
using UnityEngine;
using DirtDash.Bike;
using DirtDash.Cameras;
using DirtDash.Core;
using DirtDash.Ghost;
using DirtDash.Progression;
using DirtDash.UI;

namespace DirtDash.Track
{
    /// <summary>
    /// Coordinates level loading, bike spawning, camera tracking, and ghost playback
    /// inside the main Game scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Scene References")]
        [SerializeField] private TrackBuilder trackBuilder;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private GhostRecorder ghostRecorder;
        [SerializeField] private GhostPlayer ghostPlayer;
        [SerializeField] private HUDController hudController;

        [Header("Bike Prefab Base")]
        [SerializeField] private GameObject bikePrefab;

        [Header("Catalog of Levels")]
        [SerializeField] private List<LevelData> allLevels = new();

        private LevelData _currentLevel;
        private GameObject _spawnedBike;

        public LevelData CurrentLevel => _currentLevel;
        public GameObject SpawnedBike => _spawnedBike;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LoadSelectedLevel();
        }

        public void LoadSelectedLevel()
        {
            int worldId = WorldSelectUI.SelectedWorldId;
            int levelIndex = LevelSelectUI.SelectedLevelIndex;

            // Find matching level
            _currentLevel = allLevels.Find(l => l.worldId == worldId && l.levelIndex == levelIndex);

            // Fallback: build procedural level if asset list is not linked yet
            if (_currentLevel == null)
            {
                _currentLevel = ScriptableObject.CreateInstance<LevelData>();
                _currentLevel.worldId = worldId;
                _currentLevel.levelIndex = levelIndex;
                _currentLevel.trackPoints = GenerateDefaultTrack(worldId, levelIndex);
                _currentLevel.finishPosition = _currentLevel.trackPoints[^1];
            }

            // Build track collision and visual mesh
            if (trackBuilder != null)
            {
                trackBuilder.BuildLevel(_currentLevel);
            }

            // Spawn player's equipped bike
            SpawnPlayerBike(_currentLevel.startPosition);

            // Initialize Ghost playback if best run exists
            SetupGhost();
        }

        private void SpawnPlayerBike(Vector2 spawnPos)
        {
            if (bikePrefab != null)
            {
                _spawnedBike = Instantiate(bikePrefab, spawnPos, Quaternion.identity);
                var controller = _spawnedBike.GetComponent<BikeController>();

                // Apply active equipped bike stats from ShopManager
                if (controller != null && ShopManager.Instance != null)
                {
                    var equippedStats = ShopManager.Instance.GetEquippedBikeStats();
                    if (equippedStats != null)
                    {
                        controller.ApplyStats(equippedStats);
                    }
                }

                // Attach camera tracking
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(_spawnedBike.transform, _spawnedBike.GetComponent<Rigidbody2D>());
                }

                // Start ghost recorder
                if (ghostRecorder != null && controller != null)
                {
                    ghostRecorder.StartRecording(controller.BodyRigidbody);
                }
            }
        }

        private void SetupGhost()
        {
            if (ghostPlayer == null || SaveSystem.Instance == null) return;

            var record = SaveSystem.Instance.GetLevelRecord(_currentLevel.worldId, _currentLevel.levelIndex);
            if (record != null && record.ghostFrames != null && record.ghostFrames.Count > 1)
            {
                ghostPlayer.InitializeGhost(record.ghostFrames);
            }
            else
            {
                ghostPlayer.gameObject.SetActive(false);
            }
        }

        private List<Vector2> GenerateDefaultTrack(int world, int level)
        {
            var pts = new List<Vector2>();
            pts.AddRange(TrackGenerators.Straight(new Vector2(-10f, 0f), 25f));
            pts.AddRange(TrackGenerators.SmoothHill(pts[^1], 20f, 2.5f + level * 0.4f));
            pts.AddRange(TrackGenerators.Bumps(pts[^1], 3, 6f, 0.7f));
            pts.AddRange(TrackGenerators.Ramp(pts[^1], 16f, 4.0f));
            pts.AddRange(TrackGenerators.Straight(pts[^1], 35f));
            return pts;
        }
    }
}
