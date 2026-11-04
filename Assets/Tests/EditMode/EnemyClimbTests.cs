using Ashbound.Logic.Enemies;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class EnemyClimbTests
    {
        private static EnemyBrain ChasingBrain(bool canJump = true)
        {
            var brain = new EnemyBrain(new EnemyBrainSettings
            {
                AggroRange = 9f,
                AggroHeight = 8f,
                AttackRange = 2f,
                AttackHeight = 1.5f,
                CanJump = canJump,
                JumpReach = 6f,
                JumpHeight = 4.5f,
                JumpCooldown = 1f,
            });
            brain.Tick(Senses(5f, 0f));
            return brain;
        }

        private static EnemySenses Senses(float x, float y)
        {
            return new EnemySenses { ToTargetX = x, ToTargetY = y, Grounded = true, DeltaTime = 0.02f };
        }

        [Test]
        public void JumpsTowardATargetAbove()
        {
            EnemyBrain brain = ChasingBrain();

            brain.Tick(Senses(3f, 4f));

            Assert.IsTrue(brain.ConsumeJump());
        }

        [Test]
        public void JumpRequestIsReportedOnce()
        {
            EnemyBrain brain = ChasingBrain();
            brain.Tick(Senses(3f, 4f));
            brain.ConsumeJump();

            Assert.IsFalse(brain.ConsumeJump());
        }

        [Test]
        public void WaitsForTheCooldownBetweenJumps()
        {
            EnemyBrain brain = ChasingBrain();
            brain.Tick(Senses(3f, 4f));
            brain.ConsumeJump();

            brain.Tick(Senses(3f, 4f));

            Assert.IsFalse(brain.ConsumeJump());
        }

        [Test]
        public void DoesNotJumpInTheAir()
        {
            EnemyBrain brain = ChasingBrain();
            EnemySenses senses = Senses(3f, 4f);
            senses.Grounded = false;

            brain.Tick(senses);

            Assert.IsFalse(brain.ConsumeJump());
        }

        [Test]
        public void IgnoresATargetTooFarToReach()
        {
            EnemyBrain brain = ChasingBrain();

            brain.Tick(Senses(8f, 4f));

            Assert.IsFalse(brain.ConsumeJump());
        }

        [Test]
        public void IgnoresATargetTooHighToReach()
        {
            EnemyBrain brain = ChasingBrain();

            brain.Tick(Senses(3f, 6f));

            Assert.IsFalse(brain.ConsumeJump());
        }

        [Test]
        public void WalksToThePlatformEndInsteadOfJumpingUnderIt()
        {
            EnemyBrain brain = ChasingBrain();
            EnemySenses senses = Senses(1f, 4f);
            senses.ClimbDetour = -1;

            brain.Tick(senses);

            Assert.IsFalse(brain.ConsumeJump());
            Assert.AreEqual(-1, brain.Facing);
            Assert.AreEqual(-1f, brain.MoveX);
        }

        [Test]
        public void ClimbsAWallOnTheWayToTheTarget()
        {
            EnemyBrain brain = ChasingBrain();
            EnemySenses senses = Senses(5f, 0f);
            senses.WallAhead = true;
            senses.BlockedAhead = true;

            brain.Tick(senses);

            Assert.IsTrue(brain.ConsumeJump());
        }

        [Test]
        public void WalksOffALedgeToReachALowerTarget()
        {
            EnemyBrain brain = ChasingBrain(false);
            EnemySenses senses = Senses(5f, -4f);
            senses.LedgeAhead = true;
            senses.BlockedAhead = true;

            brain.Tick(senses);

            Assert.AreEqual(1f, brain.MoveX);
        }

        [Test]
        public void GroundedWalkerStopsAtALedgeWhenTheTargetIsLevel()
        {
            EnemyBrain brain = ChasingBrain(false);
            EnemySenses senses = Senses(5f, 0f);
            senses.LedgeAhead = true;
            senses.BlockedAhead = true;

            brain.Tick(senses);

            Assert.AreEqual(0f, brain.MoveX);
        }

        [Test]
        public void NeverLeavesItsRoom()
        {
            EnemyBrain brain = ChasingBrain();
            EnemySenses senses = Senses(5f, 4f);
            senses.RoomEdgeAhead = true;
            senses.BlockedAhead = true;

            brain.Tick(senses);

            Assert.IsFalse(brain.ConsumeJump());
            Assert.AreEqual(0f, brain.MoveX);
        }
    }
}
