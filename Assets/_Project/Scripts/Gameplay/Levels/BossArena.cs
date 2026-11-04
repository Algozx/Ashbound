using System;
using Ashbound.Gameplay.Enemies;
using UnityEngine;

namespace Ashbound.Gameplay.Levels
{
    /// <summary>Closes the door when the player walks in, and opens the exit when the boss dies.</summary>
    public sealed class BossArena : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private Transform startLine;
        [SerializeField] private GameObject door;
        [SerializeField] private GameObject exit;

        private Transform player;
        private bool started;

        public BossController Boss => boss;

        /// <summary>Raised when the fight starts.</summary>
        public event Action Started;

        /// <summary>Sets who can start the fight.</summary>
        public void Prepare(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void OnEnable()
        {
            boss.Defeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            boss.Defeated -= OnBossDefeated;
        }

        private void FixedUpdate()
        {
            if (!started && player != null && player.position.x >= startLine.position.x)
            {
                started = true;
                door.SetActive(true);
                boss.Wake(player);
                Started?.Invoke();
            }
        }

        private void OnBossDefeated()
        {
            door.SetActive(false);
            exit.SetActive(true);
        }
    }
}
