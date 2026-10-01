using UnityEngine;

namespace LastStand
{
    [RequireComponent(typeof(PlayerShooting))]
    public sealed class PlayerAudio : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip rifleShot;
        [SerializeField] private AudioClip goGoGo;
        [SerializeField] private AudioClip[] combatVoiceClips;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
        [SerializeField, Range(0f, 1f)] private float shotVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 1f;

        [Header("Combat chatter")]
        [Tooltip("Random delay after a voice line ends. A new line only starts on an actual shot.")]
        [SerializeField] private Vector2 voiceCooldown = new Vector2(10f, 20f);

        private PlayerShooting shooting;
        private AudioSource musicSource;
        private AudioSource shotSource;
        private AudioSource voiceSource;
        private AudioSource reactionSource;
        private float nextVoiceTime;

        private void Awake()
        {
            shooting = GetComponent<PlayerShooting>();
            // Separate 2D sources keep weapon playback independent of music and dialogue.
            musicSource = CreateSource(128);
            shotSource = CreateSource(128);
            voiceSource = CreateSource(64);
            reactionSource = CreateSource(32);
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            ApplyVolumes();
        }

        private AudioSource CreateSource(int priority)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.priority = priority;
            return source;
        }

        private void OnEnable()
        {
            shooting.ShotFired += OnShotFired;
            if (backgroundMusic != null)
                musicSource.Play();
        }

        private void Start() => PlayVoice(goGoGo);

        public void PlayHit()
        {
            if (hitSound == null) return;
            reactionSource.Stop();
            reactionSource.clip = hitSound;
            reactionSource.Play();
            // Play only the first 20% of the recording, using the audio clock.
            reactionSource.SetScheduledEndTime(AudioSettings.dspTime + hitSound.length * 0.2);
        }

        public void PlayDeath()
        {
            shotSource.Stop();
            voiceSource.Stop();
            reactionSource.Stop();
            if (deathSound == null) return;
            reactionSource.clip = deathSound;
            reactionSource.Play();
        }

        private void OnShotFired()
        {
            if (rifleShot != null)
            {
                shotSource.Stop();
                shotSource.clip = rifleShot;
                shotSource.Play();
            }

            if (voiceSource.isPlaying || Time.time < nextVoiceTime)
                return;

            if (combatVoiceClips == null || combatVoiceClips.Length == 0)
                return;

            PlayVoice(combatVoiceClips[Random.Range(0, combatVoiceClips.Length)]);
        }

        private void PlayVoice(AudioClip clip)
        {
            if (clip != null)
            {
                voiceSource.clip = clip;
                voiceSource.Play();
            }
            float minDelay = Mathf.Max(1f, voiceCooldown.x);
            float maxDelay = Mathf.Max(minDelay, voiceCooldown.y);
            nextVoiceTime = Time.time + (clip != null ? clip.length : 0f)
                + Random.Range(minDelay, maxDelay);
        }

        private void OnValidate()
        {
            voiceCooldown.x = Mathf.Max(1f, voiceCooldown.x);
            voiceCooldown.y = Mathf.Max(voiceCooldown.x, voiceCooldown.y);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (musicSource != null) musicSource.volume = musicVolume;
            if (shotSource != null) shotSource.volume = shotVolume;
            if (voiceSource != null) voiceSource.volume = voiceVolume;
            if (reactionSource != null) reactionSource.volume = voiceVolume;
        }

        private void OnDisable()
        {
            shooting.ShotFired -= OnShotFired;
            musicSource.Stop();
            shotSource.Stop();
            voiceSource.Stop();
            reactionSource.Stop();
        }
    }
}
