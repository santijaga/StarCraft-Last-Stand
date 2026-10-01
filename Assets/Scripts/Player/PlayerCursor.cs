using UnityEngine;

namespace LastStand
{
    public sealed class PlayerCursor : MonoBehaviour
    {
        [SerializeField] private Texture2D crosshairTexture;

        private void OnEnable() => ApplyCursor();

        private void OnDisable() => ResetCursor();

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && isActiveAndEnabled)
                ApplyCursor();
            else
                ResetCursor();
        }

        private void ApplyCursor()
        {
            if (crosshairTexture == null)
                return;

            // The centre of the crosshair is the exact mouse position used by PlayerAim.
            Vector2 hotspot = new Vector2(crosshairTexture.width * 0.5f, crosshairTexture.height * 0.5f);
            Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.Auto);
        }

        private static void ResetCursor() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
