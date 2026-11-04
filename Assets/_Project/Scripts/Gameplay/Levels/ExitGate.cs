using System;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Ends the run when the player reaches it.</summary>
    public sealed class ExitGate : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(2f, 4f);
        [SerializeField] private LayerMask playerLayers;

        private bool reached;

        public event Action Reached;

        private void FixedUpdate()
        {
            if (!reached && UnityEngine.Physics2D.OverlapBox(transform.position, size, 0f, playerLayers) != null)
            {
                reached = true;
                Reached?.Invoke();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
