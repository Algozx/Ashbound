namespace Ashbound.Logic.Movement
{
    /// <summary>What the motor needs to know about one physics step.</summary>
    public struct MotorInput
    {
        public float MoveX;
        public bool JumpPressed;
        public bool JumpHeld;
        public bool DashPressed;
        public bool Attacking;
        public bool Grounded;
        public bool HitCeiling;
        public float DeltaTime;
    }
}
