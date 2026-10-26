using System.Collections;
using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Bike
{
    /// <summary>
    /// Detects rider head/body impacts with the ground and out-of-bounds falls.
    /// Publishes BikeCrashedEvent via EventBus and initiates quick restart sequence.
    /// </summary>
    public class CrashDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BikeController bikeController;
        [SerializeField] private Collider2D headCollider;

        [Header("Crash Detection Settings")]
        [SerializeField] private LayerMask fatalLayers = ~0; // Track, Ground, Obstacles
        [SerializeField] private float killHeightY = GameConstants.DefaultKillY;
        [SerializeField] private float autoRestartDelaySeconds = 1.0f;
        [SerializeField] private bool autoRestartOnCrash = true;

        [Header("Effects (Optional)")]
        [SerializeField] private GameObject crashVfxPrefab;

        private bool _hasCrashed = false;

        public bool HasCrashed => _hasCrashed;

        private void Awake()
        {
            if (bikeController == null)
            {
                bikeController = GetComponentInParent<BikeController>();
            }

            if (headCollider == null)
            {
                headCollider = GetComponent<Collider2D>();
            }
        }

        private void Update()
        {
            if (_hasCrashed) return;

            // Check out-of-bounds fall
            if (transform.position.y < killHeightY)
            {
                TriggerCrash("Fell out of bounds");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasCrashed) return;

            // Check if collider belongs to fatal layers (ground, obstacles)
            if (((1 << other.gameObject.layer) & fatalLayers) != 0)
            {
                // Verify it's not the bike's own parts
                if (other.transform.IsChildOf(bikeController.transform)) return;

                TriggerCrash($"Head impacted {other.gameObject.name}");
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasCrashed) return;

            if (((1 << collision.gameObject.layer) & fatalLayers) != 0)
            {
                if (collision.transform.IsChildOf(bikeController.transform)) return;

                TriggerCrash($"Body impacted {collision.gameObject.name}");
            }
        }

        /// <summary>
        /// Initiates the crash event sequence.
        /// </summary>
        public void TriggerCrash(string reason)
        {
            if (_hasCrashed) return;
            _hasCrashed = true;

            Debug.Log($"[Dirt Dash] CRASH: {reason} at {transform.position}");

            // Spawn optional crash burst particle
            if (crashVfxPrefab != null)
            {
                Instantiate(crashVfxPrefab, transform.position, Quaternion.identity);
            }

            EventBus.Publish(new BikeCrashedEvent(reason, transform.position));

            if (autoRestartOnCrash)
            {
                StartCoroutine(AutoRestartRoutine());
            }
        }

        private IEnumerator AutoRestartRoutine()
        {
            yield return new WaitForSeconds(autoRestartDelaySeconds);

            // If a GameManager exists, notify it; otherwise perform local respawn
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.RestartCurrentLevel();
            }
            else if (bikeController != null)
            {
                // Fallback for standalone test scenes
                ResetCrashState();
                bikeController.Respawn(Vector2.up * 1.5f, 0f);
            }
        }

        /// <summary>
        /// Resets the crash detector state upon respawn.
        /// </summary>
        public void ResetCrashState()
        {
            _hasCrashed = false;
        }
    }
}
