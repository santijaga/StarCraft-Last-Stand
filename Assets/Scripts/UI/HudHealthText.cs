using UnityEngine;
using UnityEngine.UI;

namespace LastStand
{
    [DefaultExecutionOrder(300)]
    [RequireComponent(typeof(Text))]
    public sealed class HudHealthText : MonoBehaviour
    {
        private PlayerHealth player;
        private Text label;

        private void OnEnable()
        {
            label = GetComponent<Text>();
            player = FindAnyObjectByType<PlayerHealth>();
            if (player != null) player.HealthChanged += Refresh;
            Refresh();
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (player != null) player.HealthChanged -= Refresh;
        }

        private void Refresh()
        {
            if (player == null)
            {
                label.text = "--/--";
                label.color = new Color32(0xc8, 0xc8, 0xc8, 0xff);
                return;
            }

            int current = Mathf.Max(0, player.HitsRemaining);
            int maximum = Mathf.Max(1, player.MaxHits);
            label.text = $"{current}/{maximum}";
            float ratio = (float)current / maximum;
            label.color = current == 0 ? new Color32(0xc8, 0xc8, 0xc8, 0xff)
                : ratio > 0.5f ? new Color32(0x0e, 0xff, 0x13, 0xff)
                : ratio > 0.25f ? new Color32(0xee, 0xfa, 0x08, 0xff)
                : new Color32(0xfa, 0x07, 0x07, 0xff);
        }
    }
}
