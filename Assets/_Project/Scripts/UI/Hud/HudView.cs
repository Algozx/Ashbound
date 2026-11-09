using Ashbound.Gameplay.Combat;
using Ashbound.Gameplay.Levels;
using Ashbound.Gameplay.Meta;
using Ashbound.Gameplay.Pickups;
using Ashbound.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows health, the weapon, Embers, the flask, pickup prompts and the run seed.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat player;
        [SerializeField] private RunDirector run;
        [SerializeField] private Progression progression;
        [SerializeField] private PlayerFlask flask;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private TMP_Text weaponLabel;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private TMP_Text seedLabel;
        [SerializeField] private TMP_Text embersLabel;
        [SerializeField] private TMP_Text flaskLabel;
        [SerializeField] private GameObject flaskRow;
        [SerializeField] private GameObject promptPanel;

        private void OnEnable()
        {
            player.HealthChanged += ShowHealth;
            player.WeaponChanged += ShowWeapon;
            player.NearbyInteractableChanged += ShowPrompt;
            progression.EmbersChanged += ShowEmbers;
            flask.ChargesChanged += ShowFlask;
        }

        private void OnDisable()
        {
            player.HealthChanged -= ShowHealth;
            player.WeaponChanged -= ShowWeapon;
            player.NearbyInteractableChanged -= ShowPrompt;
            progression.EmbersChanged -= ShowEmbers;
            flask.ChargesChanged -= ShowFlask;
        }

        private void Start()
        {
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
            embersLabel.text = progression.Wallet.Carried + "   saved " + progression.Wallet.Banked;
        }

        private void ShowFlask(int charges)
        {
            flaskRow.SetActive(charges > 0);
            flaskLabel.text = "x" + charges + "  Q";
        }

        private void ShowPrompt(Interactable interactable)
        {
            promptPanel.SetActive(interactable != null);
            if (interactable != null)
            {
                promptLabel.text = "F  " + interactable.Prompt;
            }
        }
    }
}
