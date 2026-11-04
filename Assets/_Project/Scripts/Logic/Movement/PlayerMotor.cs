using System;

namespace Ashbound.Logic.Movement
{
    /// <summary>
    /// Player movement rules: acceleration, jump forgiveness and dash. It only produces a velocity,
    /// so the feel of the game can be tested without the engine.
    /// </summary>
    public sealed class PlayerMotor
    {
        private readonly PlayerMotorSettings settings;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float dashTimer;
        private float dashCooldownTimer;
        private float invulnerabilityTimer;
        private bool airDashUsed;
        private bool jumpRising;

        public PlayerMotor(PlayerMotorSettings settings)
        {
            this.settings = settings;
        }

        public float VelocityX { get; private set; }

        public float VelocityY { get; private set; }

        public int Facing { get; private set; } = 1;

        public MotorState State { get; private set; } = MotorState.Idle;

        public bool IsInvulnerable => invulnerabilityTimer > 0f;

        /// <summary>Advances the motor by one physics step.</summary>
        public void Step(in MotorInput input)
        {
            float dt = input.DeltaTime;
            TickTimers(input, dt);

            if (input.MoveX > 0.01f)
            {
                Facing = 1;
            }
            else if (input.MoveX < -0.01f)
            {
                Facing = -1;
            }

            if (input.Grounded && VelocityY <= 0f)
            {
                VelocityY = 0f;
                jumpRising = false;
                airDashUsed = false;
            }

            if (input.HitCeiling && VelocityY > 0f)
            {
                VelocityY = 0f;
            }

            if (input.DashPressed && CanDash(input.Grounded))
            {
                StartDash(input.Grounded);
            }

            if (dashTimer > 0f)
            {
                VelocityX = settings.DashSpeed * Facing;
                VelocityY = 0f;
                State = MotorState.Dash;
                return;
            }

            ApplyRun(input, dt);
            ApplyJump(input);
            ApplyGravity(input, dt);
            State = PickState(input);
        }

        private void TickTimers(in MotorInput input, float dt)
        {
            coyoteTimer = input.Grounded ? settings.CoyoteTime : Math.Max(0f, coyoteTimer - dt);
            jumpBufferTimer = input.JumpPressed ? settings.JumpBufferTime : Math.Max(0f, jumpBufferTimer - dt);
            dashTimer = Math.Max(0f, dashTimer - dt);
            dashCooldownTimer = Math.Max(0f, dashCooldownTimer - dt);
            invulnerabilityTimer = Math.Max(0f, invulnerabilityTimer - dt);
        }

        private bool CanDash(bool grounded)
        {
            return dashTimer <= 0f && dashCooldownTimer <= 0f && (grounded || !airDashUsed);
        }

        private void StartDash(bool grounded)
        {
            dashTimer = settings.DashDuration;
            dashCooldownTimer = settings.DashCooldown;
            invulnerabilityTimer = settings.DashInvulnerability;
            airDashUsed = !grounded;
            jumpRising = false;
        }

        private void ApplyRun(in MotorInput input, float dt)
        {
            float target = Math.Clamp(input.MoveX, -1f, 1f) * settings.RunSpeed;
            float acceleration = input.Grounded ? settings.GroundAcceleration : settings.AirAcceleration;
            float maxChange = acceleration * dt;
            float difference = target - VelocityX;
            VelocityX += Math.Clamp(difference, -maxChange, maxChange);
        }

        private void ApplyJump(in MotorInput input)
        {
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                VelocityY = settings.JumpVelocity;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                jumpRising = true;
            }

            if (jumpRising && !input.JumpHeld && VelocityY > 0f)
            {
                VelocityY *= settings.JumpCutMultiplier;
                jumpRising = false;
            }
        }

        private void ApplyGravity(in MotorInput input, float dt)
        {
            if (input.Grounded && VelocityY <= 0f)
            {
                return;
            }

            float gravity = settings.Gravity;
            if (VelocityY < 0f)
            {
                gravity *= settings.FallGravityMultiplier;
            }

            VelocityY = Math.Max(VelocityY - gravity * dt, -settings.MaxFallSpeed);
        }

        private MotorState PickState(in MotorInput input)
        {
            if (VelocityY > 0f)
            {
                return MotorState.Jump;
            }

            if (!input.Grounded)
            {
                return MotorState.Fall;
            }

            return Math.Abs(input.MoveX) > 0.01f ? MotorState.Run : MotorState.Idle;
        }
    }
}
