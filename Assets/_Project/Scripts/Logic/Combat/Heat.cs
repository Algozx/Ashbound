using System;

namespace Ashbound.Logic.Combat
{
    /// <summary>
    /// The signature mechanic. Landing hits builds levels that raise damage and Embers. Taking damage
    /// puts it out, and time without a hit lowers it one level at a time.
    /// </summary>
    public sealed class Heat
    {
        private readonly int hitsPerLevel;
        private readonly float decayTime;
        private readonly float[] damageBonus;
        private readonly float[] emberBonus;
        private int hits;
        private float idleTime;

        /// <param name="damageBonus">Extra damage at each level above zero, as a share.</param>
        /// <param name="emberBonus">Extra Embers at each level above zero, as a share.</param>
        public Heat(int hitsPerLevel, float decayTime, float[] damageBonus, float[] emberBonus)
        {
            this.hitsPerLevel = hitsPerLevel;
            this.decayTime = decayTime;
            this.damageBonus = damageBonus;
            this.emberBonus = emberBonus;
        }

        public int Level { get; private set; }

        public int MaxLevel => damageBonus.Length;

        public float DamageScale => 1f + (Level > 0 ? damageBonus[Level - 1] : 0f);

        public float EmberScale => 1f + (Level > 0 ? emberBonus[Level - 1] : 0f);

        /// <summary>Raised with the new level whenever it changes.</summary>
        public event Action<int> LevelChanged;

        public void RegisterHit()
        {
            idleTime = 0f;
            if (Level >= MaxLevel)
            {
                return;
            }

            hits++;
            if (hits >= hitsPerLevel)
            {
                hits = 0;
                SetLevel(Level + 1);
            }
        }

        public void Extinguish()
        {
            hits = 0;
            idleTime = 0f;
            SetLevel(0);
        }

        public void Tick(float deltaTime)
        {
            if (Level == 0 && hits == 0)
            {
                return;
            }

            idleTime += deltaTime;
            if (idleTime >= decayTime)
            {
                idleTime = 0f;
                hits = 0;
                SetLevel(Math.Max(0, Level - 1));
            }
        }

        private void SetLevel(int level)
        {
            if (level != Level)
            {
                Level = level;
                LevelChanged?.Invoke(level);
            }
        }
    }
}
