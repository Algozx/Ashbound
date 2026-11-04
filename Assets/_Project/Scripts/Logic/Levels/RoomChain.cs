using System;
using System.Collections.Generic;

namespace Ashbound.Logic.Levels
{
    /// <summary>Chooses the rooms of a run from a seed.</summary>
    public static class RoomChain
    {
        /// <summary>
        /// Picks rooms from a pool of the given size, in order. Rooms only repeat when the run is
        /// longer than the pool, and never back to back.
        /// </summary>
        public static int[] Build(SeededRandom random, int poolSize, int length)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize));
            }

            var chain = new int[length];
            var bag = new List<int>();
            int previous = -1;

            for (int i = 0; i < length; i++)
            {
                if (bag.Count == 0)
                {
                    Refill(bag, poolSize);
                }

                int pick = random.NextInt(0, bag.Count);
                if (bag[pick] == previous && bag.Count > 1)
                {
                    pick = (pick + 1) % bag.Count;
                }

                previous = bag[pick];
                chain[i] = previous;
                bag.RemoveAt(pick);
            }

            return chain;
        }

        /// <summary>Decides which spawn points of a room are used.</summary>
        public static bool[] PickSpawns(SeededRandom random, int spawnCount, double chance)
        {
            var used = new bool[spawnCount];
            bool any = false;
            for (int i = 0; i < spawnCount; i++)
            {
                used[i] = random.NextDouble() < chance;
                any |= used[i];
            }

            if (!any && spawnCount > 0)
            {
                used[random.NextInt(0, spawnCount)] = true;
            }

            return used;
        }

        private static void Refill(List<int> bag, int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                bag.Add(i);
            }
        }
    }
}
