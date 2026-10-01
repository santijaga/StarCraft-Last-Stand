using UnityEngine;

namespace LastStand
{
    // Check visibility after Cinemachine has positioned the camera for this frame.
    [DefaultExecutionOrder(230)]
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth))]
    public sealed class ZerglingSpawner : MonoBehaviour
    {
        [SerializeField] private Zergling enemyPrefab;
        [SerializeField] private Hydralisk hydraliskPrefab;
        [SerializeField, Range(0f, 100f)] private float hydraliskChancePercent = 25f;
        [SerializeField, Min(0.1f)] private float spawnInterval = 1.2f;
        [SerializeField, Min(0f)] private float initialDelay = 2f;
        [SerializeField, Min(1)] private int maxAlive = 50;
        [SerializeField, Min(0f)] private float visibilityPadding = 0.5f;
        private PlayerHealth player;
        private BoxCollider2D arena;
        private Camera gameCamera;
        private float nextSpawnTime;
        private readonly Plane[] cameraPlanes = new Plane[6];

        private void Start()
        {
            player = GetComponent<PlayerHealth>();
            arena = GetComponent<PlayerMovement>().ArenaBounds;
            gameCamera = Camera.main;
            nextSpawnTime = Time.time + initialDelay;
        }

        private void LateUpdate()
        {
            if (player == null || player.IsDead || arena == null || gameCamera == null
                || enemyPrefab == null || Time.timeScale <= 0f || Time.time < nextSpawnTime)
                return;
            nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
            if (Enemy.Active.Count >= maxAlive) return;

            Enemy selectedPrefab = hydraliskPrefab != null && Random.value * 100f < hydraliskChancePercent
                ? (Enemy)hydraliskPrefab : enemyPrefab;

            Bounds map = arena.bounds;
            GeometryUtility.CalculateFrustumPlanes(gameCamera, cameraPlanes);
            // A square around the sprite's bounding circle also covers every rotation.
            float radius = selectedPrefab.VisibilityRadius + visibilityPadding;
            float perimeter = 2f * (map.size.x + map.size.y);
            for (int attempt = 0; attempt < 64; attempt++)
            {
                float edge = Random.Range(0f, perimeter);
                Vector3 position;
                if (edge < map.size.x)
                    position = new Vector3(map.min.x + edge, map.min.y, transform.position.z);
                else if ((edge -= map.size.x) < map.size.y)
                    position = new Vector3(map.max.x, map.min.y + edge, transform.position.z);
                else if ((edge -= map.size.y) < map.size.x)
                    position = new Vector3(map.max.x - edge, map.max.y, transform.position.z);
                else
                    position = new Vector3(map.min.x, map.max.y - (edge - map.size.x), transform.position.z);

                if (GeometryUtility.TestPlanesAABB(cameraPlanes, new Bounds(position, Vector3.one * radius * 2f)))
                    continue;
                Instantiate(selectedPrefab, position, Quaternion.identity).Initialize(player);
                return;
            }
            // Never fall back to a visible point when no valid edge was found.
        }
    }
}
