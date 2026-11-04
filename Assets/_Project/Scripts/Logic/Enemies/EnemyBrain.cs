using System;

namespace Ashbound.Logic.Enemies
{
    /// <summary>
    /// Decides what an enemy does. An attack is always announced by a telegraph, and once it has
    /// started a hit does not interrupt it, so the player cannot lock an enemy in place.
    /// </summary>
    public sealed class EnemyBrain
    {
        private const float LoseInterestScale = 1.5f;
        private const float TargetLevelGap = 1.5f;

        private readonly EnemyBrainSettings settings;
        private float timer;
        private float jumpTimer;
        private bool wantsJump;

        public EnemyBrain(EnemyBrainSettings settings)
        {
            this.settings = settings;
        }

        public EnemyState State { get; private set; } = EnemyState.Patrol;

        public int Facing { get; private set; } = 1;

        /// <summary>Desired movement from -1 to 1, as a share of the enemy's speed.</summary>
        public float MoveX { get; private set; }

        /// <summary>Desired vertical movement of a flying enemy, as a share of its speed.</summary>
        public float MoveY { get; private set; }

        /// <summary>Reports a jump request once, so the body starts one jump per decision.</summary>
        public bool ConsumeJump()
        {
            bool jump = wantsJump;
            wantsJump = false;
            return jump;
        }

        public void Tick(in EnemySenses senses)
        {
            timer = Math.Max(0f, timer - senses.DeltaTime);
            jumpTimer = Math.Max(0f, jumpTimer - senses.DeltaTime);

            switch (State)
            {
                case EnemyState.Patrol:
                    TickPatrol(senses);
                    break;
                case EnemyState.Chase:
                    TickChase(senses);
                    break;
                case EnemyState.Telegraph:
                    Stop();
                    if (timer <= 0f)
                    {
                        Enter(EnemyState.Strike, settings.StrikeTime);
                    }

                    break;
                case EnemyState.Strike:
                    MoveX = senses.BlockedAhead ? 0f : Facing * settings.StrikeMoveScale;
                    MoveY = 0f;
                    if (timer <= 0f)
                    {
                        Enter(EnemyState.Recover, settings.RecoverTime);
                    }

                    break;
                case EnemyState.Recover:
                case EnemyState.Hurt:
                    Stop();
                    if (timer <= 0f)
                    {
                        State = EnemyState.Chase;
                    }

                    break;
                default:
                    Stop();
                    break;
            }
        }

        /// <summary>Reacts to a hit that did not kill.</summary>
        public void OnHit()
        {
            if (State == EnemyState.Dead || State == EnemyState.Telegraph || State == EnemyState.Strike)
            {
                return;
            }

            Enter(EnemyState.Hurt, settings.HurtTime);
        }

        public void OnKilled()
        {
            State = EnemyState.Dead;
            Stop();
        }

        private void TickPatrol(in EnemySenses senses)
        {
            if (Sees(senses, settings.AggroRange))
            {
                State = EnemyState.Chase;
                return;
            }

            if (senses.BlockedAhead)
            {
                Facing = -Facing;
            }

            MoveX = Facing * settings.PatrolSpeedScale;
            MoveY = 0f;
        }

        private void TickChase(in EnemySenses senses)
        {
            if (!Sees(senses, settings.AggroRange * LoseInterestScale))
            {
                State = EnemyState.Patrol;
                return;
            }

            Facing = senses.ToTargetX >= 0f ? 1 : -1;
            float heightGap = senses.ToTargetY + (settings.Flies ? settings.HoverHeight : 0f);

            if (Math.Abs(senses.ToTargetX) <= settings.AttackRange && Math.Abs(heightGap) <= settings.AttackHeight)
            {
                Enter(EnemyState.Telegraph, settings.TelegraphTime);
                Stop();
                return;
            }

            if (ShouldJump(senses))
            {
                wantsJump = true;
                jumpTimer = settings.JumpCooldown;
            }

            bool targetBelow = senses.ToTargetY < -TargetLevelGap;
            bool willLeaveLedge = senses.LedgeAhead && !senses.WallAhead && !senses.RoomEdgeAhead
                && (targetBelow || settings.CanJump);
            bool blocked = senses.BlockedAhead && !willLeaveLedge && !wantsJump;
            bool closeEnough = Math.Abs(senses.ToTargetX) <= settings.AttackRange;
            MoveX = blocked || closeEnough ? 0f : Facing;
            MoveY = settings.Flies ? Math.Clamp(heightGap, -1f, 1f) : 0f;
        }

        private bool ShouldJump(in EnemySenses senses)
        {
            if (!settings.CanJump || !senses.Grounded || jumpTimer > 0f || senses.RoomEdgeAhead)
            {
                return false;
            }

            bool targetAbove = senses.ToTargetY > TargetLevelGap;
            bool targetBelow = senses.ToTargetY < -TargetLevelGap;
            bool reachesTarget = targetAbove && Math.Abs(senses.ToTargetX) <= settings.JumpReach;
            bool climbsWall = senses.WallAhead && !targetBelow;
            bool crossesGap = senses.LedgeAhead && !targetBelow;
            return reachesTarget || climbsWall || crossesGap;
        }

        private bool Sees(in EnemySenses senses, float range)
        {
            return Math.Abs(senses.ToTargetX) <= range && Math.Abs(senses.ToTargetY) <= settings.AggroHeight;
        }

        private void Stop()
        {
            MoveX = 0f;
            MoveY = 0f;
        }

        private void Enter(EnemyState state, float duration)
        {
            State = state;
            timer = duration;
        }
    }
}
