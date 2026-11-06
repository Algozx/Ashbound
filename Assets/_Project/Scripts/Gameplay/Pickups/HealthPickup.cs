using Ashbound.Gameplay.Player;
using UnityEngine;

namespace Ashbound.Gameplay.Pickups
{
    /// <summary>Restores health on touch. It stays in place while the player is at full health.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int amount = 35;

        private PlayerCombat visitor;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerCombat player = other.GetComponentInParent<PlayerCombat>();
            if (player != null)
            {
                visitor = player;
                visitor.HealthChanged += OnVisitorHealthChanged;
                TryHeal();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerCombat>() != null)
            {
                Forget();
            }
        }

        private void OnDisable()
        {
            Forget();
        }

        private void OnVisitorHealthChanged(int current, int max)
        {
            TryHeal();
        }

        private void TryHeal()
        {
            if (visitor != null && visitor.CurrentHealth < visitor.MaxHealth)
            {
                PlayerCombat player = visitor;
                Forget();
                player.Heal(amount);
                gameObject.SetActive(false);
            }
        }

        private void Forget()
        {
            if (visitor != null)
            {
                visitor.HealthChanged -= OnVisitorHealthChanged;
                visitor = null;
            }
        }
    }
}
