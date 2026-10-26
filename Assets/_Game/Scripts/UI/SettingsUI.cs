using UnityEngine;
using UnityEngine.UI;
using DirtDash.Core;

namespace DirtDash.UI
{
    /// <summary>
    /// Settings dialog managing toggles for Music, SFX, Ghost, 4-Digit Times,
    /// Solid Ground, Tilt vs Buttons, Vibration, plus URLs and Reset Progress.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Toggles")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle ghostToggle;
        [SerializeField] private Toggle fourDigitsToggle;
        [SerializeField] private Toggle solidGroundToggle;
        [SerializeField] private Toggle tiltControlsToggle;
        [SerializeField] private Toggle vibrationToggle;

        [Header("Buttons")]
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private Button closeButton;

        private const string PrivacyPolicyUrl = "https://example.com/dirtdash/privacy";

        private void OnEnable()
        {
            LoadCurrentSettings();
            SetupListeners();
        }

        private void LoadCurrentSettings()
        {
            var save = SaveSystem.Instance;
            if (save == null) return;

            if (musicToggle != null) musicToggle.isOn = save.Data.musicEnabled;
            if (sfxToggle != null) sfxToggle.isOn = save.Data.sfxEnabled;
            if (ghostToggle != null) ghostToggle.isOn = save.Data.ghostEnabled;
            if (fourDigitsToggle != null) fourDigitsToggle.isOn = save.Data.fourDigitTimes;
            if (solidGroundToggle != null) solidGroundToggle.isOn = save.Data.solidGround;
            if (tiltControlsToggle != null) tiltControlsToggle.isOn = save.Data.useTiltControls;
            if (vibrationToggle != null) vibrationToggle.isOn = save.Data.vibrationEnabled;
        }

        private void SetupListeners()
        {
            if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicChanged);
            if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(OnSfxChanged);
            if (ghostToggle != null) ghostToggle.onValueChanged.AddListener(OnGhostChanged);
            if (fourDigitsToggle != null) fourDigitsToggle.onValueChanged.AddListener(OnFourDigitsChanged);
            if (solidGroundToggle != null) solidGroundToggle.onValueChanged.AddListener(OnSolidGroundChanged);
            if (tiltControlsToggle != null) tiltControlsToggle.onValueChanged.AddListener(OnTiltControlsChanged);
            if (vibrationToggle != null) vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);

            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);
            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(OnResetProgressClicked);
            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnMusicChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.musicEnabled = val; save.Save(); }
        }

        private void OnSfxChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.sfxEnabled = val; save.Save(); }
        }

        private void OnGhostChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.ghostEnabled = val; save.Save(); }
        }

        private void OnFourDigitsChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.fourDigitTimes = val; save.Save(); }
        }

        private void OnSolidGroundChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.solidGround = val; save.Save(); }
        }

        private void OnTiltControlsChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.useTiltControls = val; save.Save(); }
        }

        private void OnVibrationChanged(bool val)
        {
            var save = SaveSystem.Instance;
            if (save != null) { save.Data.vibrationEnabled = val; save.Save(); }
        }

        private void OnPrivacyPolicyClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Application.OpenURL(PrivacyPolicyUrl);
        }

        private void OnCreditsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Debug.Log("[Dirt Dash] Developed with Unity 2D physics & URP.");
        }

        private void OnResetProgressClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SaveSystem.Instance?.ResetAllProgress();
            LoadCurrentSettings();
        }
    }
}
