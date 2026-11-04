using UnityEngine;

namespace Ashbound.Gameplay.Combat
{
    /// <summary>Receives the animation events that time a <see cref="Hitbox"/>.</summary>
    public sealed class AttackAnimationEvents : MonoBehaviour
    {
        [SerializeField] private Hitbox hitbox;

        public void OpenHitbox()
        {
            hitbox.Open();
        }

        public void CloseHitbox()
        {
            hitbox.Close();
        }
    }
}
