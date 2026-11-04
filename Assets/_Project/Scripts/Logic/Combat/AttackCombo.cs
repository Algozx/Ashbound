using System;
using System.Collections.Generic;

namespace Ashbound.Logic.Combat
{
    /// <summary>
    /// Timing of a chain of attacks. A press made shortly before a step ends is kept, and the next
    /// press continues the chain until the combo window closes.
    /// </summary>
    public sealed class AttackCombo
    {
        public const int NoStep = -1;

        private readonly float bufferTime;
        private readonly float comboWindow;
        private float bufferTimer;
        private float remaining;
        private float comboTimer;
        private int nextStep;

        public AttackCombo(float bufferTime, float comboWindow)
        {
            this.bufferTime = bufferTime;
            this.comboWindow = comboWindow;
        }

        public bool IsActive => remaining > 0f;

        public void Press()
        {
            bufferTimer = bufferTime;
        }

        /// <summary>Advances time and returns the step that starts on this tick, or <see cref="NoStep"/>.</summary>
        public int Tick(float deltaTime, IReadOnlyList<float> stepDurations, bool canStart)
        {
            bool wasActive = IsActive;
            remaining = Math.Max(0f, remaining - deltaTime);

            if (wasActive && !IsActive)
            {
                comboTimer = comboWindow;
            }
            else if (!IsActive)
            {
                comboTimer = Math.Max(0f, comboTimer - deltaTime);
                if (comboTimer <= 0f)
                {
                    nextStep = 0;
                }
            }

            if (bufferTimer <= 0f)
            {
                return NoStep;
            }

            if (canStart && !IsActive)
            {
                int step = nextStep % stepDurations.Count;
                nextStep = step + 1;
                bufferTimer = 0f;
                remaining = stepDurations[step];
                return step;
            }

            bufferTimer = Math.Max(0f, bufferTimer - deltaTime);
            return NoStep;
        }

        public void Cancel()
        {
            remaining = 0f;
            bufferTimer = 0f;
            comboTimer = 0f;
            nextStep = 0;
        }
    }
}
