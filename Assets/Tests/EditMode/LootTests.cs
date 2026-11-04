using Ashbound.Logic;
using Ashbound.Logic.Combat;
using Ashbound.Logic.Levels;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class LootTests
    {
        [Test]
        public void HealStopsAtTheMaximum()
        {
            var health = new Health(100);
            health.Damage(30);

            health.Heal(50);

            Assert.AreEqual(100, health.Current);
        }

        [Test]
        public void HealDoesNotReviveTheDead()
        {
            var health = new Health(10);
            health.Damage(10);

            health.Heal(5);

            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void SameSeedOffersTheSameLoot()
        {
            int[] weights = { 3, 1, 1 };

            Assert.AreEqual(LootTable.Pick(new SeededRandom(9), weights), LootTable.Pick(new SeededRandom(9), weights));
        }

        [Test]
        public void ZeroWeightIsNeverPicked()
        {
            var random = new SeededRandom(3);

            for (int i = 0; i < 500; i++)
            {
                Assert.AreNotEqual(1, LootTable.Pick(random, new[] { 1, 0, 1 }));
            }
        }

        [Test]
        public void HeavierEntriesComeUpMoreOften()
        {
            var random = new SeededRandom(11);
            int heavy = 0;

            for (int i = 0; i < 1000; i++)
            {
                if (LootTable.Pick(random, new[] { 3, 1 }) == 0)
                {
                    heavy++;
                }
            }

            Assert.Greater(heavy, 650);
            Assert.Less(heavy, 850);
        }

        [Test]
        public void EmptyTableIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => LootTable.Pick(new SeededRandom(1), new[] { 0, 0 }));
        }
    }
}
