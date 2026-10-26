using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DirtDash.Progression;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Tasks screen managing 4 rotating missions, progress sliders, and reward chest countdowns.
    /// </summary>
    public class TasksUI : MonoBehaviour
    {
        [Header("Task Item Rows (Up to 4)")]
        [SerializeField] private TextMeshProUGUI[] taskDescTexts = new TextMeshProUGUI[4];
        [SerializeField] private Slider[] taskSliders = new Slider[4];
        [SerializeField] private TextMeshProUGUI[] taskProgressTexts = new TextMeshProUGUI[4];
        [SerializeField] private Button[] taskClaimButtons = new Button[4];

        [Header("Chests Display")]
        [SerializeField] private TextMeshProUGUI chestStatusText;
        [SerializeField] private Button openChestButton;
        [SerializeField] private Button closeButton;

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            if (openChestButton != null) openChestButton.onClick.AddListener(OnOpenChest);

            RefreshTasks();
        }

        private void Update()
        {
            UpdateChestDisplay();
        }

        public void RefreshTasks()
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            var tasks = save.Data.activeTasks;
            for (int i = 0; i < 4 && i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (i < taskDescTexts.Length && taskDescTexts[i] != null)
                {
                    taskDescTexts[i].text = task.description;
                }

                if (i < taskSliders.Length && taskSliders[i] != null)
                {
                    taskSliders[i].maxValue = task.targetGoal;
                    taskSliders[i].value = task.currentProgress;
                }

                if (i < taskProgressTexts.Length && taskProgressTexts[i] != null)
                {
                    taskProgressTexts[i].text = $"{task.currentProgress} / {task.targetGoal}";
                }

                if (i < taskClaimButtons.Length && taskClaimButtons[i] != null)
                {
                    taskClaimButtons[i].interactable = task.isCompleted && !task.isClaimed;
                    var btnText = taskClaimButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                    {
                        btnText.text = task.isClaimed ? "CLAIMED" : (task.isCompleted ? "CLAIM" : $"{task.rewardCoins} COINS");
                    }

                    string taskId = task.taskId;
                    taskClaimButtons[i].onClick.RemoveAllListeners();
                    taskClaimButtons[i].onClick.AddListener(() => OnClaimTask(taskId));
                }
            }
        }

        private void OnClaimTask(string taskId)
        {
            if (TaskManager.Instance != null && TaskManager.Instance.ClaimTaskReward(taskId))
            {
                AudioManager.Instance?.PlayWinJingle();
                RefreshTasks();
            }
        }

        private void UpdateChestDisplay()
        {
            var save = SaveSystem.Instance;
            if (save == null || save.Data.activeChests.Count == 0)
            {
                if (chestStatusText != null) chestStatusText.text = "No active chests.\nComplete tasks to earn chests!";
                if (openChestButton != null) openChestButton.interactable = false;
                return;
            }

            var firstChest = save.Data.activeChests[0];
            if (firstChest.isReadyToOpen)
            {
                if (chestStatusText != null) chestStatusText.text = $"<b>{firstChest.chestTier} Chest</b> is READY TO OPEN!";
                if (openChestButton != null) openChestButton.interactable = true;
            }
            else
            {
                int mins = (int)(firstChest.remainingSeconds / 60f);
                int secs = (int)(firstChest.remainingSeconds % 60f);
                if (chestStatusText != null) chestStatusText.text = $"Unlocking <b>{firstChest.chestTier} Chest</b>: {mins:00}:{secs:00}";
                if (openChestButton != null) openChestButton.interactable = false;
            }
        }

        private void OnOpenChest()
        {
            var save = SaveSystem.Instance;
            if (save == null || save.Data.activeChests.Count == 0) return;

            string id = save.Data.activeChests[0].chestId;
            if (TaskManager.Instance != null && TaskManager.Instance.OpenChest(id, out int coins))
            {
                AudioManager.Instance?.PlayWinJingle();
                RefreshTasks();
            }
        }
    }
}
