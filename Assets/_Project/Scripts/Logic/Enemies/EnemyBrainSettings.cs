namespace Ashbound.Logic.Enemies
{
    /// <summary>Behaviour values for one enemy type. Times are in seconds.</summary>
    public sealed class EnemyBrainSettings
    {
        public float PatrolSpeedScale;

        /// <summary>How far from home the enemy patrols. Zero means no limit.</summary>
        public float PatrolRadius;

        /// <summary>Average time walking before a rest. Zero means it never rests.</summary>
        public float PatrolLegTime;

        public float PatrolRestTime;

        /// <summary>Time without a way to the target before the enemy goes home. Zero means never.</summary>
        public float GiveUpTime;

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

        /// <summary>Highest target the enemy tries to reach with a jump.</summary>
        public float JumpHeight;

        public float JumpCooldown;

        public bool Flies;

        /// <summary>How far above the target a flying enemy stays.</summary>
        public float HoverHeight;
    }
}
