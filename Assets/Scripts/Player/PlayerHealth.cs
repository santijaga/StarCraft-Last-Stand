using UnityEngine;

namespace LastStand
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int hitsToDie = 10;
        [SerializeField] private Sprite deathSprite;
        [SerializeField] private SpriteRenderer upperBody;
        [SerializeField] private SpriteRenderer lowerBody;

        public bool IsDead { get; private set; }
        public int HitsRemaining { get; private set; }
        public int MaxHits { get; private set; }
        public event System.Action HealthChanged;

        private void Awake()
        {
            MaxHits = Mathf.Max(1, hitsToDie);
            HitsRemaining = MaxHits;
        }

        public bool TakeHit()
        {
            if (IsDead)
                return false;

            HitsRemaining--;
            HealthChanged?.Invoke();
            var audio = GetComponent<PlayerAudio>();
            if (HitsRemaining > 0)
            {
                if (audio != null) audio.PlayHit();
                return true;
            }

            IsDead = true;
            // Disable shooting before assigning the corpse sprite: it restores its resting sprite.
            DisableBehaviour<PlayerShooting>();
            DisableBehaviour<PlayerMovement>();
            DisableBehaviour<PlayerAim>();
            DisableBehaviour<PlayerLegAnimation>();
            DisableBehaviour<PlayerCursor>();
            if (lowerBody != null) lowerBody.enabled = false;
            if (upperBody != null)
            {
                upperBody.sprite = deathSprite;
                upperBody.sortingOrder = 2;
            }
            foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
            var body = GetComponent<Rigidbody2D>();
            if (body != null) body.simulated = false;
            if (audio != null) audio.PlayDeath();
            return true;
        }

        private void DisableBehaviour<T>() where T : Behaviour
        {
            var component = GetComponent<T>();
            if (component != null) component.enabled = false;
        }
    }
}
