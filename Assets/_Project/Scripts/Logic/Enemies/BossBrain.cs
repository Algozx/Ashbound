using System;

namespace Ashbound.Logic.Enemies
{
    /// <summary>
    /// Decides what the boss does. It walks to the player and swings, and it casts on a timer, so
    /// keeping away is never safe. Below a share of its health it recovers faster and casts more
    /// spells. Hits never interrupt it.
    /// </summary>
    public sealed class BossBrain
    {
        private readonly BossBrainSettings settings;
        private float timer;
        private float castTimer;
        private int meleesInARow;

        public BossBrain(BossBrainSettings settings)
        {
            this.settings = settings;
            castTimer = settings.CastInterval;
        }

        public BossState State { get; private set; } = BossState.Waiting;

        public int Facing { get; private set; } = -1;

        /// <summary>Desired movement from -1 to 1, as a share of the boss's speed.</summary>
        public float MoveX { get; private set; }

        public bool IsEnraged { get; private set; }

        /// <summary>Spells the current cast releases.</summary>
        public int SpellCount => IsEnraged ? settings.EnragedSpellsPerCast : settings.SpellsPerCast;

        /// <summary>Starts the fight.</summary>
        public void Wake()
        {
            if (State == BossState.Waiting)
            {
                State = BossState.Approach;
            }
        }

        /// <summary>Updates the phase from the current share of health.</summary>
        public void OnHealthChanged(float healthShare)
        {
            if (healthShare <= settings.EnrageHealthShare)
            {
                IsEnraged = true;
            }
        }

        public void OnKilled()
        {
            State = BossState.Dead;
            MoveX = 0f;
        }

        public void Tick(float toTargetX, float deltaTime)
        {
            timer = Math.Max(0f, timer - deltaTime);
            castTimer = Math.Max(0f, castTimer - deltaTime);
            MoveX = 0f;

            switch (State)
            {
                case BossState.Approach:
                    TickApproach(toTargetX);
                    break;
                case BossState.MeleeWindup:
                    AdvanceTo(BossState.MeleeStrike, settings.MeleeStrike);
                    break;
                case BossState.MeleeStrike:
                case BossState.CastRelease:
                    AdvanceTo(BossState.Recover, RecoverTime());
                    break;
                case BossState.CastWindup:
                    AdvanceTo(BossState.CastRelease, settings.CastRelease);
                    break;
                case BossState.Recover:
                    AdvanceTo(BossState.Approach, 0f);
                    break;
            }
        }

        private void TickApproach(float toTargetX)
        {
            Facing = toTargetX >= 0f ? 1 : -1;

            if (castTimer <= 0f || meleesInARow >= settings.MeleesBeforeCast)
            {
                meleesInARow = 0;
                castTimer = settings.CastInterval;
                Enter(BossState.CastWindup, settings.CastWindup);
                return;
            }

            if (Math.Abs(toTargetX) <= settings.MeleeRange)
            {
                meleesInARow++;
                Enter(BossState.MeleeWindup, settings.MeleeWindup);
                return;
            }

            MoveX = Facing;
        }

        private float RecoverTime()
        {
            return IsEnraged ? settings.Recover * settings.EnragedRecoverScale : settings.Recover;
        }

        private void AdvanceTo(BossState next, float duration)
        {
            if (timer <= 0f)
            {
                Enter(next, duration);
            }
        }

        private void Enter(BossState state, float duration)
        {
            State = state;
            timer = duration;
        }
    }
}
