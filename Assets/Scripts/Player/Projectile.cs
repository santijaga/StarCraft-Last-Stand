using UnityEngine;

namespace LastStand
{
    [DefaultExecutionOrder(50)]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 3f;
        [SerializeField, Min(0f)] private float hitRadius = 0.07f;
        private Vector3 velocity;

        public void Launch(Vector2 direction, float speed)
        {
            direction = direction.normalized;
            velocity = direction * Mathf.Max(0f, speed);
            transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            Destroy(gameObject, Mathf.Max(0.1f, lifetime));
        }

        private void Update()
        {
            Vector2 step = velocity * Time.deltaTime;
            float distance = step.magnitude;
            if (distance <= 0f) return;
            Vector2 direction = step / distance;
            Vector2 origin = transform.position;
            Enemy closest = null;
            float closestDistance = distance;
            // Sweep the full travelled segment so fast bullets cannot skip small targets.
            foreach (var enemy in Enemy.Active)
            {
                Vector2 offset = (Vector2)enemy.transform.position - origin;
                float radius = hitRadius + enemy.HitRadius;
                float projection = Vector2.Dot(offset, direction);
                float discriminant = radius * radius - (offset.sqrMagnitude - projection * projection);
                if (discriminant < 0f) continue;
                float root = Mathf.Sqrt(discriminant);
                if (projection + root < 0f) continue;
                float entry = Mathf.Max(0f, projection - root);
                if (entry > closestDistance) continue;
                closest = enemy;
                closestDistance = entry;
            }
            if (closest != null)
            {
                transform.position += (Vector3)(direction * closestDistance);
                closest.TakeHit();
                Destroy(gameObject);
                return;
            }
            transform.position += (Vector3)step;
        }
    }
}
