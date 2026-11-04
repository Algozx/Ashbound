namespace Ashbound.Logic.Movement
{
    /// <summary>Movement values for one step of <see cref="PlayerMotor"/>.</summary>
    public sealed class PlayerMotorSettings
    {
        public float RunSpeed;
        public float GroundAcceleration;
        public float AirAcceleration;
        public float JumpHeight;
        public float TimeToApex;
        public float FallGravityMultiplier;
        public float MaxFallSpeed;
        public float JumpCutMultiplier;
        public float CoyoteTime;
        public float JumpBufferTime;
        public float DashSpeed;
        public float DashDuration;
        public float DashCooldown;
        public float DashInvulnerability;
        public float AttackMoveScale;

        public float Gravity => 2f * JumpHeight / (TimeToApex * TimeToApex);

        public float JumpVelocity => Gravity * TimeToApex;
    }
}
