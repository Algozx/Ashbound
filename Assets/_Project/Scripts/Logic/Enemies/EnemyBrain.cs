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
        private const float ForgetTime = 2f;
        private const float ReturnSpeedScale = 0.8f;
        private const float ReturnTimeLimit = 8f;
        private const float ArriveDistance = 0.5f;
        private const float CruiseHeight = 2.5f;
        private const float HoverWave = 0.25f;
        private const float HoverWaveSpeed = 2f;

        private readonly EnemyBrainSettings settings;
        private SeededRandom random;
        private float timer;
        private float jumpTimer;
        private float giveUpTimer;
        private float forgetTimer;
        private float homeX;
        private float homeY;
        private bool hasHome;
        private bool wantsJump;
        private bool ledgeDecided;
        private bool dropsOffLedge;

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

        /// <summary>Sets the stream that varies patrol walks, so a seed repeats them.</summary>
        public void UseRandom(SeededRandom patrolRandom)
        {
            random = patrolRandom;
        }

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
            forgetTimer = Math.Max(0f, forgetTimer - senses.DeltaTime);

            if (!hasHome)
            {
                hasHome = true;
                SetHome(senses);
                homeY += settings.Flies ? CruiseHeight : 0f;
                timer = settings.PatrolLegTime;
            }

            switch (State)
            {
                case EnemyState.Patrol:
                    TickPatrol(senses);
                    break;
                case EnemyState.Rest:
                    TickRest(senses);
                    break;
                case EnemyState.Return:
                    TickReturn(senses);
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
            if (Notices(senses))
            {
                State = EnemyState.Chase;
                return;
            }

            float fromHome = (senses.X - homeX) * Facing;
            bool pastPatrolArea = settings.PatrolRadius > 0f && fromHome >= settings.PatrolRadius;
            bool legOver = settings.PatrolLegTime > 0f && timer <= 0f;
            if (legOver || pastPatrolArea)
            {
                Rest();
                return;
            }

            if (senses.WallAhead && !senses.RoomEdgeAhead && CanJumpNow(senses))
            {
                RequestJump();
            }
            else if (senses.BlockedAhead && !wantsJump && !DropsOffLedge(senses) && (senses.Grounded || settings.Flies))
            {
                Facing = -Facing;
            }

            MoveX = Facing * settings.PatrolSpeedScale;
            MoveY = settings.Flies ? PatrolHover(senses) : 0f;
        }

        private void TickRest(in EnemySenses senses)
        {
            if (Notices(senses))
            {
                State = EnemyState.Chase;
                return;
            }

            MoveX = 0f;
            MoveY = settings.Flies ? PatrolHover(senses) : 0f;
            if (timer <= 0f)
            {
                StartPatrolLeg(senses);
            }
        }

        private void TickReturn(in EnemySenses senses)
        {
            if (Notices(senses))
            {
                State = EnemyState.Chase;
                return;
            }

            float toHomeX = homeX - senses.X;
            float toHomeY = settings.Flies ? homeY - senses.Y : 0f;
            if (Math.Abs(toHomeX) <= ArriveDistance && Math.Abs(toHomeY) <= ArriveDistance)
            {
                Rest();
                return;
            }

            Facing = toHomeX >= 0f ? 1 : -1;
            if ((senses.WallAhead || senses.LedgeAhead) && !senses.RoomEdgeAhead && CanJumpNow(senses))
            {
                RequestJump();
            }

            bool noWayHome = senses.BlockedAhead && !settings.CanJump && senses.Grounded;
            if (timer <= 0f || noWayHome)
            {
                // Home is out of reach, so this spot becomes the new one.
                SetHome(senses);
                Rest();
                return;
            }

            MoveX = Facing * ReturnSpeedScale;
            MoveY = settings.Flies ? Math.Clamp(toHomeY, -1f, 1f) : 0f;
        }

        private void TickChase(in EnemySenses senses)
        {
            if (!Sees(senses, settings.AggroRange * LoseInterestScale))
            {
                GoHome();
                return;
            }

            Facing = senses.ToTargetX >= 0f ? 1 : -1;
            float heightGap = senses.ToTargetY + (settings.Flies ? settings.HoverHeight : 0f);

            if (Math.Abs(senses.ToTargetX) <= settings.AttackRange && Math.Abs(heightGap) <= settings.AttackHeight)
            {
                giveUpTimer = 0f;
                Enter(EnemyState.Telegraph, settings.TelegraphTime);
                Stop();
                return;
            }

            bool detours = senses.ClimbDetour != 0 && TargetInJumpRange(senses);
            if (detours)
            {
                Facing = senses.ClimbDetour;
            }
            else if (ShouldJump(senses))
            {
                RequestJump();
            }

            bool targetBelow = senses.ToTargetY < -TargetLevelGap;
            bool willLeaveLedge = senses.LedgeAhead && !senses.WallAhead && !senses.RoomEdgeAhead
                && (targetBelow || settings.CanJump);
            bool blocked = senses.BlockedAhead && !willLeaveLedge && !wantsJump;
            bool closeEnough = Math.Abs(senses.ToTargetX) <= settings.AttackRange && !detours;
            MoveX = blocked || closeEnough ? 0f : Facing;
            MoveY = settings.Flies ? Math.Clamp(heightGap, -1f, 1f) : 0f;

            TrackGivingUp(senses);
        }

        // An enemy with no way forward stays a moment, then leaves instead of standing at the limit.
        private void TrackGivingUp(in EnemySenses senses)
        {
            bool stuck = MoveX == 0f && senses.Grounded && !settings.Flies;
            if (senses.TargetOutsideTerritory || stuck)
            {
                giveUpTimer += senses.DeltaTime;
            }
            else if (senses.Grounded || settings.Flies)
            {
                giveUpTimer = 0f;
            }

            if (settings.GiveUpTime > 0f && giveUpTimer >= settings.GiveUpTime)
            {
                forgetTimer = ForgetTime;
                GoHome();
            }
        }

        private bool ShouldJump(in EnemySenses senses)
        {
            if (!CanJumpNow(senses) || senses.RoomEdgeAhead)
            {
                return false;
            }

            bool targetBelow = senses.ToTargetY < -TargetLevelGap;
            bool climbsWall = senses.WallAhead && !targetBelow;
            bool crossesGap = senses.LedgeAhead && !targetBelow;
            return TargetInJumpRange(senses) || climbsWall || crossesGap;
        }

        private bool TargetInJumpRange(in EnemySenses senses)
        {
            return senses.ToTargetY > TargetLevelGap
                && senses.ToTargetY <= settings.JumpHeight
                && Math.Abs(senses.ToTargetX) <= settings.JumpReach;
        }

        private bool CanJumpNow(in EnemySenses senses)
        {
            return settings.CanJump && senses.Grounded && jumpTimer <= 0f;
        }

        private void RequestJump()
        {
            wantsJump = true;
            jumpTimer = settings.JumpCooldown;
        }

        // Decided once per walk, so an enemy does not change its mind at every step of the ledge.
        private bool DropsOffLedge(in EnemySenses senses)
        {
            if (!senses.LedgeAhead || senses.WallAhead || senses.RoomEdgeAhead || !settings.CanJump)
            {
                return false;
            }

            if (!ledgeDecided)
            {
                ledgeDecided = true;
                dropsOffLedge = random != null && random.NextInt(0, 2) == 0;
            }

            return dropsOffLedge;
        }

        private float PatrolHover(in EnemySenses senses)
        {
            float toHome = Math.Clamp(homeY - senses.Y, -1f, 1f) * settings.PatrolSpeedScale;
            return toHome + HoverWave * (float)Math.Sin(timer * HoverWaveSpeed);
        }

        private void StartPatrolLeg(in EnemySenses senses)
        {
            float fromHome = senses.X - homeX;
            if (settings.PatrolRadius > 0f && Math.Abs(fromHome) >= settings.PatrolRadius * 0.5f)
            {
                Facing = fromHome > 0f ? -1 : 1;
            }
            else if (random != null)
            {
                Facing = random.NextInt(0, 2) == 0 ? -1 : 1;
            }

            ledgeDecided = false;
            Enter(EnemyState.Patrol, Varied(settings.PatrolLegTime));
        }

        private void Rest()
        {
            Stop();
            Enter(EnemyState.Rest, Varied(settings.PatrolRestTime));
        }

        private void GoHome()
        {
            giveUpTimer = 0f;
            Enter(EnemyState.Return, ReturnTimeLimit);
        }

        private void SetHome(in EnemySenses senses)
        {
            homeX = senses.X;
            homeY = senses.Y;
        }

        // Between 70% and 130% of the base time.
        private float Varied(float baseTime)
        {
            return random == null ? baseTime : baseTime * (0.7f + 0.6f * (float)random.NextDouble());
        }

        private bool Notices(in EnemySenses senses)
        {
            return forgetTimer <= 0f && !senses.TargetOutsideTerritory && Sees(senses, settings.AggroRange);
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
