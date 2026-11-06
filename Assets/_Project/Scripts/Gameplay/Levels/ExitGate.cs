using System;
using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Ends the run when the player reaches it.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ExitGate : MonoBehaviour
    {
        private bool reached;

        public event Action Reached;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!reached && other.GetComponentInParent<PlayerCombat>() != null)
            {
                reached = true;
                Reached?.Invoke();
            }
        }
    }
}
