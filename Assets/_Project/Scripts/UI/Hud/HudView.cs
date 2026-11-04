using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Levels;
using Ashbound.Gameplay.Meta;
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
        [SerializeField] private Progression progression;
        [SerializeField] private PlayerFlask flask;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthLabel;
        [SerializeField] private Text weaponLabel;
        [SerializeField] private Text promptLabel;
        [SerializeField] private Text seedLabel;
        [SerializeField] private Text embersLabel;
        [SerializeField] private Text flaskLabel;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Text messageLabel;

        private void OnEnable()
        {
            player.HealthChanged += ShowHealth;
            player.WeaponChanged += ShowWeapon;
            player.NearbyInteractableChanged += ShowPrompt;
            progression.EmbersChanged += ShowEmbers;
            flask.ChargesChanged += ShowFlask;
            run.RunEnded += ShowMessage;
        }

        private void OnDisable()
        {
            player.HealthChanged -= ShowHealth;
            player.WeaponChanged -= ShowWeapon;
            player.NearbyInteractableChanged -= ShowPrompt;
            progression.EmbersChanged -= ShowEmbers;
            flask.ChargesChanged -= ShowFlask;
            run.RunEnded -= ShowMessage;
        }

        private void Start()
        {
            messagePanel.SetActive(false);
            seedLabel.text = "Seed " + run.Seed;
            ShowHealth(player.CurrentHealth, player.MaxHealth);
            ShowWeapon(player.Weapon);
            ShowPrompt(null);
            ShowEmbers();
            ShowFlask(flask.Charges);
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

        private void ShowEmbers()
        {
            embersLabel.text = "Embers " + progression.Wallet.Carried + "   Saved " + progression.Wallet.Banked;
        }

        private void ShowFlask(int charges)
        {
            flaskLabel.text = charges > 0 ? "Q  Flask x" + charges : string.Empty;
        }

        private void ShowPrompt(Interactable interactable)
        {
            promptLabel.text = interactable != null ? "F  " + interactable.Prompt : string.Empty;
        }

        private void ShowMessage(string message)
        {
            messageLabel.text = message;
            messagePanel.SetActive(true);
        }
    }
}
