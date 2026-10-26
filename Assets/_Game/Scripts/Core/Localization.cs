using System.Collections.Generic;

namespace DirtDash.Core
{
    /// <summary>
    /// Centralized localization dictionary for all UI text, dialogue, and notifications in Dirt Dash.
    /// Can be swapped for JSON/CSV or TextMeshPro string tables.
    /// </summary>
    public static class Localization
    {
        private static readonly Dictionary<string, string> Strings = new()
        {
            // Main Menu
            { "ui_game_title", "DIRT DASH" },
            { "ui_singleplayer", "SINGLE PLAYER" },
            { "ui_tasks", "TASKS" },
            { "ui_shop", "GARAGE / SHOP" },
            { "ui_settings", "SETTINGS" },
            { "ui_multiplayer", "MULTIPLAYER" },
            { "ui_tournaments", "TOURNAMENTS" },
            { "ui_coming_soon", "Coming Soon" },

            // HUD & In-Game
            { "ui_touch_to_start", "Touch anywhere to start!" },
            { "ui_tutorial_mobile", "Hold Right: RUN | Hold Left: BRAKE | Tilt: ROTATE" },
            { "ui_tutorial_desktop", "D / → : RUN | A / ← : BRAKE | W/S or ↑/↓ : TILT" },
            { "ui_pause", "PAUSE" },
            { "ui_resume", "RESUME" },
            { "ui_restart", "RESTART" },
            { "ui_stage_select", "STAGES" },
            { "ui_home", "HOME" },
            { "ui_next_level", "NEXT LEVEL" },

            // Results Screen
            { "ui_victory", "FINISHED!" },
            { "ui_new_best", "NEW BEST TIME!" },
            { "ui_time", "TIME" },
            { "ui_target_times", "STAR TARGETS" },
            { "ui_coins_earned", "+COINS" },
            { "ui_leaderboard", "TOP RIDERS" },

            // Settings
            { "ui_music", "Music" },
            { "ui_sfx", "Sound FX" },
            { "ui_ghost", "Best Time Ghost" },
            { "ui_four_digits", "4-Digit Times (ms)" },
            { "ui_solid_ground", "Solid Ground Fill" },
            { "ui_control_mode", "Control Mode (Tilt vs Buttons)" },
            { "ui_vibration", "Vibration" },
            { "ui_privacy", "Privacy Policy" },
            { "ui_credits", "Credits" },
            { "ui_reset_progress", "Reset All Progress" }
        };

        public static string Get(string key, string fallback = "")
        {
            if (Strings.TryGetValue(key, out string value))
            {
                return value;
            }
            return !string.IsNullOrEmpty(fallback) ? fallback : key;
        }
    }
}
