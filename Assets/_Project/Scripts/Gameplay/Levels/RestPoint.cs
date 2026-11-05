using Ashbound.Gameplay.Pickups;
using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Restores health and the flask, once.</summary>
    public sealed class RestPoint : Interactable
    {
        [SerializeField] private GameObject flame;
        [SerializeField] private AudioSource audioSource;

        private bool used;

        public override string Prompt => used ? "Already rested" : "Rest";

        public override void Interact(PlayerCombat player)
        {
            if (used)
            {
                return;
            }

            used = true;
            flame.SetActive(false);
            audioSource.Play();
            player.Heal(player.MaxHealth);
            player.GetComponent<PlayerFlask>().Refill();
            player.SetNearbyInteractable(this);
        }
    }
}
