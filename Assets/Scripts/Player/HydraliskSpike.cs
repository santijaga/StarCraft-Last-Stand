using UnityEngine;

namespace LastStand
{
    [DefaultExecutionOrder(50)]
    public sealed class HydraliskSpike : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 5f;
        [SerializeField, Min(0f)] private float hitRadius = 0.09f;
        private Vector3 velocity;
        private PlayerHealth target;
        private CircleCollider2D targetCollider;

        public void Launch(Vector2 direction, float speed, PlayerHealth player)
        {
            target = player;
            targetCollider = player.GetComponent<CircleCollider2D>();
            direction.Normalize();
            velocity = direction * Mathf.Max(0f, speed);
            transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            Destroy(gameObject, Mathf.Max(0.1f, lifetime));
        }

        private void Update()
        {
            if (target == null || target.IsDead)
            {
                Destroy(gameObject);
                return;
            }
            Vector2 step = velocity * Time.deltaTime;
            if (step.sqrMagnitude <= 0f) return;
            Vector2 origin = transform.position;
            Vector2 centre = targetCollider != null
                ? (Vector2)targetCollider.transform.TransformPoint(targetCollider.offset)
                : (Vector2)target.transform.position;
            float playerRadius = targetCollider != null ? targetCollider.radius
                * Mathf.Max(Mathf.Abs(targetCollider.transform.lossyScale.x), Mathf.Abs(targetCollider.transform.lossyScale.y)) : 0.3f;
            float fraction = Mathf.Clamp01(Vector2.Dot(centre - origin, step) / step.sqrMagnitude);
            Vector2 nearest = origin + step * fraction;
            float radius = hitRadius + playerRadius;
            if ((centre - nearest).sqrMagnitude <= radius * radius)
            {
                target.TakeHit();
                Destroy(gameObject);
                return;
            }
            transform.position += (Vector3)step;
        }
    }
}
