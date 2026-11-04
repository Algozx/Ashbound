using Ashbound.Logic.Combat;
using Ashbound.Logic.Meta;
using NUnit.Framework;

namespace Ashbound.Tests
{
    public class ProgressionTests
    {
        private static readonly int[] Costs = { 40, 80 };

        private static EmberWallet WalletWith(int banked, int carried, out Profile profile)
        {
            profile = new Profile();
            profile.Bank(banked);
            var wallet = new EmberWallet(profile);
            wallet.Collect(carried);
            return wallet;
        }

        [Test]
        public void DeathKeepsHalfOfWhatIsCarried()
        {
            EmberWallet wallet = WalletWith(10, 31, out Profile profile);

            int kept = wallet.SettleDeath();

            Assert.AreEqual(15, kept);
            Assert.AreEqual(25, profile.Embers);
            Assert.AreEqual(0, wallet.Carried);
        }

        [Test]
        public void VictoryKeepsEverything()
        {
            EmberWallet wallet = WalletWith(10, 31, out Profile profile);

            int kept = wallet.SettleVictory();

            Assert.AreEqual(31, kept);
            Assert.AreEqual(41, profile.Embers);
        }

        [Test]
        public void PurchasesUseCarriedEmbersFirst()
        {
            EmberWallet wallet = WalletWith(100, 30, out Profile profile);

            Assert.IsTrue(wallet.TryBuy("health", Costs));

            Assert.AreEqual(0, wallet.Carried);
            Assert.AreEqual(90, profile.Embers);
        }

        [Test]
        public void CannotBuyWithoutEnoughEmbers()
        {
            EmberWallet wallet = WalletWith(10, 29, out Profile profile);

            Assert.IsFalse(wallet.TryBuy("health", Costs));

            Assert.AreEqual(0, profile.Level("health"));
            Assert.AreEqual(39, wallet.Total);
        }

        [Test]
        public void EachLevelCostsItsOwnPrice()
        {
            EmberWallet wallet = WalletWith(500, 0, out Profile profile);

            wallet.TryBuy("health", Costs);

            Assert.AreEqual(1, profile.Level("health"));
            Assert.AreEqual(80, wallet.NextCost("health", Costs));
        }

        [Test]
        public void CompleteUnlockCannotBeBoughtAgain()
        {
            EmberWallet wallet = WalletWith(500, 0, out Profile profile);
            wallet.TryBuy("health", Costs);
            wallet.TryBuy("health", Costs);

            Assert.AreEqual(-1, wallet.NextCost("health", Costs));
            Assert.IsFalse(wallet.TryBuy("health", Costs));
            Assert.AreEqual(380, profile.Embers);
        }

        [Test]
        public void ProfileSurvivesSaving()
        {
            EmberWallet wallet = WalletWith(120, 0, out Profile profile);
            wallet.TryBuy("health", Costs);

            Profile loaded = Profile.FromData(profile.ToData());

            Assert.AreEqual(80, loaded.Embers);
            Assert.AreEqual(1, loaded.Level("health"));
        }

        [Test]
        public void MissingDataGivesAnEmptyProfile()
        {
            Profile profile = Profile.FromData(null);

            Assert.AreEqual(0, profile.Embers);
            Assert.AreEqual(0, profile.Level("health"));
        }

        [Test]
        public void SavedDataHasTheCurrentVersion()
        {
            Assert.AreEqual(ProfileData.CurrentVersion, new Profile().ToData().version);
        }

        [Test]
        public void RaisingMaximumHealthAlsoHeals()
        {
            var health = new Health(100);
            health.Damage(30);

            health.RaiseMax(15);

            Assert.AreEqual(115, health.Max);
            Assert.AreEqual(85, health.Current);
        }
    }
}
