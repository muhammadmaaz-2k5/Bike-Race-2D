using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using DirtDash.Core;

namespace DirtDash.Bike
{
    /// <summary>
    /// Mobile input controller for Dirt Dash.
    /// Manages left/right screen half touch zones (Brake & Run), device accelerometer tilt,
    /// and alternative on-screen lean buttons when tilt is disabled in settings.
    /// </summary>
    public class MobileInputController : MonoBehaviour
    {
        [Header("Target Bike Controller")]
        [SerializeField] private BikeController bikeController;

        [Header("Tilt / Accelerometer Sensitivity")]
        [SerializeField] private float tiltSensitivity = 2.4f;
        [SerializeField] private float tiltDeadzone = 0.05f;

        [Header("On-Screen Button Fallback")]
        [SerializeField] private bool showOnScreenLeanButtons = false;

        private float _mobileThrottle = 0f;
        private float _mobileBrake = 0f;
        private float _mobileLean = 0f;

        private void Start()
        {
            if (bikeController == null)
            {
                bikeController = GetComponent<BikeController>();
                if (bikeController == null)
                {
                    bikeController = FindFirstObjectByType<BikeController>();
                }
            }
        }

        private void Update()
        {
            if (bikeController == null || !bikeController.IsControllable) return;

            bool isTouchDevice = Application.isMobilePlatform || Input.touchCount > 0;
            if (!isTouchDevice && !Application.isEditor) return;

            // Process Touch Zones: Left half = Brake, Right half = Accelerate
            ProcessScreenTouches();

            // Process Tilt or Button Leaning
            ProcessTilt();

            // Feed inputs into BikeController
            if (_mobileThrottle > 0.01f || _mobileBrake > 0.01f || Mathf.Abs(_mobileLean) > 0.01f)
            {
                bikeController.SetMobileInputs(_mobileThrottle, _mobileBrake, _mobileLean);
            }
            else
            {
                bikeController.ReleaseMobileInputLock();
            }
        }

        private void ProcessScreenTouches()
        {
            _mobileThrottle = 0f;
            _mobileBrake = 0f;

            float screenWidth = Screen.width;

#if ENABLE_INPUT_SYSTEM
            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (touches.Count > 0)
            {
                foreach (var touch in touches)
                {
                    if (touch.screenPosition.x > screenWidth * 0.5f)
                    {
                        _mobileThrottle = 1f;
                    }
                    else
                    {
                        _mobileBrake = 1f;
                    }
                }
                return;
            }
#endif

            // Fallback to legacy touches or mouse clicks in editor
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    if (t.position.x > screenWidth * 0.5f)
                    {
                        _mobileThrottle = 1f;
                    }
                    else
                    {
                        _mobileBrake = 1f;
                    }
                }
            }
        }

        private void ProcessTilt()
        {
            _mobileLean = 0f;

            bool useTilt = SaveSystem.Instance == null || SaveSystem.Instance.Data.useTiltControls;

            if (useTilt)
            {
                // Read accelerometer
                float accelX = 0f;
#if ENABLE_INPUT_SYSTEM
                if (Accelerometer.current != null)
                {
                    accelX = Accelerometer.current.acceleration.ReadValue().x;
                }
                else
                {
                    accelX = Input.acceleration.x;
                }
#else
                accelX = Input.acceleration.x;
#endif

                if (Mathf.Abs(accelX) > tiltDeadzone)
                {
                    // Map accelerometer tilt to lean input
                    _mobileLean = Mathf.Clamp(accelX * tiltSensitivity, -1f, 1f);
                }
            }
        }

        /// <summary>
        /// Public callbacks for UI on-screen lean buttons (when tilt mode is turned off).
        /// </summary>
        public void SetButtonLean(float leanValue)
        {
            _mobileLean = Mathf.Clamp(leanValue, -1f, 1f);
        }
    }
}
