using System;
using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Receives the animation events that time an attack.</summary>
    public sealed class AttackAnimationEvents : MonoBehaviour
    {
        [SerializeField] private Hitbox hitbox;

        /// <summary>Raised on the frame a ranged attack releases its shot.</summary>
        public event Action ProjectileRequested;

        public void OpenHitbox()
        {
            hitbox.Open();
        }

        public void CloseHitbox()
        {
            hitbox.Close();
        }

        public void FireProjectile()
        {
            ProjectileRequested?.Invoke();
        }
    }
}
