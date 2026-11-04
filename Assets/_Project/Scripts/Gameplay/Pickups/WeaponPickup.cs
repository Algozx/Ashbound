using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>A weapon on the ground. Taking it leaves the old weapon in its place.</summary>
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private Vector2 size = new Vector2(1.6f, 2f);
        [SerializeField] private LayerMask playerLayers;

        private PlayerCombat nearby;

        public WeaponDefinition Weapon => weapon;

        /// <summary>Swaps this pickup's weapon with the player's.</summary>
        public void Take(PlayerCombat player)
        {
            WeaponDefinition previous = player.Weapon;
            player.Equip(weapon);
            weapon = previous;
            ShowWeapon();
            player.SetNearbyPickup(this);
        }

        private void Awake()
        {
            ShowWeapon();
        }

        private void FixedUpdate()
        {
            Collider2D touching = UnityEngine.Physics2D.OverlapBox(transform.position, size, 0f, playerLayers);
            PlayerCombat player = touching != null ? touching.GetComponentInParent<PlayerCombat>() : null;

            if (player == nearby)
            {
                return;
            }

            if (nearby != null)
            {
                nearby.SetNearbyPickup(null);
            }

            nearby = player;
            if (nearby != null)
            {
                nearby.SetNearbyPickup(this);
            }
        }

        private void ShowWeapon()
        {
            iconRenderer.sprite = weapon.Icon;
        }
    }
}
