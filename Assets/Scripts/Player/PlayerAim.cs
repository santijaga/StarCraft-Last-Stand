using UnityEngine;
using UnityEngine.InputSystem;

namespace LastStand
{
    // CinemachineBrain updates at 100: aim using the camera's final rendered position.
    [DefaultExecutionOrder(200)]
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Transform upperBody;
        [SerializeField] private Camera aimCamera;
        [Tooltip("The supplied marine sprite faces up (+Y).")]
        [SerializeField] private float spriteAngleOffset = -90f;

        public Vector2 FacingDirection { get; private set; } = Vector2.up;
        public Vector3 AimPosition { get; private set; }
        public bool HasAimPosition { get; private set; }

        private void LateUpdate()
        {
            HasAimPosition = false;
            if (aimCamera == null)
                aimCamera = Camera.main;
            if (aimCamera == null || upperBody == null || Mouse.current == null)
                return;

            Ray ray = aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.forward, transform.position);
            if (!plane.Raycast(ray, out float distance))
                return;

            AimPosition = ray.GetPoint(distance);
            HasAimPosition = true;
            Vector2 direction = AimPosition - transform.position;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            FacingDirection = direction.normalized;
            upperBody.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spriteAngleOffset);
        }
    }
}
