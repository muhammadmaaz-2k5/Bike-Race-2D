using System;
using System.Collections.Generic;

namespace DirtDash.Core
{
    /// <summary>
    /// Lightweight decoupled Event Bus for Dirt Dash.
    /// Provides strongly-typed publish/subscribe mechanism without rigid dependencies.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (!Subscribers.TryGetValue(eventType, out List<Delegate> list))
            {
                list = new List<Delegate>();
                Subscribers[eventType] = list;
            }
            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (Subscribers.TryGetValue(eventType, out List<Delegate> list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    Subscribers.Remove(eventType);
                }
            }
        }

        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            if (Subscribers.TryGetValue(eventType, out List<Delegate> list))
            {
                // Iterate backwards or over a copy to allow handlers to unsubscribe during dispatch
                var copy = list.ToArray();
                for (int i = 0; i < copy.Length; i++)
                {
                    if (copy[i] is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }
    }

    #region Common Event Payloads

    /// <summary>
    /// Dispatched when the bike crashes (head touches ground or falling into void).
    /// </summary>
    public readonly struct BikeCrashedEvent
    {
        public readonly string Reason;
        public readonly UnityEngine.Vector2 Position;

        public BikeCrashedEvent(string reason, UnityEngine.Vector2 position)
        {
            Reason = reason;
            Position = position;
        }
    }

    /// <summary>
    /// Dispatched when the rider performs a front or back flip in the air.
    /// </summary>
    public readonly struct BikeFlipEvent
    {
        public readonly bool IsBackflip;
        public readonly int TotalFlipsThisAir;
        public readonly int BonusScore;

        public BikeFlipEvent(bool isBackflip, int totalFlipsThisAir, int bonusScore = 10)
        {
            IsBackflip = isBackflip;
            TotalFlipsThisAir = totalFlipsThisAir;
            BonusScore = bonusScore;
        }
    }

    /// <summary>
    /// Dispatched when the bike crosses the finish line.
    /// </summary>
    public readonly struct LevelFinishedEvent
    {
        public readonly float FinalTime;
        public readonly int StarsEarned;

        public LevelFinishedEvent(float finalTime, int starsEarned)
        {
            FinalTime = finalTime;
            StarsEarned = starsEarned;
        }
    }

    #endregion
}
