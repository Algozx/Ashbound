using Ashbound.Logic.Enemies;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class BossTests
    {
        private const float Step = 0.02f;

        private static BossBrain AwakeBoss()
        {
            var brain = new BossBrain(new BossBrainSettings
            {
                MeleeRange = 3f,
                MeleeWindup = 0.4f,
                MeleeStrike = 0.3f,
                CastWindup = 0.5f,
                CastRelease = 0.4f,
                Recover = 1f,
                CastInterval = 5f,
                MeleesBeforeCast = 2,
                EnrageHealthShare = 0.5f,
                EnragedRecoverScale = 0.5f,
                SpellsPerCast = 1,
                EnragedSpellsPerCast = 3,
            });
            brain.Wake();
            return brain;
        }

        private static void Run(BossBrain brain, float toTargetX, float seconds)
        {
            for (float t = 0f; t < seconds; t += Step)
            {
                brain.Tick(toTargetX, Step);
            }
        }

        [Test]
        public void WaitsUntilTheFightStarts()
        {
            var brain = new BossBrain(new BossBrainSettings { CastInterval = 1f });

            brain.Tick(1f, Step);

            Assert.AreEqual(BossState.Waiting, brain.State);
        }

        [Test]
        public void WalksTowardAFarTarget()
        {
            BossBrain brain = AwakeBoss();

            brain.Tick(-10f, Step);

            Assert.AreEqual(BossState.Approach, brain.State);
            Assert.AreEqual(-1f, brain.MoveX);
        }

        [Test]
        public void SwingsAtACloseTarget()
        {
            BossBrain brain = AwakeBoss();

            brain.Tick(2f, Step);

            Assert.AreEqual(BossState.MeleeWindup, brain.State);
            Assert.AreEqual(1, brain.Facing);
        }

        [Test]
        public void SwingFollowsItsTimingAndRecovers()
        {
            BossBrain brain = AwakeBoss();
            brain.Tick(2f, Step);

            Run(brain, 2f, 0.45f);
            Assert.AreEqual(BossState.MeleeStrike, brain.State);

            Run(brain, 2f, 0.35f);
            Assert.AreEqual(BossState.Recover, brain.State);
        }

        [Test]
        public void KeepingAwayStillDrawsASpell()
        {
            BossBrain brain = AwakeBoss();

            Run(brain, 20f, 5.1f);

            Assert.AreEqual(BossState.CastWindup, brain.State);
        }

        [Test]
        public void CastsAfterTwoSwingsInARow()
        {
            BossBrain brain = AwakeBoss();
            brain.Tick(2f, Step);
            Run(brain, 2f, 1.8f);
            Assert.AreEqual(BossState.MeleeWindup, brain.State);

            Run(brain, 2f, 1.8f);

            Assert.AreEqual(BossState.CastWindup, brain.State);
        }

        [Test]
        public void EnragesAtHalfHealth()
        {
            BossBrain brain = AwakeBoss();
            Assert.AreEqual(1, brain.SpellCount);

            brain.OnHealthChanged(0.5f);

            Assert.IsTrue(brain.IsEnraged);
            Assert.AreEqual(3, brain.SpellCount);
        }

        [Test]
        public void EnragedBossRecoversFaster()
        {
            BossBrain brain = AwakeBoss();
            brain.OnHealthChanged(0.2f);
            brain.Tick(2f, Step);
            Run(brain, 2f, 0.8f);
            Assert.AreEqual(BossState.Recover, brain.State);

            Run(brain, 2f, 0.55f);

            Assert.AreNotEqual(BossState.Recover, brain.State);
        }

        [Test]
        public void DeadBossDoesNothing()
        {
            BossBrain brain = AwakeBoss();
            brain.OnKilled();

            brain.Tick(1f, Step);

            Assert.AreEqual(BossState.Dead, brain.State);
            Assert.AreEqual(0f, brain.MoveX);
        }
    }
}
