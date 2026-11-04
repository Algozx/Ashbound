using System;

namespace Ashbound.Logic.Combat
{
    /// <summary>Hit points of the player or an enemy.</summary>
    public sealed class Health
    {
        public Health(int max)
        {
            Max = max;
            Current = max;
        }

        public int Max { get; }

        public int Current { get; private set; }

        public bool IsDead => Current <= 0;

        /// <summary>Restores hit points, up to the maximum. A dead body stays dead.</summary>
        public void Heal(int amount)
        {
            if (!IsDead)
            {
                Current = Math.Min(Max, Current + amount);
            }
        }

        /// <summary>Removes hit points and reports whether this hit was the killing one.</summary>
        public bool Damage(int amount)
        {
            if (IsDead)
            {
                return false;
            }

            Current = Math.Max(0, Current - amount);
            return IsDead;
        }
    }
}
