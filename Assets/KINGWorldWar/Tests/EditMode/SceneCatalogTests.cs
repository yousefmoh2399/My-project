using KingWorldWar.Core.Scenes;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class SceneCatalogTests
    {
        [Test]
        public void SceneCatalog_RequiredScenes_ContainsBootstrapAndMainMenu()
        {
            Assert.Contains(SceneNameCatalog.Bootstrap, SceneNameCatalog.RequiredScenes);
            Assert.Contains(SceneNameCatalog.MainMenu, SceneNameCatalog.RequiredScenes);
        }

        [Test]
        public void IsValid_KnownScenes_ReturnsTrue()
        {
            Assert.IsTrue(SceneNameCatalog.IsValid(SceneNameCatalog.Bootstrap));
            Assert.IsTrue(SceneNameCatalog.IsValid(SceneNameCatalog.MainMenu));
            Assert.IsTrue(SceneNameCatalog.IsValid(SceneNameCatalog.Base));
            Assert.IsTrue(SceneNameCatalog.IsValid(SceneNameCatalog.WorldMap));
            Assert.IsTrue(SceneNameCatalog.IsValid(SceneNameCatalog.BattleReport));
        }

        [Test]
        public void IsValid_UnknownOrNull_ReturnsFalse()
        {
            Assert.IsFalse(SceneNameCatalog.IsValid(null));
            Assert.IsFalse(SceneNameCatalog.IsValid(""));
            Assert.IsFalse(SceneNameCatalog.IsValid("NonExistentScene_XYZ"));
        }
    }
}
