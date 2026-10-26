using System;
using System.Collections.Generic;
using UnityEngine;

namespace DirtDash.Core
{
    /// <summary>
    /// Persistent serializable save data structure for Dirt Dash.
    /// Serialized to JSON in Application.persistentDataPath.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int coins = 100;
        public int totalStars = 0;
        public string selectedBikeId = "bike_dirtrunner";
        public List<string> ownedBikeIds = new() { "bike_dirtrunner" };

        // Level records: key = "W{worldId}_L{levelIndex}"
        public SerializableDictionary<string, LevelRecord> levelRecords = new();

        // Settings
        public bool musicEnabled = true;
        public bool sfxEnabled = true;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public bool ghostEnabled = true;
        public bool fourDigitTimes = true;
        public bool solidGround = true;
        public bool useTiltControls = true;
        public bool vibrationEnabled = true;

        // Tasks and Chests
        public List<TaskData> activeTasks = new();
        public List<ChestData> activeChests = new();
        public long lastChestClaimTimestamp = 0;
    }

    [Serializable]
    public class LevelRecord
    {
        public int starsEarned = 0;
        public float bestTimeSeconds = 999.99f;
        public bool isUnlocked = false;
        public List<GhostFrame> ghostFrames = new();
    }

    [Serializable]
    public class GhostFrame
    {
        public float time;
        public float posX;
        public float posY;
        public float rotZ;
    }

    [Serializable]
    public class TaskData
    {
        public string taskId;
        public string description;
        public int currentProgress;
        public int targetGoal;
        public int rewardCoins;
        public bool isCompleted;
        public bool isClaimed;
    }

    [Serializable]
    public class ChestData
    {
        public string chestId;
        public string chestTier; // "Bronze", "Silver", "Gold"
        public int coinReward;
        public string bonusBikeId;
        public float unlockDurationSeconds;
        public float remainingSeconds;
        public bool isReadyToOpen;
    }

    /// <summary>
    /// Helper serializable dictionary for Unity JSON utility.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new();
        [SerializeField] private List<TValue> values = new();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}
