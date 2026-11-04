namespace Ashbound.Logic.Enemies
{
    /// <summary>Behaviour values for one enemy type. Times are in seconds.</summary>
    public sealed class EnemyBrainSettings
    {
        public float PatrolSpeedScale;
        public float AggroRange;
        public float AggroHeight;
        public float AttackRange;
        public float TelegraphTime;
        public float StrikeTime;
        public float RecoverTime;
        public float HurtTime;

        /// <summary>Largest height difference at which the attack starts.</summary>
        public float AttackHeight;

        /// <summary>Forward movement during the strike, as a share of the enemy's speed.</summary>
        public float StrikeMoveScale;

        public bool CanJump;

        /// <summary>Farthest horizontal distance at which the enemy jumps toward a higher target.</summary>
        public float JumpReach;

        public float JumpCooldown;

        public bool Flies;

        /// <summary>How far above the target a flying enemy stays.</summary>
        public float HoverHeight;
    }
}
