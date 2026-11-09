using System;
using Ashbound.Logic.Combat;
using UnityEngine;

namespace Ashbound.Gameplay.Player
{
    /// <summary>Holds the player's Heat and shows it on the character.</summary>
    public sealed class PlayerHeat : MonoBehaviour
    {
        [SerializeField] private int hitsPerLevel = 6;
        [Tooltip("Seconds without a hit before one level is lost.")]
        [SerializeField] private float decayTime = 6f;
        [Tooltip("Extra damage at each level, as a share.")]
        [SerializeField] private float[] damageBonus = { 0.05f, 0.1f, 0.2f };
        [Tooltip("Extra Embers at each level, as a share.")]
        [SerializeField] private float[] emberBonus = { 0.25f, 0.5f, 0.75f };
        [SerializeField] private Color hottestTint = new Color(1f, 0.65f, 0.35f);
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField, Range(0f, 1f)] private float levelUpVolume = 0.4f;

        private PlayerController controller;
        private Heat heat;
        private int shownLevel;

        public int Level => heat.Level;

        public int MaxLevel => heat.MaxLevel;

        public float DamageScale => heat.DamageScale;

        public float EmberScale => heat.EmberScale;

        /// <summary>Raised with the new level whenever it changes.</summary>
        public event Action<int> LevelChanged;

        public void RegisterHit()
        {
            heat.RegisterHit();
        }

        public void Extinguish()
        {
            heat.Extinguish();
        }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            heat = new Heat(hitsPerLevel, decayTime, damageBonus, emberBonus);
            heat.LevelChanged += OnLevelChanged;
        }

        private void Update()
        {
            heat.Tick(Time.deltaTime);
        }

        private void OnLevelChanged(int level)
        {
            controller.Tint = Color.Lerp(Color.white, hottestTint, (float)level / heat.MaxLevel);
            if (level > shownLevel)
            {
                audioSource.PlayOneShot(levelUpSound, levelUpVolume);
            }

            shownLevel = level;
            LevelChanged?.Invoke(level);
        }
    }
}
