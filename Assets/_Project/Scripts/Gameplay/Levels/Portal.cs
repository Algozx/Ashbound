using System;
using Ashbound.Gameplay.Pickups;
using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Takes the player to the next part of the run.</summary>
    public sealed class Portal : Interactable
    {
        private Vector2 destination;

        public override string Prompt => "Enter the portal";

        /// <summary>Raised after a jump, with the distance the player moved.</summary>
        public event Action<Vector2> Used;

        public void SetDestination(Vector2 position)
        {
            destination = position;
        }

        public override void Interact(PlayerCombat player)
        {
            Vector2 moved = destination - (Vector2)player.transform.position;
            player.GetComponent<PlayerController>().Teleport(destination);
            Used?.Invoke(moved);
        }
    }
}
