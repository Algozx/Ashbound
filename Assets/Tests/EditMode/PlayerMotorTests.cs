using Ashbound.Logic.Movement;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class PlayerMotorTests
    {
        private const float Step = 0.02f;

        private static PlayerMotorSettings Settings()
        {
            return new PlayerMotorSettings
            {
                RunSpeed = 8f,
                GroundAcceleration = 80f,
                AirAcceleration = 40f,
                JumpHeight = 4f,
                TimeToApex = 0.4f,
                FallGravityMultiplier = 1.5f,
                MaxFallSpeed = 20f,
                JumpCutMultiplier = 0.5f,
                CoyoteTime = 0.1f,
                JumpBufferTime = 0.1f,
                DashSpeed = 18f,
                DashDuration = 0.2f,
                DashCooldown = 0.6f,
                DashInvulnerability = 0.2f,
            };
        }

        private static MotorInput Grounded(float moveX = 0f)
        {
            return new MotorInput { Grounded = true, MoveX = moveX, DeltaTime = Step };
        }

        private static MotorInput Air(float moveX = 0f)
        {
            return new MotorInput { MoveX = moveX, DeltaTime = Step };
        }

        private static void Repeat(PlayerMotor motor, MotorInput input, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                motor.Step(input);
            }
        }

        [Test]
        public void RunReachesTopSpeedAndNoMore()
        {
            var motor = new PlayerMotor(Settings());

            Repeat(motor, Grounded(1f), 30);

            Assert.AreEqual(8f, motor.VelocityX, 0.001f);
            Assert.AreEqual(MotorState.Run, motor.State);
        }

        [Test]
        public void MovingLeftTurnsThePlayer()
        {
            var motor = new PlayerMotor(Settings());

            motor.Step(Grounded(-1f));

            Assert.AreEqual(-1, motor.Facing);
        }

        [Test]
        public void JumpReachesTheConfiguredHeight()
        {
            PlayerMotorSettings settings = Settings();
            var motor = new PlayerMotor(settings);
            motor.Step(Grounded());

            MotorInput jump = Grounded();
            jump.JumpPressed = true;
            jump.JumpHeld = true;
            motor.Step(jump);

            float height = motor.VelocityY * Step;
            MotorInput held = Air();
            held.JumpHeld = true;
            while (motor.VelocityY > 0f)
            {
                motor.Step(held);
                height += System.Math.Max(0f, motor.VelocityY) * Step;
            }

            Assert.AreEqual(settings.JumpHeight, height, 0.25f);
        }

        [Test]
        public void ReleasingJumpCutsTheRise()
        {
            var motor = new PlayerMotor(Settings());
            motor.Step(Grounded());
            MotorInput jump = Grounded();
            jump.JumpPressed = true;
            jump.JumpHeld = true;
            motor.Step(jump);
            float rising = motor.VelocityY;

            motor.Step(Air());

            Assert.Less(motor.VelocityY, rising * 0.6f);
        }

        [Test]
        public void JumpWorksJustAfterLeavingALedge()
        {
            var motor = new PlayerMotor(Settings());
            motor.Step(Grounded());
            Repeat(motor, Air(), 3);

            MotorInput jump = Air();
            jump.JumpPressed = true;
            jump.JumpHeld = true;
            motor.Step(jump);

            Assert.Greater(motor.VelocityY, 0f);
        }

        [Test]
        public void JumpFailsLongAfterLeavingALedge()
        {
            var motor = new PlayerMotor(Settings());
            motor.Step(Grounded());
            Repeat(motor, Air(), 10);

            MotorInput jump = Air();
            jump.JumpPressed = true;
            jump.JumpHeld = true;
            motor.Step(jump);

            Assert.LessOrEqual(motor.VelocityY, 0f);
        }

        [Test]
        public void JumpPressedJustBeforeLandingStillFires()
        {
            var motor = new PlayerMotor(Settings());
            Repeat(motor, Air(), 20);
            MotorInput press = Air();
            press.JumpPressed = true;
            press.JumpHeld = true;
            motor.Step(press);
            MotorInput held = Air();
            held.JumpHeld = true;
            Repeat(motor, held, 2);

            MotorInput landed = Grounded();
            landed.JumpHeld = true;
            motor.Step(landed);

            Assert.Greater(motor.VelocityY, 0f);
        }

        [Test]
        public void DashMovesFastAndProtectsThePlayer()
        {
            var motor = new PlayerMotor(Settings());
            MotorInput dash = Grounded(1f);
            dash.DashPressed = true;

            motor.Step(dash);

            Assert.AreEqual(18f, motor.VelocityX, 0.001f);
            Assert.AreEqual(MotorState.Dash, motor.State);
            Assert.IsTrue(motor.IsInvulnerable);
        }

        [Test]
        public void DashEndsAndProtectionFades()
        {
            var motor = new PlayerMotor(Settings());
            MotorInput dash = Grounded(1f);
            dash.DashPressed = true;
            motor.Step(dash);

            Repeat(motor, Grounded(), 12);

            Assert.AreNotEqual(MotorState.Dash, motor.State);
            Assert.IsFalse(motor.IsInvulnerable);
        }

        [Test]
        public void DashWaitsForItsCooldown()
        {
            var motor = new PlayerMotor(Settings());
            MotorInput dash = Grounded(1f);
            dash.DashPressed = true;
            motor.Step(dash);
            Repeat(motor, Grounded(), 12);

            motor.Step(dash);

            Assert.AreNotEqual(MotorState.Dash, motor.State);
        }

        [Test]
        public void OnlyOneDashPerAirTime()
        {
            var motor = new PlayerMotor(Settings());
            MotorInput dash = Air(1f);
            dash.DashPressed = true;
            motor.Step(dash);
            Repeat(motor, Air(), 40);

            motor.Step(dash);

            Assert.AreNotEqual(MotorState.Dash, motor.State);
        }

        [Test]
        public void CeilingStopsTheRise()
        {
            var motor = new PlayerMotor(Settings());
            motor.Step(Grounded());
            MotorInput jump = Grounded();
            jump.JumpPressed = true;
            jump.JumpHeld = true;
            motor.Step(jump);

            MotorInput ceiling = Air();
            ceiling.JumpHeld = true;
            ceiling.HitCeiling = true;
            motor.Step(ceiling);

            Assert.LessOrEqual(motor.VelocityY, 0f);
        }
    }
}
