using Ashbound.Logic.Levels;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class MapFogTests
    {
        [Test]
        public void StartsHidden()
        {
            Assert.IsFalse(new MapFog(10, 10).IsRevealed(5, 5));
        }

        [Test]
        public void RevealsACircleAroundThePlayer()
        {
            var fog = new MapFog(20, 20);

            fog.Reveal(10, 10, 3);

            Assert.IsTrue(fog.IsRevealed(13, 10));
            Assert.IsFalse(fog.IsRevealed(13, 13));
        }

        [Test]
        public void ReportsOnlyNewCells()
        {
            var fog = new MapFog(20, 20);
            fog.Reveal(10, 10, 3);

            Assert.IsFalse(fog.Reveal(10, 10, 3));
            Assert.IsTrue(fog.Reveal(11, 10, 3));
        }

        [Test]
        public void IgnoresCellsOutsideTheMap()
        {
            var fog = new MapFog(5, 5);

            Assert.DoesNotThrow(() => fog.Reveal(0, 0, 3));
            Assert.IsFalse(fog.IsRevealed(-1, 0));
            Assert.IsFalse(fog.IsRevealed(5, 0));
        }
    }
}
