using System;

namespace Ashbound.Logic.Enemies
{
    /// <summary>
    /// Decides what a melee enemy does. An attack is always announced by a telegraph, and once it
    /// has started a hit does not interrupt it, so the player cannot lock an enemy in place.
    /// </summary>
    public sealed class EnemyBrain
    {
        private const float LoseInterestScale = 1.5f;

        private readonly EnemyBrainSettings settings;
        private float timer;

        public EnemyBrain(EnemyBrainSettings settings)
        {
            this.settings = settings;
        }

        public EnemyState State { get; private set; } = EnemyState.Patrol;

        public int Facing { get; private set; } = 1;

        /// <summary>Desired movement from -1 to 1, as a share of the enemy's speed.</summary>
        public float MoveX { get; private set; }

        public void Tick(in EnemySenses senses)
        {
            timer = Math.Max(0f, timer - senses.DeltaTime);

            switch (State)
            {
                case EnemyState.Patrol:
                    TickPatrol(senses);
                    break;
                case EnemyState.Chase:
                    TickChase(senses);
                    break;
                case EnemyState.Telegraph:
                    MoveX = 0f;
                    if (timer <= 0f)
                    {
                        Enter(EnemyState.Strike, settings.StrikeTime);
                    }

                    break;
                case EnemyState.Strike:
                    MoveX = 0f;
                    if (timer <= 0f)
                    {
                        Enter(EnemyState.Recover, settings.RecoverTime);
                    }

                    break;
                case EnemyState.Recover:
                case EnemyState.Hurt:
                    MoveX = 0f;
                    if (timer <= 0f)
                    {
                        State = EnemyState.Chase;
                    }

                    break;
                default:
                    MoveX = 0f;
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
            MoveX = 0f;
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
        }

        private void TickChase(in EnemySenses senses)
        {
            if (!Sees(senses, settings.AggroRange * LoseInterestScale))
            {
                State = EnemyState.Patrol;
                return;
            }

            Facing = senses.ToTargetX >= 0f ? 1 : -1;

            if (Math.Abs(senses.ToTargetX) <= settings.AttackRange)
            {
                Enter(EnemyState.Telegraph, settings.TelegraphTime);
                MoveX = 0f;
                return;
            }

            MoveX = senses.BlockedAhead ? 0f : Facing;
        }

        private bool Sees(in EnemySenses senses, float range)
        {
            return Math.Abs(senses.ToTargetX) <= range && Math.Abs(senses.ToTargetY) <= settings.AggroHeight;
        }

        private void Enter(EnemyState state, float duration)
        {
            State = state;
            timer = duration;
        }
    }
}
