using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>A weapon on the ground. Taking it leaves the old weapon in its place.</summary>
    public sealed class WeaponPickup : Interactable
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private SpriteRenderer iconRenderer;

        public WeaponDefinition Weapon => weapon;

        public override string Prompt => "Take " + weapon.DisplayName;

        /// <summary>Swaps this pickup's weapon with the player's.</summary>
        public override void Interact(PlayerCombat player)
        {
            WeaponDefinition previous = player.Weapon;
            player.Equip(weapon);
            weapon = previous;
            ShowWeapon();
            player.SetNearbyInteractable(this);
        }

        private void Awake()
        {
            ShowWeapon();
        }

        private void ShowWeapon()
        {
            iconRenderer.sprite = weapon.Icon;
        }
    }
}
