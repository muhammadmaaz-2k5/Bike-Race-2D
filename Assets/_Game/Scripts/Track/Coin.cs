using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Track
{
    /// <summary>
    /// Collectible coin pickup with bobbing and spinning visual animation.
    /// Rewards currency when collected by the player bike.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        [Header("Coin Settings")]
        [SerializeField] private int coinValue = 1;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 2.5f;

        [Header("Visuals")]
        [SerializeField] private Transform visualTransform;

        private Vector3 _startLocalPos;
        private bool _isCollected = false;

        private void Start()
        {
            if (visualTransform == null) visualTransform = transform;
            _startLocalPos = visualTransform.localPosition;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Spin animation around Y axis
            if (visualTransform != null)
            {
                visualTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                // Vertical gentle floating bob
                float newY = _startLocalPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                visualTransform.localPosition = new Vector3(_startLocalPos.x, newY, _startLocalPos.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;

            if (other.CompareTag(GameConstants.TagPlayer) || other.transform.root.CompareTag(GameConstants.TagPlayer))
            {
                Collect();
            }
        }

        private void Collect()
        {
            _isCollected = true;

            SaveSystem.Instance?.AddCoins(coinValue);
            AudioManager.Instance?.PlayCoinPickup();

            gameObject.SetActive(false);
        }

        public void ResetCoin()
        {
            _isCollected = false;
            gameObject.SetActive(true);
        }
    }
}
