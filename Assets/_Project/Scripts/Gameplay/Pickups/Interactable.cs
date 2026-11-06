using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>Something the player can use with the interact button while inside its trigger.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        private PlayerCombat nearby;

        /// <summary>What the interact button does here.</summary>
        public abstract string Prompt { get; }

        public abstract void Interact(PlayerCombat player);

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerCombat player = other.GetComponentInParent<PlayerCombat>();
            if (player != null)
            {
                nearby = player;
                nearby.SetNearbyInteractable(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerCombat>() != null)
            {
                Forget();
            }
        }

        // A map switching off sends no exit message.
        private void OnDisable()
        {
            Forget();
        }

        private void Forget()
        {
            if (nearby != null)
            {
                nearby.ClearNearbyInteractable(this);
                nearby = null;
            }
        }
    }
}
