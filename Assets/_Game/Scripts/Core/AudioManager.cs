using UnityEngine;

namespace DirtDash.Core
{
    /// <summary>
    /// Central audio manager with dedicated Music and SFX buses.
    /// Includes procedural audio synthesis fallbacks so all sound effects and engine tones
    /// play immediately even before external audio clips are assigned.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource engineSource;

        [Header("Configured Audio Clips (Optional overrides)")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip coinClip;
        [SerializeField] private AudioClip crashClip;
        [SerializeField] private AudioClip starClip;
        [SerializeField] private AudioClip brakeClip;
        [SerializeField] private AudioClip landingClip;
        [SerializeField] private AudioClip winJingleClip;

        // Procedurally generated fallback clips
        private AudioClip _procClick;
        private AudioClip _procCoin;
        private AudioClip _procCrash;
        private AudioClip _procStar;
        private AudioClip _procBrake;
        private AudioClip _procLanding;
        private AudioClip _procWin;
        private AudioClip _procEngine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            SetupAudioSources();
            GenerateProceduralAudio();
        }

        private void SetupAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.loop = true;
                engineSource.playOnAwake = false;
            }
        }

        #region Sound Effect Triggers

        public void PlayButtonClick()
        {
            PlayClip(buttonClickClip != null ? buttonClickClip : _procClick, 0.7f);
        }

        public void PlayCoinPickup()
        {
            PlayClip(coinClip != null ? coinClip : _procCoin, 0.85f);
        }

        public void PlayCrash()
        {
            PlayClip(crashClip != null ? crashClip : _procCrash, 1.0f);
        }

        public void PlayStarEarned()
        {
            PlayClip(starClip != null ? starClip : _procStar, 0.9f);
        }

        public void PlayBrakeScreech()
        {
            PlayClip(brakeClip != null ? brakeClip : _procBrake, 0.65f);
        }

        public void PlayLandingImpact()
        {
            PlayClip(landingClip != null ? landingClip : _procLanding, 0.75f);
        }

        public void PlayWinJingle()
        {
            PlayClip(winJingleClip != null ? winJingleClip : _procWin, 0.9f);
        }

        private void PlayClip(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || sfxSource == null) return;
            var save = SaveSystem.Instance;
            if (save != null && !save.Data.sfxEnabled) return;

            float vol = (save != null ? save.Data.sfxVolume : 1.0f) * volumeScale;
            sfxSource.PlayOneShot(clip, vol);
        }

        #endregion

        #region Engine Audio Simulation

        public void StartEngineSound()
        {
            if (engineSource == null) return;
            var save = SaveSystem.Instance;
            if (save != null && !save.Data.sfxEnabled) return;

            if (!engineSource.isPlaying)
            {
                engineSource.clip = _procEngine;
                engineSource.volume = (save != null ? save.Data.sfxVolume : 1f) * 0.45f;
                engineSource.pitch = 0.8f;
                engineSource.Play();
            }
        }

        public void UpdateEnginePitch(float speedKmh, bool isThrottlePressed)
        {
            if (engineSource == null || !engineSource.isPlaying) return;

            float targetPitch = 0.8f + (speedKmh / 35f) * 1.4f;
            if (isThrottlePressed) targetPitch += 0.2f;

            engineSource.pitch = Mathf.Lerp(engineSource.pitch, Mathf.Clamp(targetPitch, 0.75f, 2.5f), Time.deltaTime * 6f);
        }

        public void StopEngineSound()
        {
            if (engineSource != null && engineSource.isPlaying)
            {
                engineSource.Stop();
            }
        }

        #endregion

        #region Procedural Audio Synthesizer

        private void GenerateProceduralAudio()
        {
            _procClick = CreateTone(880f, 0.05f, ToneType.Sine);
            _procCoin = CreateArpeggio(new float[] { 987.77f, 1318.51f }, 0.12f);
            _procCrash = CreateNoise(0.45f);
            _procStar = CreateArpeggio(new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.35f);
            _procBrake = CreateNoise(0.2f, lowPass: true);
            _procLanding = CreateTone(120f, 0.15f, ToneType.Triangle);
            _procWin = CreateArpeggio(new float[] { 440f, 554f, 659f, 880f }, 0.6f);
            _procEngine = CreateEngineLoop();
        }

        private enum ToneType { Sine, Triangle, Noise }

        private AudioClip CreateTone(float frequency, float duration, ToneType type)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - ((float)i / samples); // Linear decay
                float wave = Mathf.Sin(2 * Mathf.PI * frequency * t);
                data[i] = wave * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("ProcTone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateArpeggio(float[] frequencies, float totalDuration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * totalDuration);
            float[] data = new float[totalSamples];
            int noteSamples = totalSamples / frequencies.Length;

            for (int n = 0; n < frequencies.Length; n++)
            {
                float freq = frequencies[n];
                int start = n * noteSamples;
                int end = (n == frequencies.Length - 1) ? totalSamples : (n + 1) * noteSamples;

                for (int i = start; i < end; i++)
                {
                    float t = (float)(i - start) / sampleRate;
                    float env = 1f - ((float)(i - start) / (end - start));
                    data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.35f;
                }
            }

            AudioClip clip = AudioClip.Create("ProcArp", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNoise(float duration, bool lowPass = false)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            float last = 0f;

            for (int i = 0; i < samples; i++)
            {
                float raw = Random.Range(-1f, 1f);
                if (lowPass)
                {
                    last = Mathf.Lerp(last, raw, 0.15f);
                    raw = last;
                }
                float env = 1f - ((float)i / samples);
                data[i] = raw * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create("ProcNoise", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateEngineLoop()
        {
            int sampleRate = 44100;
            float duration = 0.5f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Motor tone: rich blend of 70Hz fundamental and harmonics
                float w1 = Mathf.Sin(2 * Mathf.PI * 75f * t);
                float w2 = Mathf.Sin(2 * Mathf.PI * 150f * t) * 0.5f;
                float w3 = Mathf.Sin(2 * Mathf.PI * 225f * t) * 0.25f;
                data[i] = (w1 + w2 + w3) * 0.25f;
            }

            AudioClip clip = AudioClip.Create("ProcEngine", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        #endregion
    }
}
