using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Bike
{
    /// <summary>
    /// Tracks mid-air continuous rotation to detect front-flips and back-flips.
    /// Publishes BikeFlipEvent with direction and flip counter.
    /// </summary>
    public class FlipDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BikeController bikeController;

        [Header("Runtime State")]
        [SerializeField] private float accumulatedAngle = 0f;
        [SerializeField] private int totalFlipsInCurrentJump = 0;
        [SerializeField] private bool wasGroundedLastFrame = true;

        private float _lastZRotation = 0f;

        public int TotalFlipsInCurrentJump => totalFlipsInCurrentJump;

        private void Awake()
        {
            if (bikeController == null)
            {
                bikeController = GetComponent<BikeController>();
            }
        }

        private void Start()
        {
            if (bikeController != null && bikeController.BodyRigidbody != null)
            {
                _lastZRotation = bikeController.BodyRigidbody.rotation;
            }
        }

        private void Update()
        {
            if (bikeController == null || bikeController.BodyRigidbody == null) return;
            if (bikeController.IsCrashed) return;

            bool isGrounded = bikeController.IsGrounded;
            float currentZRotation = bikeController.BodyRigidbody.rotation;

            if (isGrounded)
            {
                // Reset jump tracking when grounded
                if (!wasGroundedLastFrame)
                {
                    accumulatedAngle = 0f;
                    totalFlipsInCurrentJump = 0;
                }
            }
            else
            {
                // In air: accumulate angular delta
                float deltaAngle = Mathf.DeltaAngle(_lastZRotation, currentZRotation);
                accumulatedAngle += deltaAngle;

                // Check 360 degree completion
                // Positive delta = counter-clockwise (Backflip in 2D right-facing bike)
                // Negative delta = clockwise (Frontflip in 2D right-facing bike)
                if (accumulatedAngle >= 360f)
                {
                    totalFlipsInCurrentJump++;
                    accumulatedAngle -= 360f;
                    Debug.Log($"[Dirt Dash] BACKFLIP! (Total this jump: {totalFlipsInCurrentJump})");
                    EventBus.Publish(new BikeFlipEvent(isBackflip: true, totalFlipsInCurrentJump));
                }
                else if (accumulatedAngle <= -360f)
                {
                    totalFlipsInCurrentJump++;
                    accumulatedAngle += 360f;
                    Debug.Log($"[Dirt Dash] FRONTFLIP! (Total this jump: {totalFlipsInCurrentJump})");
                    EventBus.Publish(new BikeFlipEvent(isBackflip: false, totalFlipsInCurrentJump));
                }
            }

            _lastZRotation = currentZRotation;
            wasGroundedLastFrame = isGrounded;
        }
    }
}
