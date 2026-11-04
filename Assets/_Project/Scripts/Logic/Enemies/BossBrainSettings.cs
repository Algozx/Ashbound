namespace Ashbound.Logic.Enemies
{
    /// <summary>Behaviour values of the boss. Times are in seconds.</summary>
    public sealed class BossBrainSettings
    {
        public float MeleeRange;
        public float MeleeWindup;
        public float MeleeStrike;
        public float CastWindup;
        public float CastRelease;
        public float Recover;

        /// <summary>Longest time between two casts.</summary>
        public float CastInterval;

        /// <summary>Melee attacks in a row before the boss casts anyway.</summary>
        public int MeleesBeforeCast;

        /// <summary>Share of health at which the second phase starts.</summary>
        public float EnrageHealthShare;

        /// <summary>Multiplier for recovery in the second phase.</summary>
        public float EnragedRecoverScale;

        public int SpellsPerCast;
        public int EnragedSpellsPerCast;
    }
}
