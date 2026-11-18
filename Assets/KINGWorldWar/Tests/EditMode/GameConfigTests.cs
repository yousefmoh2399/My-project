using KingWorldWar.Infrastructure.Config;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class GameConfigTests
    {
        [Test]
        public void Validate_ValidDevConfig_Succeeds()
        {
            var config = GameConfig.CreateInMemory(EnvironmentType.Dev, "0.1.0", "127.0.0.1", 7350);
            var result = config.Validate();

            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void Validate_EmptyVersion_FailsWithConfigInvalid()
        {
            var config = GameConfig.CreateInMemory(EnvironmentType.Dev, "", "127.0.0.1", 7350);
            var result = config.Validate();

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("CONFIG_INVALID", result.Error.Code);
        }

        [Test]
        public void Validate_InvalidPort_Fails()
        {
            var configNegative = GameConfig.CreateInMemory(EnvironmentType.Dev, "0.1.0", "127.0.0.1", -1);
            Assert.IsTrue(configNegative.Validate().IsFailure);

            var configZero = GameConfig.CreateInMemory(EnvironmentType.Dev, "0.1.0", "127.0.0.1", 0);
            Assert.IsTrue(configZero.Validate().IsFailure);

            var configTooHigh = GameConfig.CreateInMemory(EnvironmentType.Dev, "0.1.0", "127.0.0.1", 70000);
            Assert.IsTrue(configTooHigh.Validate().IsFailure);
        }

        [Test]
        public void Validate_ProductionWithLocalhost_Fails()
        {
            var prodLocal = GameConfig.CreateInMemory(EnvironmentType.Prod, "1.0.0", "127.0.0.1", 7350);
            var result = prodLocal.Validate();

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual("CONFIG_INVALID", result.Error.Code);
            Assert.IsTrue(result.Error.Details.Contains("Production environment cannot target localhost"));
        }
    }
}
