using UnityEngine;

namespace DirtDash.Cameras
{
    /// <summary>
    /// Parallax background layer follower.
    /// Moves at a fraction of the camera's horizontal speed to create depth,
    /// and infinitely tiles when the camera traverses past the sprite width.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("Horizontal speed multiplier (0 = static sky, 0.2 = far mountains, 0.7 = near hills)")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactorX = 0.3f;

        [Tooltip("Vertical speed multiplier")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactorY = 0.1f;

        [Tooltip("Length along X before repositioning for seamless tiling")]
        [SerializeField] private float textureUnitSizeX = 30f;

        [SerializeField] private Transform cameraTransform;

        private Vector3 _lastCameraPosition;
        private float _startPosX;

        private void Start()
        {
            if (cameraTransform == null && UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
            }

            if (cameraTransform != null)
            {
                _lastCameraPosition = cameraTransform.position;
            }

            _startPosX = transform.position.x;
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 deltaMovement = cameraTransform.position - _lastCameraPosition;

            // Move relative to parallax factor
            transform.position += new Vector3(
                deltaMovement.x * (1f - parallaxFactorX),
                deltaMovement.y * (1f - parallaxFactorY),
                0f
            );

            _lastCameraPosition = cameraTransform.position;

            // Infinite horizontal tiling check
            if (textureUnitSizeX > 0.01f)
            {
                float distanceFromStart = cameraTransform.position.x * (1f - parallaxFactorX);
                float offset = cameraTransform.position.x - transform.position.x;

                if (Mathf.Abs(offset) >= textureUnitSizeX)
                {
                    transform.position = new Vector3(cameraTransform.position.x, transform.position.y, transform.position.z);
                }
            }
        }
    }
}
