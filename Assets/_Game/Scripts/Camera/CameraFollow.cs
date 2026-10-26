using UnityEngine;

namespace DirtDash.Cameras
{
    /// <summary>
    /// Smooth follow camera with forward velocity look-ahead, speed-based dynamic zoom,
    /// manual close/far zoom toggle, and vertical bounds clamping.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private Rigidbody2D targetRigidbody;
        [SerializeField] private Vector3 baseOffset = new Vector3(2.5f, 1.8f, -10f);

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float lookAheadFactor = 0.25f;
        [SerializeField] private float maxLookAheadX = 5.0f;

        [Header("Dynamic Speed Zoom")]
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private float minOrthoSize = 6.0f;
        [SerializeField] private float maxOrthoSize = 10.5f;
        [SerializeField] private float maxSpeedForZoom = 30f;
        [SerializeField] private float zoomSpeed = 4f;

        [Header("Manual Zoom Toggle Mode")]
        [SerializeField] private bool isFarViewActive = false;
        [SerializeField] private float farViewOrthoMultiplier = 1.35f;

        [Header("Clamping Limits")]
        [SerializeField] private float minCameraY = -2f;

        private float _currentLookAheadX = 0f;

        public bool IsFarViewActive => isFarViewActive;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<UnityEngine.Camera>();
                if (targetCamera == null)
                {
                    targetCamera = UnityEngine.Camera.main;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Compute look-ahead based on target horizontal velocity
            float targetVelX = 0f;
            float targetSpeed = 0f;
            if (targetRigidbody != null)
            {
                targetVelX = targetRigidbody.linearVelocity.x;
                targetSpeed = targetRigidbody.linearVelocity.magnitude;
            }

            float desiredLookAheadX = Mathf.Clamp(targetVelX * lookAheadFactor, -maxLookAheadX * 0.5f, maxLookAheadX);
            _currentLookAheadX = Mathf.Lerp(_currentLookAheadX, desiredLookAheadX, Time.deltaTime * 3f);

            // Compute desired camera position
            Vector3 desiredPosition = target.position + baseOffset;
            desiredPosition.x += _currentLookAheadX;
            desiredPosition.z = baseOffset.z;

            // Clamp vertical position so camera never drops below the track kill-threshold
            if (desiredPosition.y < minCameraY)
            {
                desiredPosition.y = minCameraY;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

            // Dynamic Orthographic Zoom
            if (targetCamera != null && targetCamera.orthographic)
            {
                float speedRatio = Mathf.Clamp01(targetSpeed / maxSpeedForZoom);
                float targetSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, speedRatio);

                if (isFarViewActive)
                {
                    targetSize *= farViewOrthoMultiplier;
                }

                targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
            }
        }

        /// <summary>
        /// Assigns target to follow.
        /// </summary>
        public void SetTarget(Transform newTarget, Rigidbody2D newRb = null)
        {
            target = newTarget;
            targetRigidbody = newRb;
            if (target != null && targetRigidbody == null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
            }
        }

        /// <summary>
        /// Toggles between close and far view (called by HUD magnifier zoom button).
        /// </summary>
        public void ToggleFarView()
        {
            isFarViewActive = !isFarViewActive;
        }

        public void SetFarView(bool active)
        {
            isFarViewActive = active;
        }
    }
}
