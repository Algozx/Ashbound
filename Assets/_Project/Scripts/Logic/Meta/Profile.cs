using System;
using System.Collections.Generic;

namespace Ashbound.Logic.Meta
{
    /// <summary>What survives between runs: banked Embers and the unlocks bought.</summary>
    public sealed class Profile
    {
        private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

        public int Embers { get; private set; }

        /// <summary>Rebuilds a profile from saved data. Missing data gives an empty profile.</summary>
        public static Profile FromData(ProfileData data)
        {
            var profile = new Profile();
            if (data == null)
            {
                return profile;
            }

            profile.Embers = Math.Max(0, data.embers);
            foreach (UnlockLevel unlock in data.unlocks ?? new UnlockLevel[0])
            {
                profile.levels[unlock.id] = Math.Max(0, unlock.level);
            }

            return profile;
        }

        public ProfileData ToData()
        {
            var unlocks = new List<UnlockLevel>();
            foreach (KeyValuePair<string, int> pair in levels)
            {
                unlocks.Add(new UnlockLevel { id = pair.Key, level = pair.Value });
            }

            return new ProfileData { embers = Embers, unlocks = unlocks.ToArray() };
        }

        public int Level(string id)
        {
            return levels.TryGetValue(id, out int level) ? level : 0;
        }

        public void Bank(int amount)
        {
            Embers += amount;
        }

        public void Withdraw(int amount)
        {
            Embers -= amount;
        }

        public void Raise(string id)
        {
            levels[id] = Level(id) + 1;
        }
    }
}
