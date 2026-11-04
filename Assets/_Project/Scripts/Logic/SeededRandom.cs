namespace Ashbound.Logic
{
    /// <summary>
    /// Deterministic random stream (SplitMix64). The same seed always gives the same run,
    /// on every platform, which System.Random and UnityEngine.Random do not guarantee.
    /// </summary>
    public sealed class SeededRandom
    {
        private ulong state;

        public SeededRandom(ulong seed)
        {
            state = seed;
        }

        /// <summary>Next raw 64-bit value.</summary>
        public ulong NextUInt64()
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>Integer in [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new System.ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            ulong range = (ulong)((long)maxExclusive - minInclusive);
            return (int)(minInclusive + (long)(NextUInt64() % range));
        }

        /// <summary>Double in [0, 1).</summary>
        public double NextDouble()
        {
            return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>
        /// Independent stream for one subsystem, so adding a roll to loot does not change
        /// the level layout of the same seed.
        /// </summary>
        public SeededRandom Fork(ulong salt)
        {
            return new SeededRandom(NextUInt64() ^ salt);
        }
    }
}
