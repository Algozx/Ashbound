using System;
using System.Collections.Generic;

namespace Ashbound.Logic.Meta
{
    /// <summary>
    /// Embers carried in a run. They are at risk until the run ends, so purchases use them before
    /// touching the banked ones.
    /// </summary>
    public sealed class EmberWallet
    {
        private readonly Profile profile;

        public EmberWallet(Profile profile)
        {
            this.profile = profile;
        }

        public int Carried { get; private set; }

        public int Banked => profile.Embers;

        public int Total => Carried + Banked;

        public void Collect(int amount)
        {
            Carried += amount;
        }

        /// <summary>Price of the next level, or -1 when there are no more levels.</summary>
        public int NextCost(string id, IReadOnlyList<int> costs)
        {
            int level = profile.Level(id);
            return level < costs.Count ? costs[level] : -1;
        }

        /// <summary>Buys the next level of an unlock and reports whether it could be paid.</summary>
        public bool TryBuy(string id, IReadOnlyList<int> costs)
        {
            int cost = NextCost(id, costs);
            if (cost < 0 || cost > Total)
            {
                return false;
            }

            int fromCarried = Math.Min(cost, Carried);
            Carried -= fromCarried;
            profile.Withdraw(cost - fromCarried);
            profile.Raise(id);
            return true;
        }

        /// <summary>Banks half of what is carried and returns that amount.</summary>
        public int SettleDeath()
        {
            return Settle(Carried / 2);
        }

        /// <summary>Banks everything carried and returns that amount.</summary>
        public int SettleVictory()
        {
            return Settle(Carried);
        }

        private int Settle(int kept)
        {
            profile.Bank(kept);
            Carried = 0;
            return kept;
        }
    }
}
