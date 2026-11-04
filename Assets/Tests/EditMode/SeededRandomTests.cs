using Ashbound.Logic;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class SeededRandomTests
    {
        [Test]
        public void SameSeedGivesSameSequence()
        {
            var first = new SeededRandom(1234);
            var second = new SeededRandom(1234);

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(first.NextUInt64(), second.NextUInt64());
            }
        }

        [Test]
        public void KnownSeedKeepsItsFirstValue()
        {
            // Reference value of SplitMix64 for seed 0. A change here breaks every shared seed.
            Assert.AreEqual(0xE220A8397B1DCDAFUL, new SeededRandom(0).NextUInt64());
        }

        [Test]
        public void DifferentSeedsDiverge()
        {
            Assert.AreNotEqual(new SeededRandom(1).NextUInt64(), new SeededRandom(2).NextUInt64());
        }

        [Test]
        public void NextIntStaysInRange()
        {
            var random = new SeededRandom(99);

            for (int i = 0; i < 10000; i++)
            {
                int value = random.NextInt(-3, 4);
                Assert.GreaterOrEqual(value, -3);
                Assert.Less(value, 4);
            }
        }

        [Test]
        public void NextIntRejectsEmptyRange()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new SeededRandom(1).NextInt(5, 5));
        }

        [Test]
        public void NextDoubleStaysInRange()
        {
            var random = new SeededRandom(7);

            for (int i = 0; i < 10000; i++)
            {
                double value = random.NextDouble();
                Assert.GreaterOrEqual(value, 0.0);
                Assert.Less(value, 1.0);
            }
        }

        [Test]
        public void ForkDoesNotDependOnLaterRollsOfTheParent()
        {
            var first = new SeededRandom(5);
            var second = new SeededRandom(5);

            SeededRandom firstLevel = first.Fork(1);
            SeededRandom secondLevel = second.Fork(1);
            second.NextUInt64();

            Assert.AreEqual(firstLevel.NextUInt64(), secondLevel.NextUInt64());
        }
    }
}
