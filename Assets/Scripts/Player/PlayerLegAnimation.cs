using UnityEngine;

namespace LastStand
{
    [DefaultExecutionOrder(210)]
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerAim))]
    public sealed class PlayerLegAnimation : MonoBehaviour
    {
        [SerializeField] private Transform lowerBody;
        [SerializeField] private SpriteRenderer legRenderer;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite idleSprite;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 10f;
        [SerializeField] private float spriteAngleOffset = -90f;

        private PlayerMovement movement;
        private PlayerAim aim;
        private float framePosition;
        public bool IsMoving { get; private set; }
        public bool IsMovingBackwards { get; private set; }

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            aim = GetComponent<PlayerAim>();
        }

        public static bool IsBackwards(Vector2 movementDirection, Vector2 facingDirection)
            => Vector2.Dot(movementDirection, facingDirection) < 0f;

        private void LateUpdate()
        {
            if (lowerBody == null || legRenderer == null || walkFrames == null || walkFrames.Length == 0)
                return;

            Vector2 velocity = movement.Velocity;
            IsMoving = velocity.sqrMagnitude > 0.0001f;
            IsMovingBackwards = IsMoving && IsBackwards(velocity, aim.FacingDirection);
            if (!IsMoving)
            {
                framePosition = 0f;
                legRenderer.sprite = idleSprite != null ? idleSprite : walkFrames[0];
                return;
            }

            // In the rear hemisphere the feet face opposite travel and the cycle runs backwards.
            Vector2 legDirection = IsMovingBackwards ? -velocity : velocity;
            lowerBody.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(legDirection.y, legDirection.x) * Mathf.Rad2Deg + spriteAngleOffset);
            float speedRatio = velocity.magnitude / Mathf.Max(movement.MoveSpeed, 0.001f);
            float sign = IsMovingBackwards ? -1f : 1f;
            framePosition = Mathf.Repeat(framePosition + sign * framesPerSecond * speedRatio * Time.deltaTime,
                walkFrames.Length);
            legRenderer.sprite = walkFrames[Mathf.FloorToInt(framePosition)];
        }
    }
}
