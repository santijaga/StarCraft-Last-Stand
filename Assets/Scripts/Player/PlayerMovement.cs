using UnityEngine;
using UnityEngine.InputSystem;

namespace LastStand
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [Tooltip("Optional axis-aligned arena boundary. Keeps the player inside the field.")]
        [SerializeField] private BoxCollider2D arenaBounds;
        [SerializeField, Min(0f)] private float boundaryPadding = 0.5f;

        private Rigidbody2D body;
        private InputAction moveAction;
        private PlayerShooting shooting;
        public Vector2 Velocity => body == null ? Vector2.zero : body.linearVelocity;
        public float MoveSpeed => moveSpeed;
        public BoxCollider2D ArenaBounds => arenaBounds;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            shooting = GetComponent<PlayerShooting>();
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        }

        private void OnEnable() => moveAction.Enable();

        private void FixedUpdate()
        {
            float speedMultiplier = shooting != null ? shooting.MovementSpeedMultiplier : 1f;
            Vector2 velocity = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
                * moveSpeed * speedMultiplier;
            if (arenaBounds != null)
            {
                Bounds bounds = arenaBounds.bounds;
                Vector2 next = body.position + velocity * Time.fixedDeltaTime;
                float paddingX = Mathf.Min(boundaryPadding, bounds.extents.x);
                float paddingY = Mathf.Min(boundaryPadding, bounds.extents.y);
                next.x = Mathf.Clamp(next.x, bounds.min.x + paddingX, bounds.max.x - paddingX);
                next.y = Mathf.Clamp(next.y, bounds.min.y + paddingY, bounds.max.y - paddingY);
                velocity = (next - body.position) / Time.fixedDeltaTime;
            }
            body.linearVelocity = velocity;
        }

        private void OnDisable()
        {
            moveAction.Disable();
            body.linearVelocity = Vector2.zero;
        }

        private void OnDestroy() => moveAction?.Dispose();
    }
}
