using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>Restores health on touch. It stays in place while the player is at full health.</summary>
    public sealed class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 35;
        [SerializeField] private Vector2 size = new Vector2(1.2f, 1.6f);
        [SerializeField] private LayerMask playerLayers;

        private void FixedUpdate()
        {
            Collider2D touching = UnityEngine.Physics2D.OverlapBox(transform.position, size, 0f, playerLayers);
            if (touching == null)
            {
                return;
            }

            var player = touching.GetComponentInParent<PlayerCombat>();
            if (player != null && player.CurrentHealth < player.MaxHealth)
            {
                player.Heal(amount);
                gameObject.SetActive(false);
            }
        }
    }
}
