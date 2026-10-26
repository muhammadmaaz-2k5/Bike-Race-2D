using System;
using System.Collections.Generic;
using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Progression
{
    /// <summary>
    /// Manages rotating challenges, progress updates, and reward chests with countdown unlock timers.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureActiveTasks();
        }

        private void Update()
        {
            UpdateChestTimers(Time.deltaTime);
        }

        public void EnsureActiveTasks()
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            if (save.Data.activeTasks.Count == 0)
            {
                save.Data.activeTasks.Add(new TaskData
                {
                    taskId = "task_stars_12",
                    description = "Collect 12 total Stars",
                    currentProgress = save.Data.totalStars,
                    targetGoal = 12,
                    rewardCoins = 150,
                    isCompleted = save.Data.totalStars >= 12
                });

                save.Data.activeTasks.Add(new TaskData
                {
                    taskId = "task_flips_5",
                    description = "Land 5 Backflips or Frontflips",
                    currentProgress = 0,
                    targetGoal = 5,
                    rewardCoins = 100
                });

                save.Data.activeTasks.Add(new TaskData
                {
                    taskId = "task_complete_3",
                    description = "Finish 3 Race Tracks",
                    currentProgress = 0,
                    targetGoal = 3,
                    rewardCoins = 120
                });

                save.Data.activeTasks.Add(new TaskData
                {
                    taskId = "task_coins_50",
                    description = "Collect 50 Coins",
                    currentProgress = 0,
                    targetGoal = 50,
                    rewardCoins = 200
                });

                save.Save();
            }
        }

        public void ReportProgress(string taskType, int amount = 1)
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            foreach (var task in save.Data.activeTasks)
            {
                if (task.isCompleted) continue;

                if (task.taskId.Contains(taskType))
                {
                    task.currentProgress = Mathf.Min(task.targetGoal, task.currentProgress + amount);
                    if (task.currentProgress >= task.targetGoal)
                    {
                        task.isCompleted = true;
                        Debug.Log($"[Dirt Dash] Task completed: {task.description}");
                    }
                    save.Save();
                }
            }
        }

        public bool ClaimTaskReward(string taskId)
        {
            var save = SaveSystem.Instance;
            if (save == null) return false;

            var task = save.Data.activeTasks.Find(t => t.taskId == taskId);
            if (task != null && task.isCompleted && !task.isClaimed)
            {
                task.isClaimed = true;
                save.AddCoins(task.rewardCoins);
                // Grant chest
                AddRewardChest("Silver", 150, 60f); // 60s chest for demonstration
                return true;
            }
            return false;
        }

        public void AddRewardChest(string tier, int coins, float durationSeconds)
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            save.Data.activeChests.Add(new ChestData
            {
                chestId = "chest_" + Guid.NewGuid().ToString().Substring(0, 8),
                chestTier = tier,
                coinReward = coins,
                unlockDurationSeconds = durationSeconds,
                remainingSeconds = durationSeconds,
                isReadyToOpen = false
            });
            save.Save();
        }

        private void UpdateChestTimers(float delta)
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            foreach (var chest in save.Data.activeChests)
            {
                if (!chest.isReadyToOpen)
                {
                    chest.remainingSeconds -= delta;
                    if (chest.remainingSeconds <= 0f)
                    {
                        chest.remainingSeconds = 0f;
                        chest.isReadyToOpen = true;
                    }
                }
            }
        }

        public bool OpenChest(string chestId, out int coinsReward)
        {
            coinsReward = 0;
            var save = SaveSystem.Instance;
            if (save == null) return false;

            var chest = save.Data.activeChests.Find(c => c.chestId == chestId);
            if (chest != null && chest.isReadyToOpen)
            {
                coinsReward = chest.coinReward;
                save.AddCoins(coinsReward);
                save.Data.activeChests.Remove(chest);
                save.Save();
                return true;
            }
            return false;
        }
    }
}
