using System;

namespace Ashbound.Logic.Combat
{
    /// <summary>
    /// Timing of one attack. A press made shortly before the current attack ends is kept, so
    /// chained attacks do not depend on frame-perfect input.
    /// </summary>
    public sealed class AttackTimer
    {
        private readonly float bufferTime;
        private float bufferTimer;
        private float remaining;

        public AttackTimer(float bufferTime)
        {
            this.bufferTime = bufferTime;
        }

        public bool IsActive => remaining > 0f;

        public void Press()
        {
            bufferTimer = bufferTime;
        }

        /// <summary>Advances time and reports whether a new attack starts on this tick.</summary>
        public bool Tick(float deltaTime, float attackDuration, bool canStart)
        {
            remaining = Math.Max(0f, remaining - deltaTime);

            if (bufferTimer <= 0f)
            {
                return false;
            }

            if (canStart && !IsActive)
            {
                bufferTimer = 0f;
                remaining = attackDuration;
                return true;
            }

            bufferTimer = Math.Max(0f, bufferTimer - deltaTime);
            return false;
        }

        public void Cancel()
        {
            remaining = 0f;
            bufferTimer = 0f;
        }
    }
}
