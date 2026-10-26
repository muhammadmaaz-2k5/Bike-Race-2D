using System.Collections.Generic;
using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Ghost
{
    /// <summary>
    /// Replays previously saved best-run ghost frames as a semi-transparent bike.
    /// Controlled by the Ghost toggle in Settings.
    /// </summary>
    public class GhostPlayer : MonoBehaviour
    {
        [Header("Ghost Visuals")]
        [SerializeField] private GameObject ghostVisualInstance;
        [SerializeField] private Color ghostTint = new Color(0.4f, 0.8f, 1.0f, 0.45f);

        private List<GhostFrame> _frames;
        private int _playbackIndex = 0;
        private bool _isPlaying = false;

        public void InitializeGhost(List<GhostFrame> frames)
        {
            var save = SaveSystem.Instance;
            if (save != null && !save.Data.ghostEnabled)
            {
                gameObject.SetActive(false);
                return;
            }

            if (frames == null || frames.Count < 2)
            {
                gameObject.SetActive(false);
                return;
            }

            _frames = frames;
            _playbackIndex = 0;
            _isPlaying = true;
            gameObject.SetActive(true);

            ApplyGhostVisualShading();
        }

        private void ApplyGhostVisualShading()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = ghostTint;
                    }
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", ghostTint);
                    }
                }
            }
        }

        private void Update()
        {
            if (!_isPlaying || _frames == null || _playbackIndex >= _frames.Count - 1) return;

            var gm = GameManager.Instance;
            if (gm == null || !gm.IsRaceActive) return;

            float raceTime = gm.RaceTimer;

            // Advance playback index until current raceTime is between playbackIndex and playbackIndex+1
            while (_playbackIndex < _frames.Count - 2 && _frames[_playbackIndex + 1].time < raceTime)
            {
                _playbackIndex++;
            }

            GhostFrame current = _frames[_playbackIndex];
            GhostFrame next = _frames[_playbackIndex + 1];

            float segmentDuration = next.time - current.time;
            float t = (segmentDuration > 0.0001f) ? Mathf.Clamp01((raceTime - current.time) / segmentDuration) : 0f;

            // Interpolate position and rotation
            Vector3 pos = Vector3.Lerp(new Vector3(current.posX, current.posY, 0.1f), new Vector3(next.posX, next.posY, 0.1f), t);
            float rotZ = Mathf.LerpAngle(current.rotZ, next.rotZ, t);

            transform.position = pos;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        }
    }
}
