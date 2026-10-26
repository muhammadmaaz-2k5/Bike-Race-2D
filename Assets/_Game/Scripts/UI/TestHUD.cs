using UnityEngine;
using DirtDash.Bike;
using DirtDash.Cameras;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Lightweight runtime telemetry and controls overlay for testing bike physics.
    /// Works immediately without complex UI canvas setup.
    /// </summary>
    public class TestHUD : MonoBehaviour
    {
        [SerializeField] private BikeController bikeController;
        [SerializeField] private CameraFollow cameraFollow;

        private string _lastFlipNotice = "";
        private float _flipNoticeTimer = 0f;

        private void OnEnable()
        {
            EventBus.Subscribe<BikeFlipEvent>(OnBikeFlip);
            EventBus.Subscribe<BikeCrashedEvent>(OnBikeCrashed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BikeFlipEvent>(OnBikeFlip);
            EventBus.Unsubscribe<BikeCrashedEvent>(OnBikeCrashed);
        }

        private void Start()
        {
            if (bikeController == null)
            {
                bikeController = FindFirstObjectByType<BikeController>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }
        }

        private void Update()
        {
            if (_flipNoticeTimer > 0f)
            {
                _flipNoticeTimer -= Time.deltaTime;
            }

            // Quick restart key for testing
            if (Input.GetKeyDown(KeyCode.R))
            {
                var gm = GameManager.Instance;
                if (gm != null) gm.RestartCurrentLevel();
            }

            // Quick camera zoom key
            if (Input.GetKeyDown(KeyCode.Z) && cameraFollow != null)
            {
                cameraFollow.ToggleFarView();
            }
        }

        private void OnBikeFlip(BikeFlipEvent evt)
        {
            string flipName = evt.IsBackflip ? "BACKFLIP!" : "FRONTFLIP!";
            _lastFlipNotice = $"+{flipName} (x{evt.TotalFlipsThisAir}) +{evt.BonusScore} PTS";
            _flipNoticeTimer = 2.0f;
        }

        private void OnBikeCrashed(BikeCrashedEvent evt)
        {
            _lastFlipNotice = $"CRASH! ({evt.Reason}) Respawning in 1s...";
            _flipNoticeTimer = 1.8f;
        }

        private void OnGUI()
        {
            // Clean overlay box in top-left
            GUI.Box(new Rect(15, 15, 310, 160), "<b>DIRT DASH - PHASE 1 TEST HUD</b>");

            GUILayout.BeginArea(new Rect(25, 42, 290, 130));

            if (bikeController != null)
            {
                GUILayout.Label($"<b>Speed:</b> {bikeController.CurrentSpeedKmh:F1} km/h");
                GUILayout.Label($"<b>Grounded:</b> {(bikeController.IsGrounded ? "<color=#2ECC71>YES</color>" : "<color=#E74C3C>IN AIR</color>")}");
                GUILayout.Label($"<b>Front/Rear Ground:</b> F:{(bikeController.IsFrontGrounded ? "✓" : "✗")}  R:{(bikeController.IsBackGrounded ? "✓" : "✗")}");
            }

            GUILayout.Label("<b>Controls:</b> D / → (Run), A / ← (Brake), W/S or ↑/↓ (Tilt)");
            GUILayout.Label("<b>Keys:</b> R (Restart), Z (Toggle Zoom)");

            GUILayout.EndArea();

            // Flip / Crash Banner in Center Screen
            if (_flipNoticeTimer > 0f && !string.IsNullOrEmpty(_lastFlipNotice))
            {
                GUIStyle bannerStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                bannerStyle.normal.textColor = _lastFlipNotice.StartsWith("CRASH") ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.85f, 0.2f);

                GUI.Box(new Rect(Screen.width * 0.5f - 200, 70, 400, 50), _lastFlipNotice, bannerStyle);
            }
        }
    }
}
