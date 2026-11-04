using System.Linq;
using Ashbound.Logic;
using Ashbound.Logic.Levels;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class RoomChainTests
    {
        [Test]
        public void SameSeedBuildsTheSameRun()
        {
            CollectionAssert.AreEqual(
                RoomChain.Build(new SeededRandom(42), 6, 5),
                RoomChain.Build(new SeededRandom(42), 6, 5));
        }

        [Test]
        public void DifferentSeedsBuildDifferentRuns()
        {
            int distinct = Enumerable.Range(0, 50)
                .Select(seed => string.Join(",", RoomChain.Build(new SeededRandom((ulong)seed), 4, 3)))
                .Distinct().Count();

            Assert.Greater(distinct, 10);
        }

        [Test]
        public void RoomsDoNotRepeatWhileThePoolLasts()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                int[] chain = RoomChain.Build(new SeededRandom(seed), 4, 4);
                Assert.AreEqual(4, chain.Distinct().Count());
            }
        }

        [Test]
        public void LongRunsNeverRepeatARoomBackToBack()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                int[] chain = RoomChain.Build(new SeededRandom(seed), 3, 12);
                for (int i = 1; i < chain.Length; i++)
                {
                    Assert.AreNotEqual(chain[i - 1], chain[i]);
                }
            }
        }

        [Test]
        public void EveryRoomHasAtLeastOneEnemy()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                Assert.IsTrue(RoomChain.PickSpawns(new SeededRandom(seed), 3, 0.01).Any(used => used));
            }
        }

        [Test]
        public void EmptyRoomStaysEmpty()
        {
            Assert.IsEmpty(RoomChain.PickSpawns(new SeededRandom(1), 0, 1.0));
        }
    }
}
