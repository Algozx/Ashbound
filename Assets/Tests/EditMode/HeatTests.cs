using Ashbound.Logic.Combat;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class HeatTests
    {
        private static Heat NewHeat()
        {
            return new Heat(6, 6f, new[] { 0.05f, 0.1f, 0.2f }, new[] { 0.25f, 0.5f, 0.75f });
        }

        private static void Hit(Heat heat, int times)
        {
            for (int i = 0; i < times; i++)
            {
                heat.RegisterHit();
            }
        }

        [Test]
        public void StartsCold()
        {
            Heat heat = NewHeat();

            Assert.AreEqual(0, heat.Level);
            Assert.AreEqual(1f, heat.DamageScale);
            Assert.AreEqual(1f, heat.EmberScale);
        }

        [Test]
        public void GainsALevelEverySixHits()
        {
            Heat heat = NewHeat();

            Hit(heat, 5);
            Assert.AreEqual(0, heat.Level);

            Hit(heat, 1);
            Assert.AreEqual(1, heat.Level);
        }

        [Test]
        public void LevelsRaiseDamageAndEmbers()
        {
            Heat heat = NewHeat();

            Hit(heat, 18);

            Assert.AreEqual(1.2f, heat.DamageScale, 0.001f);
            Assert.AreEqual(1.75f, heat.EmberScale, 0.001f);
        }

        [Test]
        public void StopsAtTheLastLevel()
        {
            Heat heat = NewHeat();

            Hit(heat, 40);

            Assert.AreEqual(3, heat.Level);
        }

        [Test]
        public void TakingDamagePutsItOut()
        {
            Heat heat = NewHeat();
            Hit(heat, 13);

            heat.Extinguish();
            Hit(heat, 5);

            Assert.AreEqual(0, heat.Level);
        }

        [Test]
        public void LosesOneLevelAfterTimeWithoutAHit()
        {
            Heat heat = NewHeat();
            Hit(heat, 12);

            heat.Tick(6f);

            Assert.AreEqual(1, heat.Level);
        }

        [Test]
        public void KeepsCoolingWhileIdle()
        {
            Heat heat = NewHeat();
            Hit(heat, 12);

            heat.Tick(6f);
            heat.Tick(6f);

            Assert.AreEqual(0, heat.Level);
        }

        [Test]
        public void AHitResetsTheCooling()
        {
            Heat heat = NewHeat();
            Hit(heat, 6);
            heat.Tick(5f);

            heat.RegisterHit();
            heat.Tick(5f);

            Assert.AreEqual(1, heat.Level);
        }

        [Test]
        public void PartialProgressCoolsToo()
        {
            Heat heat = NewHeat();
            Hit(heat, 5);

            heat.Tick(6f);
            Hit(heat, 1);

            Assert.AreEqual(0, heat.Level);
        }

        [Test]
        public void ReportsLevelChanges()
        {
            Heat heat = NewHeat();
            int reported = -1;
            heat.LevelChanged += level => reported = level;

            Hit(heat, 6);

            Assert.AreEqual(1, reported);
        }
    }
}
