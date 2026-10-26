using System;
using System.Collections.Generic;
using UnityEngine;

namespace DirtDash.Core
{
    [Serializable]
    public struct LeaderboardEntry
    {
        public int rank;
        public string riderName;
        public float timeSeconds;
        public bool isPlayer;

        public LeaderboardEntry(int rank, string riderName, float timeSeconds, bool isPlayer = false)
        {
            this.rank = rank;
            this.riderName = riderName;
            this.timeSeconds = timeSeconds;
            this.isPlayer = isPlayer;
        }
    }

    /// <summary>
    /// Contract for leaderboard backends (local ghost rivals or online leaderboards).
    /// </summary>
    public interface ILeaderboardService
    {
        void GetLeaderboard(int worldId, int levelIndex, Action<List<LeaderboardEntry>> onComplete);
        void SubmitTime(int worldId, int levelIndex, float playerTime, Action<bool> onComplete);
    }

    /// <summary>
    /// Local offline implementation providing competitive AI rival benchmark times.
    /// </summary>
    public class LocalLeaderboardService : ILeaderboardService
    {
        private static readonly string[] RivalNames = new[]
        {
            "Dusty Bolt", "Sandy Viper", "Blaze Runner", "Apex Rider", "Turbo Fox", "Nitro Nomad"
        };

        public void GetLeaderboard(int worldId, int levelIndex, Action<List<LeaderboardEntry>> onComplete)
        {
            var list = new List<LeaderboardEntry>();
            float baseTarget = 15f + (worldId * 4f) + (levelIndex * 2f);

            var record = SaveSystem.Instance?.GetLevelRecord(worldId, levelIndex);
            float playerBest = (record != null && record.bestTimeSeconds < 900f) ? record.bestTimeSeconds : 0f;

            // Generate 4 procedural benchmark rivals
            for (int i = 0; i < 4; i++)
            {
                float rivalTime = baseTarget + (i * 1.8f) + (worldId * 0.5f);
                list.Add(new LeaderboardEntry(i + 1, RivalNames[(worldId + levelIndex + i) % RivalNames.Length], rivalTime));
            }

            // Insert player time if recorded
            if (playerBest > 0.01f)
            {
                list.Add(new LeaderboardEntry(0, "YOU", playerBest, true));
                list.Sort((a, b) => a.timeSeconds.CompareTo(b.timeSeconds));
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    list[i] = new LeaderboardEntry(i + 1, e.riderName, e.timeSeconds, e.isPlayer);
                }
            }

            if (list.Count > 5) list = list.GetRange(0, 5);
            onComplete?.Invoke(list);
        }

        public void SubmitTime(int worldId, int levelIndex, float playerTime, Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }

    #region Monetization Stubs

    public interface IAdService
    {
        bool IsRewardedAdAvailable();
        void ShowRewardedAd(Action<bool> onReward);
        void ShowInterstitial(Action onClosed);
    }

    public class StubAdService : IAdService
    {
        public bool IsRewardedAdAvailable() => true;
        public void ShowRewardedAd(Action<bool> onReward) => onReward?.Invoke(true);
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();
    }

    public interface IPurchaseService
    {
        void BuyCoinPack(int packId, Action<bool> onComplete);
        void RestorePurchases(Action<bool> onComplete);
    }

    public class StubPurchaseService : IPurchaseService
    {
        public void BuyCoinPack(int packId, Action<bool> onComplete) => onComplete?.Invoke(true);
        public void RestorePurchases(Action<bool> onComplete) => onComplete?.Invoke(true);
    }

    #endregion
}
