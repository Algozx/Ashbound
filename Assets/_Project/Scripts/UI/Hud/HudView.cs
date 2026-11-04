using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Levels;
using Ashbound.Gameplay.Pickups;
using Ashbound.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows health, the weapon, pickup prompts, the run seed and the end message.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat player;
        [SerializeField] private RunDirector run;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthLabel;
        [SerializeField] private Text weaponLabel;
        [SerializeField] private Text promptLabel;
        [SerializeField] private Text seedLabel;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Text messageLabel;

        private void OnEnable()
        {
            player.HealthChanged += ShowHealth;
            player.WeaponChanged += ShowWeapon;
            player.NearbyPickupChanged += ShowPrompt;
            run.RunEnded += ShowMessage;
        }

        private void OnDisable()
        {
            player.HealthChanged -= ShowHealth;
            player.WeaponChanged -= ShowWeapon;
            player.NearbyPickupChanged -= ShowPrompt;
            run.RunEnded -= ShowMessage;
        }

        private void Start()
        {
            messagePanel.SetActive(false);
            seedLabel.text = "Seed " + run.Seed;
            ShowHealth(player.CurrentHealth, player.MaxHealth);
            ShowWeapon(player.Weapon);
            ShowPrompt(null);
        }

        private void ShowHealth(int current, int max)
        {
            healthFill.fillAmount = (float)current / max;
            healthLabel.text = current + " / " + max;
        }

        private void ShowWeapon(WeaponDefinition weapon)
        {
            weaponLabel.text = weapon.DisplayName;
        }

        private void ShowPrompt(WeaponPickup pickup)
        {
            promptLabel.text = pickup != null ? "F  Take " + pickup.Weapon.DisplayName : string.Empty;
        }

        private void ShowMessage(string message)
        {
            messageLabel.text = message;
            messagePanel.SetActive(true);
        }
    }
}
