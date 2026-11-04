namespace Ashbound.Logic.Enemies
{
    /// <summary>What the boss is doing, also used to pick the animation.</summary>
    public enum BossState
    {
        Waiting,
        Approach,
        MeleeWindup,
        MeleeStrike,
        CastWindup,
        CastRelease,
        Recover,
        Dead,
    }
}
