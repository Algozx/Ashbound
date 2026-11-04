using Ashbound.Logic.Enemies;
using Ashbound.Logic.Levels;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class EnemyVarietyTests
    {
        private const float Step = 0.02f;

        private static EnemyBrainSettings Settings(bool flies = false)
        {
            return new EnemyBrainSettings
            {
                AggroRange = 9f,
                AggroHeight = 8f,
                AttackRange = 2f,
                AttackHeight = 1f,
                TelegraphTime = 0.1f,
                StrikeTime = 0.1f,
                RecoverTime = 0.1f,
                StrikeMoveScale = 2f,
                Flies = flies,
                HoverHeight = 1f,
            };
        }

        private static EnemySenses Senses(float x, float y)
        {
            return new EnemySenses { ToTargetX = x, ToTargetY = y, DeltaTime = Step };
        }

        [Test]
        public void GroundEnemyDoesNotSwingAtATargetOnAPlatform()
        {
            var brain = new EnemyBrain(Settings());
            brain.Tick(Senses(1f, 4f));

            brain.Tick(Senses(1f, 4f));

            Assert.AreEqual(EnemyState.Chase, brain.State);
        }

        [Test]
        public void GroundEnemyNeverAsksToMoveVertically()
        {
            var brain = new EnemyBrain(Settings());
            brain.Tick(Senses(5f, 4f));

            brain.Tick(Senses(5f, 4f));

            Assert.AreEqual(0f, brain.MoveY);
        }

        [Test]
        public void FlyingEnemyClimbsTowardItsTarget()
        {
            var brain = new EnemyBrain(Settings(true));
            brain.Tick(Senses(5f, 4f));

            brain.Tick(Senses(5f, 4f));

            Assert.Greater(brain.MoveY, 0f);
        }

        [Test]
        public void FlyingEnemyHoversAboveItsTarget()
        {
            var brain = new EnemyBrain(Settings(true));
            brain.Tick(Senses(5f, -1f));

            brain.Tick(Senses(5f, -1f));

            Assert.AreEqual(0f, brain.MoveY, 0.001f);
        }

        [Test]
        public void FlyingEnemyAttacksFromItsHoverHeight()
        {
            var brain = new EnemyBrain(Settings(true));
            brain.Tick(Senses(1f, -1f));

            brain.Tick(Senses(1f, -1f));

            Assert.AreEqual(EnemyState.Telegraph, brain.State);
        }

        [Test]
        public void StrikeLungesForward()
        {
            var brain = new EnemyBrain(Settings());
            brain.Tick(Senses(1f, 0f));
            brain.Tick(Senses(1f, 0f));

            for (int i = 0; i < 7; i++)
            {
                brain.Tick(Senses(1f, 0f));
            }

            Assert.AreEqual(EnemyState.Strike, brain.State);
            Assert.AreEqual(2f, brain.MoveX, 0.001f);
        }

        [Test]
        public void EnemiesGetStrongerDeeperInTheRun()
        {
            var curve = new DifficultyCurve(0.3f, 0.6, 0.2);

            Assert.AreEqual(1f, curve.StatScale(0), 0.001f);
            Assert.AreEqual(1.6f, curve.StatScale(2), 0.001f);
        }

        [Test]
        public void SpawnChanceRisesAndStopsAtCertainty()
        {
            var curve = new DifficultyCurve(0.3f, 0.6, 0.2);

            Assert.AreEqual(0.8, curve.SpawnChance(1), 0.001);
            Assert.AreEqual(1.0, curve.SpawnChance(9), 0.001);
        }
    }
}
