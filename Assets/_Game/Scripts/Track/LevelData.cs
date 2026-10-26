using System;
using System.Collections.Generic;
using UnityEngine;

namespace DirtDash.Track
{
    /// <summary>
    /// Level configuration ScriptableObject containing track point coordinates,
    /// start/finish coordinates, coin pickups, and 3-star target times.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Dirt Dash/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Identity")]
        public int worldId = 1;
        public int levelIndex = 1;
        public string levelTitle = "Rolling Meadows";
        public string themeId = "hills"; // "hills", "desert", "arctic"

        [Header("Star Target Times (Seconds)")]
        [Tooltip("Target time to earn 1 star (Bronze)")]
        public float star1Time = 30.0f;

        [Tooltip("Target time to earn 2 stars (Silver)")]
        public float star2Time = 22.0f;

        [Tooltip("Target time to earn 3 stars (Gold)")]
        public float star3Time = 16.5f;

        [Header("Spawn & Finish Positions")]
        public Vector2 startPosition = new Vector2(0f, 1.5f);
        public float startRotationAngle = 0f;
        public Vector2 finishPosition = new Vector2(100f, 0f);

        [Header("Track Points & Optional Gap Segments")]
        [Tooltip("Continuous sequence of 2D coordinates forming the track")]
        public List<Vector2> trackPoints = new();

        [Tooltip("Optional disjoint segments (for jumps across gaps)")]
        public List<SegmentData> disjointSegments = new();

        [Header("Collectible Coins")]
        public List<Vector2> coinPositions = new();
    }

    [Serializable]
    public class SegmentData
    {
        public List<Vector2> points = new();
    }
}
