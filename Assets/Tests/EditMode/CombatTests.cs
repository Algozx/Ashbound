using Ashbound.Logic.Combat;
using Ashbound.Logic.Enemies;
using Ashbound.Logic.Movement;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class CombatTests
    {
        private const float Step = 0.02f;

        private static EnemyBrainSettings BrainSettings()
        {
            return new EnemyBrainSettings
            {
                PatrolSpeedScale = 0.5f,
                AggroRange = 8f,
                AggroHeight = 3f,
                AttackRange = 2f,
                TelegraphTime = 0.5f,
                StrikeTime = 0.2f,
                RecoverTime = 0.6f,
                HurtTime = 0.3f,
            };
        }

        private static EnemySenses Senses(float toTargetX, bool blocked = false)
        {
            return new EnemySenses { ToTargetX = toTargetX, BlockedAhead = blocked, DeltaTime = Step };
        }

        private static void Run(EnemyBrain brain, EnemySenses senses, float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                brain.Tick(senses);
            }
        }

        private static EnemyBrain BrainInTelegraph()
        {
            var brain = new EnemyBrain(BrainSettings());
            brain.Tick(Senses(1f));
            brain.Tick(Senses(1f));
            return brain;
        }

        [Test]
        public void HealthStopsAtZeroAndReportsTheKillOnce()
        {
            var health = new Health(10);

            Assert.IsFalse(health.Damage(4));
            Assert.IsTrue(health.Damage(20));
            Assert.IsFalse(health.Damage(1));
            Assert.AreEqual(0, health.Current);
        }

        [Test]
        public void AttackStartsOnPressAndEndsAfterItsDuration()
        {
            var timer = new AttackTimer(0.1f);
            timer.Press();

            Assert.IsTrue(timer.Tick(Step, 0.5f, true));
            Assert.IsTrue(timer.IsActive);

            timer.Tick(0.6f, 0.5f, true);

            Assert.IsFalse(timer.IsActive);
        }

        [Test]
        public void PressNearTheEndChainsTheNextAttack()
        {
            var timer = new AttackTimer(0.1f);
            timer.Press();
            timer.Tick(Step, 0.5f, true);
            timer.Tick(0.45f, 0.5f, true);

            timer.Press();

            Assert.IsTrue(timer.Tick(0.06f, 0.5f, true));
        }

        [Test]
        public void EarlyPressIsForgotten()
        {
            var timer = new AttackTimer(0.1f);
            timer.Press();
            timer.Tick(Step, 1f, true);
            timer.Press();

            timer.Tick(0.2f, 1f, true);
            timer.Tick(0.9f, 1f, true);

            Assert.IsFalse(timer.IsActive);
        }

        [Test]
        public void AttackDoesNotStartWhenNotAllowed()
        {
            var timer = new AttackTimer(0.1f);
            timer.Press();

            Assert.IsFalse(timer.Tick(Step, 0.5f, false));
        }

        [Test]
        public void PatrolTurnsAroundWhenBlocked()
        {
            var brain = new EnemyBrain(BrainSettings());

            brain.Tick(Senses(100f, true));

            Assert.AreEqual(-1, brain.Facing);
            Assert.Less(brain.MoveX, 0f);
        }

        [Test]
        public void EnemyChasesATargetInRange()
        {
            var brain = new EnemyBrain(BrainSettings());

            brain.Tick(Senses(-6f));
            brain.Tick(Senses(-6f));

            Assert.AreEqual(EnemyState.Chase, brain.State);
            Assert.AreEqual(-1f, brain.MoveX);
        }

        [Test]
        public void EnemyIgnoresATargetOnAnotherFloor()
        {
            var brain = new EnemyBrain(BrainSettings());
            EnemySenses senses = Senses(3f);
            senses.ToTargetY = 6f;

            brain.Tick(senses);

            Assert.AreEqual(EnemyState.Patrol, brain.State);
        }

        [Test]
        public void ChaseStopsAtALedge()
        {
            var brain = new EnemyBrain(BrainSettings());
            brain.Tick(Senses(6f));

            brain.Tick(Senses(6f, true));

            Assert.AreEqual(0f, brain.MoveX);
        }

        [Test]
        public void AttackIsAlwaysTelegraphedFirst()
        {
            EnemyBrain brain = BrainInTelegraph();

            Assert.AreEqual(EnemyState.Telegraph, brain.State);

            Run(brain, Senses(1f), 0.3f);
            Assert.AreEqual(EnemyState.Telegraph, brain.State);

            Run(brain, Senses(1f), 0.25f);
            Assert.AreEqual(EnemyState.Strike, brain.State);
        }

        [Test]
        public void EnemyRecoversAfterStriking()
        {
            EnemyBrain brain = BrainInTelegraph();

            Run(brain, Senses(1f), 0.8f);

            Assert.AreEqual(EnemyState.Recover, brain.State);
        }

        [Test]
        public void TelegraphIsNotInterruptedByHits()
        {
            EnemyBrain brain = BrainInTelegraph();

            brain.OnHit();

            Assert.AreEqual(EnemyState.Telegraph, brain.State);
        }

        [Test]
        public void HitStunsAnEnemyThatIsNotAttacking()
        {
            var brain = new EnemyBrain(BrainSettings());

            brain.OnHit();

            Assert.AreEqual(EnemyState.Hurt, brain.State);
        }

        [Test]
        public void DeadEnemyStaysDead()
        {
            var brain = new EnemyBrain(BrainSettings());
            brain.OnKilled();

            brain.OnHit();
            brain.Tick(Senses(1f));

            Assert.AreEqual(EnemyState.Dead, brain.State);
        }

        [Test]
        public void HurtTakesControlAwayAndProtectsThePlayer()
        {
            var motor = new PlayerMotor(new PlayerMotorSettings { RunSpeed = 8f, GroundAcceleration = 80f, JumpHeight = 4f, TimeToApex = 0.4f, FallGravityMultiplier = 1f, MaxFallSpeed = 20f });

            motor.Hurt(-6f, 4f, 0.3f, 0.8f);
            motor.Step(new MotorInput { MoveX = 1f, DeltaTime = Step });

            Assert.AreEqual(MotorState.Hurt, motor.State);
            Assert.AreEqual(-6f, motor.VelocityX, 0.001f);
            Assert.IsTrue(motor.IsInvulnerable);
        }

        [Test]
        public void AttackingOnTheGroundSlowsThePlayer()
        {
            var motor = new PlayerMotor(new PlayerMotorSettings { RunSpeed = 10f, GroundAcceleration = 1000f, AttackMoveScale = 0.2f, TimeToApex = 1f, JumpHeight = 1f });

            motor.Step(new MotorInput { MoveX = 1f, Attacking = true, Grounded = true, DeltaTime = Step });

            Assert.AreEqual(2f, motor.VelocityX, 0.001f);
        }
    }
}
