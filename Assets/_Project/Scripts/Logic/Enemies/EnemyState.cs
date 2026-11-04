namespace Ashbound.Logic.Enemies
{
    /// <summary>What an enemy is doing, also used to pick the animation.</summary>
    public enum EnemyState
    {
        Patrol,
        Chase,
        Telegraph,
        Strike,
        Recover,
        Hurt,
        Dead,
    }
}
