using Ashbound.Logic;
using Ashbound.Logic.Enemies;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class EnemyPatrolTests
    {
        private const float Step = 0.02f;
        private const float FarAway = 100f;

        private static EnemyBrainSettings Settings(bool canJump = false, bool flies = false)
        {
            return new EnemyBrainSettings
            {
                PatrolSpeedScale = 0.5f,
                PatrolRadius = 8f,
                PatrolLegTime = 3f,
                PatrolRestTime = 1f,
                GiveUpTime = 1f,
                AggroRange = 9f,
                AggroHeight = 7f,
                AttackRange = 2f,
                AttackHeight = 1.5f,
                TelegraphTime = 0.5f,
                CanJump = canJump,
                JumpReach = 6f,
                JumpHeight = 4.5f,
                JumpCooldown = 1f,
                Flies = flies,
            };
        }

        private static EnemySenses Senses(float x, float toTargetX)
        {
            return new EnemySenses { X = x, ToTargetX = toTargetX, Grounded = true, DeltaTime = Step };
        }

        private static void Run(EnemyBrain brain, EnemySenses senses, float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                brain.Tick(senses);
            }
        }

        private static EnemyBrain ChasingBrain(EnemyBrainSettings settings, float x)
        {
            var brain = new EnemyBrain(settings);
            brain.Tick(Senses(x, 5f));
            Assert.AreEqual(EnemyState.Chase, brain.State);
            return brain;
        }

        [Test]
        public void RestsAfterAWalk()
        {
            var brain = new EnemyBrain(Settings());

            Run(brain, Senses(0f, FarAway), 3.1f);

            Assert.AreEqual(EnemyState.Rest, brain.State);
            Assert.AreEqual(0f, brain.MoveX);
        }

        [Test]
        public void WalksAgainAfterARest()
        {
            var brain = new EnemyBrain(Settings());

            Run(brain, Senses(0f, FarAway), 4.2f);

            Assert.AreEqual(EnemyState.Patrol, brain.State);
        }

        [Test]
        public void StopsAtTheEndOfItsPatrolArea()
        {
            var brain = new EnemyBrain(Settings());
            brain.Tick(Senses(0f, FarAway));

            brain.Tick(Senses(8f, FarAway));

            Assert.AreEqual(EnemyState.Rest, brain.State);
        }

        [Test]
        public void HeadsBackAfterRestingFarFromHome()
        {
            var brain = new EnemyBrain(Settings());
            brain.Tick(Senses(0f, FarAway));
            brain.Tick(Senses(8f, FarAway));

            Run(brain, Senses(8f, FarAway), 1.1f);

            Assert.AreEqual(EnemyState.Patrol, brain.State);
            Assert.AreEqual(-1, brain.Facing);
        }

        [Test]
        public void SameSeedWalksTheSameWay()
        {
            var first = new EnemyBrain(Settings());
            var second = new EnemyBrain(Settings());
            first.UseRandom(new SeededRandom(42));
            second.UseRandom(new SeededRandom(42));

            Run(first, Senses(0f, FarAway), 30f);
            Run(second, Senses(0f, FarAway), 30f);

            Assert.AreEqual(first.State, second.State);
            Assert.AreEqual(first.Facing, second.Facing);
        }

        [Test]
        public void JumpsUpAStepWhilePatrolling()
        {
            var brain = new EnemyBrain(Settings(true));
            EnemySenses senses = Senses(0f, FarAway);
            senses.WallAhead = true;
            senses.BlockedAhead = true;

            brain.Tick(senses);

            Assert.IsTrue(brain.ConsumeJump());
            Assert.AreEqual(1, brain.Facing);
        }

        [Test]
        public void TurnsAtAWallItCannotClimb()
        {
            var brain = new EnemyBrain(Settings(true));
            EnemySenses senses = Senses(0f, FarAway);
            senses.WallAhead = true;
            senses.BlockedAhead = true;
            brain.Tick(senses);
            brain.ConsumeJump();

            brain.Tick(senses);

            Assert.AreEqual(-1, brain.Facing);
        }

        [Test]
        public void WaitsAMomentWhenTheTargetLeavesItsTerritory()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses senses = Senses(0f, 5f);
            senses.TargetOutsideTerritory = true;

            Run(brain, senses, 0.5f);

            Assert.AreEqual(EnemyState.Chase, brain.State);
        }

        [Test]
        public void GoesHomeWhenTheTargetLeavesItsTerritory()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses senses = Senses(6f, 5f);
            senses.TargetOutsideTerritory = true;

            Run(brain, senses, 1.1f);
            brain.Tick(senses);

            Assert.AreEqual(EnemyState.Return, brain.State);
            Assert.AreEqual(-1, brain.Facing);
            Assert.Less(brain.MoveX, 0f);
        }

        [Test]
        public void GoesHomeWhenStuckBelowAnUnreachableTarget()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses senses = Senses(6f, 1f);
            senses.ToTargetY = 4f;

            Run(brain, senses, 1.1f);

            Assert.AreEqual(EnemyState.Return, brain.State);
        }

        [Test]
        public void IgnoresTheTargetForAMomentAfterGivingUp()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses outside = Senses(6f, 5f);
            outside.TargetOutsideTerritory = true;
            Run(brain, outside, 1.1f);

            brain.Tick(Senses(6f, 5f));

            Assert.AreEqual(EnemyState.Return, brain.State);
        }

        [Test]
        public void ChasesAgainWhenTheTargetComesBack()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses outside = Senses(6f, 5f);
            outside.TargetOutsideTerritory = true;
            Run(brain, outside, 1.1f);

            Run(brain, Senses(6f, 5f), 2.1f);

            Assert.AreEqual(EnemyState.Chase, brain.State);
        }

        [Test]
        public void RestsWhenItArrivesHome()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses outside = Senses(6f, 5f);
            outside.TargetOutsideTerritory = true;
            Run(brain, outside, 1.1f);

            outside.X = 0.2f;
            brain.Tick(outside);

            Assert.AreEqual(EnemyState.Rest, brain.State);
        }

        [Test]
        public void MakesANewHomeWhenTheWayBackIsBlocked()
        {
            EnemyBrain brain = ChasingBrain(Settings(), 0f);
            EnemySenses outside = Senses(6f, 5f);
            outside.TargetOutsideTerritory = true;
            Run(brain, outside, 1.1f);

            outside.BlockedAhead = true;
            brain.Tick(outside);
            Run(brain, Senses(6f, FarAway), 1.1f);

            // Rested at the new home, so the next walk is not forced back to the old one.
            Assert.AreEqual(EnemyState.Patrol, brain.State);
            Assert.Less(System.Math.Abs(brain.MoveX), 1f);
        }

        [Test]
        public void FlyingEnemyReturnsToItsHeight()
        {
            var brain = new EnemyBrain(Settings(flies: true));
            brain.Tick(Senses(0f, 5f));
            EnemySenses outside = Senses(4f, 5f);
            outside.Y = 3f;
            outside.TargetOutsideTerritory = true;

            Run(brain, outside, 1.1f);
            brain.Tick(outside);

            Assert.AreEqual(EnemyState.Return, brain.State);
            Assert.Less(brain.MoveY, 0f);
        }
    }
}
