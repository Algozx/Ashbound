using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashbound.Gameplay.Player
{
    /// <summary>Healing the player carries into a run. It does nothing until it is unlocked.</summary>
    [RequireComponent(typeof(PlayerCombat))]
    public sealed class PlayerFlask : MonoBehaviour
    {
        [SerializeField] private InputActionReference useAction;
        [SerializeField] private int healAmount = 40;

        private PlayerCombat player;
        private int capacity;

        public int Charges { get; private set; }

        /// <summary>Raised with the charges left.</summary>
        public event Action<int> ChargesChanged;

        /// <summary>Makes the flask bigger, and fills the new charges.</summary>
        public void AddCapacity(int amount)
        {
            capacity += amount;
            SetCharges(Charges + amount);
        }

        public void Refill()
        {
            SetCharges(capacity);
        }

        private void Awake()
        {
            player = GetComponent<PlayerCombat>();
        }

        private void OnEnable()
        {
            useAction.action.Enable();
        }

        private void Update()
        {
            bool hurt = player.CurrentHealth > 0 && player.CurrentHealth < player.MaxHealth;
            if (Charges > 0 && hurt && useAction.action.WasPressedThisFrame())
            {
                player.Heal(healAmount);
                SetCharges(Charges - 1);
            }
        }

        private void SetCharges(int charges)
        {
            Charges = charges;
            ChargesChanged?.Invoke(Charges);
        }
    }
}
