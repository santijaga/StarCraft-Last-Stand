using UnityEngine;

namespace LastStand
{
    public sealed class Hydralisk : Enemy
    {
        [SerializeField] private HydraliskSpike spikePrefab;
        [SerializeField] private Sprite shootSprite;
        [SerializeField, Min(0.1f)] private float spikeSpeed = 8f;
        [SerializeField, Min(0.01f)] private float shootSpriteDuration = 0.15f;
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0f, 0.6f);
        private float shootingUntil;

        public override float VisibilityRadius => Mathf.Max(base.VisibilityRadius,
            shootSprite == null ? 0f : (shootSprite.bounds.center.magnitude + shootSprite.bounds.extents.magnitude)
                * Mathf.Max(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y)));

        protected override void Attack()
        {
            if (spikePrefab == null) return;
            Vector3 origin = transform.TransformPoint(muzzleOffset);
            Vector2 direction = target.transform.position - origin;
            if (direction.sqrMagnitude < 0.0001f) direction = transform.up;
            Instantiate(spikePrefab, origin, Quaternion.identity).Launch(direction, spikeSpeed, target);
            shootingUntil = Time.time + shootSpriteDuration;
            PlaySound(strikeSound);
        }

        protected override void Update()
        {
            base.Update();
            if (IsDead) return;
            if (Time.time < shootingUntil && shootSprite != null)
                spriteRenderer.sprite = shootSprite;
            else if (target != null && (target.IsDead
                || Vector2.Distance(transform.position, target.transform.position) <= attackDistance + 0.001f)
                && runFrames != null && runFrames.Length > 0)
                spriteRenderer.sprite = runFrames[0];
        }
    }
}
