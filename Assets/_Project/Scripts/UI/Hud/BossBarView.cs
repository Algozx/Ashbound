using Ashbound.Gameplay.Enemies;
using Ashbound.Gameplay.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashbound.UI.Hud
{
    /// <summary>Shows the boss's name and health during the fight.</summary>
    public sealed class BossBarView : MonoBehaviour
    {
        [SerializeField] private RunDirector run;
        [SerializeField] private GameObject panel;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text nameLabel;

        private BossArena arena;
        private BossController boss;

        private void Start()
        {
            panel.SetActive(false);
            arena = run.Arena;
            boss = arena.Boss;
            nameLabel.text = boss.DisplayName;
            arena.Started += OnFightStarted;
            boss.HealthChanged += ShowHealth;
            boss.Defeated += OnDefeated;
        }

        private void OnDestroy()
        {
            if (arena != null)
            {
                arena.Started -= OnFightStarted;
            }

            if (boss != null)
            {
                boss.HealthChanged -= ShowHealth;
                boss.Defeated -= OnDefeated;
            }
        }

        private void OnFightStarted()
        {
            ShowHealth(boss.CurrentHealth, boss.MaxHealth);
            panel.SetActive(true);
        }

        private void ShowHealth(int current, int max)
        {
            healthFill.fillAmount = (float)current / max;
        }

        private void OnDefeated()
        {
            panel.SetActive(false);
        }
    }
}
