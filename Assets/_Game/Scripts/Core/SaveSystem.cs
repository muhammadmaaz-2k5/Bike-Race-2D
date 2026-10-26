using System;
using System.IO;
using UnityEngine;

namespace DirtDash.Core
{
    /// <summary>
    /// Persistent JSON save manager for Dirt Dash.
    /// Manages player progression, unlocked bikes, star tallies, best times, and settings.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SaveFileName = "dirtdash_save.json";
        private string _saveFilePath;
        private SaveData _data;

        public SaveData Data => _data ?? (_data = LoadOrCreate());

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            _data = LoadOrCreate();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Dirt Dash] Failed to save game data: {ex.Message}");
            }
        }

        public SaveData LoadOrCreate()
        {
            if (_saveFilePath == null)
            {
                _saveFilePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            }

            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var loaded = JsonUtility.FromJson<SaveData>(json);
                    if (loaded != null)
                    {
                        EnsureDefaultUnlocks(loaded);
                        return loaded;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Dirt Dash] Save file corrupted or unreadable ({ex.Message}), creating new save.");
                }
            }

            var fresh = new SaveData();
            EnsureDefaultUnlocks(fresh);
            return fresh;
        }

        private void EnsureDefaultUnlocks(SaveData data)
        {
            if (!data.ownedBikeIds.Contains("bike_dirtrunner"))
            {
                data.ownedBikeIds.Add("bike_dirtrunner");
            }

            // Ensure Level 1 is unlocked
            string w1l1 = "W1_L1";
            if (!data.levelRecords.ContainsKey(w1l1))
            {
                data.levelRecords[w1l1] = new LevelRecord { isUnlocked = true };
            }
            else
            {
                data.levelRecords[w1l1].isUnlocked = true;
            }
        }

        public void AddCoins(int amount)
        {
            Data.coins += amount;
            Save();
        }

        public bool SpendCoins(int amount)
        {
            if (Data.coins >= amount)
            {
                Data.coins -= amount;
                Save();
                return true;
            }
            return false;
        }

        public LevelRecord GetLevelRecord(int worldId, int levelIndex)
        {
            string key = $"W{worldId}_L{levelIndex}";
            if (!Data.levelRecords.ContainsKey(key))
            {
                bool unlockedByDefault = (worldId == 1 && levelIndex == 1);
                Data.levelRecords[key] = new LevelRecord { isUnlocked = unlockedByDefault };
            }
            return Data.levelRecords[key];
        }

        public void RecordLevelFinish(int worldId, int levelIndex, float finishTime, int starsEarned)
        {
            string key = $"W{worldId}_L{levelIndex}";
            var record = GetLevelRecord(worldId, levelIndex);

            // Update best stars
            if (starsEarned > record.starsEarned)
            {
                int newStars = starsEarned - record.starsEarned;
                Data.totalStars += newStars;
                record.starsEarned = starsEarned;
                AddCoins(newStars * 10); // +10 coins per new star earned!
            }

            // Update best time
            if (finishTime < record.bestTimeSeconds)
            {
                record.bestTimeSeconds = finishTime;
            }

            // Unlock next level in this world
            if (levelIndex < 8)
            {
                var next = GetLevelRecord(worldId, levelIndex + 1);
                next.isUnlocked = true;
            }

            Save();
        }

        public void ResetAllProgress()
        {
            _data = new SaveData();
            EnsureDefaultUnlocks(_data);
            Save();
            Debug.Log("[Dirt Dash] Progress reset to fresh state.");
        }
    }
}
