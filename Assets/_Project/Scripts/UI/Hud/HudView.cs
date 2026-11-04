using Ashbound.Gameplay.Levels;
using Ashbound.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows health, the run seed and the end-of-run message.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat player;
        [SerializeField] private RunDirector run;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text healthLabel;
        [SerializeField] private Text seedLabel;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private Text messageLabel;

        private void OnEnable()
        {
            player.HealthChanged += ShowHealth;
            run.RunEnded += ShowMessage;
        }

        private void OnDisable()
        {
            player.HealthChanged -= ShowHealth;
            run.RunEnded -= ShowMessage;
        }

        private void Start()
        {
            messagePanel.SetActive(false);
            seedLabel.text = "Seed " + run.Seed;
            ShowHealth(player.CurrentHealth, player.MaxHealth);
        }

        private void ShowHealth(int current, int max)
        {
            healthFill.fillAmount = (float)current / max;
            healthLabel.text = current + " / " + max;
        }

        private void ShowMessage(string message)
        {
            messageLabel.text = message;
            messagePanel.SetActive(true);
        }
    }
}
