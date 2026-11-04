using System;

namespace Ashbound.Logic.Meta
{
    /// <summary>The saved form of a profile. Raise the version when the layout changes.</summary>
    [Serializable]
    public sealed class ProfileData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int embers;
        public UnlockLevel[] unlocks = new UnlockLevel[0];
    }

    /// <summary>How many levels of one unlock were bought.</summary>
    [Serializable]
    public sealed class UnlockLevel
    {
        public string id;
        public int level;
    }
}
