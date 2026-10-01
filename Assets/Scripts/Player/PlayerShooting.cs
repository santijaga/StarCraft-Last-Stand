using UnityEngine;
using UnityEngine.InputSystem;

namespace LastStand
{
    // Fire after PlayerAim has updated the cursor position and the upper-body rotation.
    [DefaultExecutionOrder(220)]
    [RequireComponent(typeof(PlayerAim))]
    public sealed class PlayerShooting : MonoBehaviour
    {
        public event System.Action ShotFired;

        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private SpriteRenderer upperBodyRenderer;
        [SerializeField] private Sprite[] shotSprites;
        [Tooltip("Spawn point in the upper body's local space, at the end of the gun.")]
        [SerializeField] private Vector2 muzzleOffset = new Vector2(0.125f, 0.375f);
        [Tooltip("Maximum number of shots per second, including repeated clicks.")]
        [SerializeField, Min(0.1f)] private float fireRate = 6f;
        [Tooltip("Projectile speed in world units per second.")]
        [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;
        [SerializeField, Min(0.01f)] private float shotSpriteDuration = 0.06f;
        [Tooltip("Movement speed reduction while firing, in percent. 0 = full speed, 100 = stationary.")]
        [SerializeField, Range(0f, 100f)] private float movementSlowdownPercent = 30f;

        private PlayerAim aim;
        private Sprite restingSprite;
        private float nextShotTime;
        private float restoreSpriteTime;
        private bool showingShot;
        private bool firing;
        private float slowdownUntil;

        public float MovementSpeedMultiplier => isActiveAndEnabled && Application.isFocused
            && Time.timeScale > 0f && (firing || Time.time < slowdownUntil)
            ? 1f - Mathf.Clamp01(movementSlowdownPercent / 100f) : 1f;

        private void Awake()
        {
            aim = GetComponent<PlayerAim>();
            if (upperBodyRenderer != null)
                restingSprite = upperBodyRenderer.sprite;
        }

        private void LateUpdate()
        {
            firing = false;
            if (showingShot && Time.time >= restoreSpriteTime)
                RestoreSprite();

            if (Time.timeScale <= 0f || !Application.isFocused || Mouse.current == null
                || !aim.HasAimPosition || projectilePrefab == null || upperBodyRenderer == null)
                return;

            var trigger = Mouse.current.leftButton;
            firing = trigger.isPressed || trigger.wasPressedThisFrame;
            if (!firing || Time.time < nextShotTime)
                return;

            Vector3 origin = upperBodyRenderer.transform.TransformPoint(muzzleOffset);
            Vector2 direction = aim.AimPosition - origin;
            if (direction.sqrMagnitude < 0.0001f)
                direction = aim.FacingDirection;

            Projectile projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, projectileSpeed);
            float interval = 1f / Mathf.Max(0.1f, fireRate);
            nextShotTime = Time.time + interval;
            // Keep a single click slowed for one firing cycle, too.
            slowdownUntil = nextShotTime;
            ShotFired?.Invoke();

            if (shotSprites != null && shotSprites.Length > 0)
            {
                upperBodyRenderer.sprite = shotSprites[Random.Range(0, shotSprites.Length)];
                restoreSpriteTime = Time.time + Mathf.Min(shotSpriteDuration, interval * 0.8f);
                showingShot = true;
            }
        }

        private void OnDisable()
        {
            firing = false;
            slowdownUntil = 0f;
            RestoreSprite();
        }

        private void RestoreSprite()
        {
            if (showingShot && upperBodyRenderer != null)
                upperBodyRenderer.sprite = restingSprite;
            showingShot = false;
        }
    }
}
