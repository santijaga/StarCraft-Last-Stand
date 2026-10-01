using System.Collections.Generic;
using UnityEngine;

namespace LastStand
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Enemy : MonoBehaviour
    {
        private static readonly List<Enemy> active = new List<Enemy>();
        public static IReadOnlyList<Enemy> Active => active;

        [SerializeField, Min(1)] private int hitsToDie = 3;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.3f;
        [SerializeField, Min(0.1f)] protected float attackDistance = 0.65f;
        [SerializeField, Min(0.1f)] private float attackInterval = 1f;
        [SerializeField, Min(0.1f)] private float appearDistance = 6f;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 10f;
        [SerializeField] protected Sprite[] runFrames;
        [SerializeField] private Sprite deathSprite;
        [SerializeField] private AudioClip appearSound;
        [SerializeField] private AudioClip[] appearSounds;
        [SerializeField] protected AudioClip strikeSound;
        [SerializeField] private AudioClip[] deathSounds;
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

        protected PlayerHealth target;
        protected SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private int hitsRemaining;
        private float nextAttackTime;
        private float framePosition;
        private bool announced;
        public bool IsDead { get; private set; }
        public float HitRadius => hitRadius;
        public virtual float VisibilityRadius
        {
            get
            {
                float radius = hitRadius;
                if (runFrames != null)
                    foreach (var frame in runFrames)
                        if (frame != null)
                            radius = Mathf.Max(radius, frame.bounds.center.magnitude + frame.bounds.extents.magnitude);
                return radius * Mathf.Max(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => active.Clear();

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitsRemaining = Mathf.Max(1, hitsToDie);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = volume;
        }

        protected virtual void OnEnable() { if (!IsDead) active.Add(this); }
        protected virtual void OnDisable() => active.Remove(this);
        public void Initialize(PlayerHealth player) => target = player;

        protected virtual void Update()
        {
            if (IsDead || target == null || target.IsDead || Time.timeScale <= 0f)
                return;

            Vector2 offset = target.transform.position - transform.position;
            float distance = offset.magnitude;
            if (distance > 0.001f)
                transform.rotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg - 90f);

            if (!announced && distance <= appearDistance)
            {
                announced = true;
                PlaySound(appearSounds != null && appearSounds.Length > 0
                    ? appearSounds[Random.Range(0, appearSounds.Length)] : appearSound);
            }

            if (distance > attackDistance)
            {
                float step = Mathf.Min(moveSpeed * Time.deltaTime, distance - attackDistance);
                transform.position += (Vector3)(offset.normalized * step);
                if (runFrames != null && runFrames.Length > 0)
                {
                    framePosition = Mathf.Repeat(framePosition + framesPerSecond * Time.deltaTime, runFrames.Length);
                    spriteRenderer.sprite = runFrames[Mathf.FloorToInt(framePosition)];
                }
            }

            if (Vector2.Distance(transform.position, target.transform.position) <= attackDistance + 0.001f
                && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + Mathf.Max(0.1f, attackInterval);
                Attack();
            }
        }

        protected virtual void Attack()
        {
            if (target.TakeHit()) PlaySound(strikeSound);
        }

        public void TakeHit()
        {
            if (IsDead) return;
            if (--hitsRemaining > 0) return;
            IsDead = true;
            spriteRenderer.sprite = deathSprite;
            spriteRenderer.sortingOrder = 1;
            if (deathSounds != null && deathSounds.Length > 0)
                PlaySound(deathSounds[Random.Range(0, deathSounds.Length)]);
            // Keep only an inert corpse in the scene; it is removed from projectile targeting.
            enabled = false;
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
