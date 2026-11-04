using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>Something the player can use with the interact button while standing next to it.</summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(1.6f, 2f);
        [SerializeField] private LayerMask playerLayers;

        private PlayerCombat nearby;

        /// <summary>What the interact button does here.</summary>
        public abstract string Prompt { get; }

        public abstract void Interact(PlayerCombat player);

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
                nearby.SetNearbyInteractable(null);
            }

            nearby = player;
            if (nearby != null)
            {
                nearby.SetNearbyInteractable(this);
            }
        }
    }
}
