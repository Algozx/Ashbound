using System;
using System.Collections.Generic;

namespace Ashbound.Logic.Levels
{
    /// <summary>Weighted choice of what a room offers.</summary>
    public static class LootTable
    {
        /// <summary>Returns the index of the chosen entry. A weight of zero is never picked.</summary>
        public static int Pick(SeededRandom random, IReadOnlyList<int> weights)
        {
            int total = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                total += weights[i];
            }

            if (total <= 0)
            {
                throw new ArgumentException("At least one weight must be positive.", nameof(weights));
            }

            int roll = random.NextInt(0, total);
            for (int i = 0; i < weights.Count; i++)
            {
                roll -= weights[i];
                if (roll < 0)
                {
                    return i;
                }
            }

            return weights.Count - 1;
        }
    }
}
