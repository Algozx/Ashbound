namespace Ashbound.Logic.Enemies
{
    /// <summary>What an enemy perceives on one physics step.</summary>
    public struct EnemySenses
    {
        public float X;
        public float Y;
        public float ToTargetX;
        public float ToTargetY;
        public bool BlockedAhead;
        public bool WallAhead;
        public bool LedgeAhead;
        public bool RoomEdgeAhead;
        public bool Grounded;

        /// <summary>True when the target stands beyond the area this enemy guards.</summary>
        public bool TargetOutsideTerritory;

        /// <summary>Side to walk to before jumping, when a platform blocks the way up. Zero if none does.</summary>
        public int ClimbDetour;

        public float DeltaTime;
    }
}
