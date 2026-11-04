namespace Ashbound.Logic.Enemies
{
    /// <summary>What an enemy perceives on one physics step.</summary>
    public struct EnemySenses
    {
        public float ToTargetX;
        public float ToTargetY;
        public bool BlockedAhead;
        public float DeltaTime;
    }
}
