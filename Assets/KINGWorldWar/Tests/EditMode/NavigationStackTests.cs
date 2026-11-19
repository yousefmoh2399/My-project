using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Infrastructure.UI;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class NavigationStackTests
    {
        private class DummyLog : ILog
        {
            public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;
            public void Trace(string category, string message) { }
            public void Debug(string category, string message) { }
            public void Info(string category, string message) { }
            public void Warning(string category, string message, System.Exception exception = null) { }
            public void Error(string category, string message, System.Exception exception = null) { }
        }

        private class MockScreen : IScreen
        {
            public ScreenId ScreenId { get; }
            public bool IsVisible { get; private set; }

            public MockScreen(ScreenId id) => ScreenId = id;
            public void Show() => IsVisible = true;
            public void Hide() => IsVisible = false;
        }

        private ScreenNavigationService _nav;
        private MockScreen _worldScreen;
        private MockScreen _baseScreen;
        private MockScreen _armyScreen;

        [SetUp]
        public void SetUp()
        {
            _nav = new ScreenNavigationService(new DummyLog());
            _worldScreen = new MockScreen(ScreenId.World);
            _baseScreen = new MockScreen(ScreenId.Base);
            _armyScreen = new MockScreen(ScreenId.Army);

            _nav.RegisterScreen(ScreenId.World, _worldScreen);
            _nav.RegisterScreen(ScreenId.Base, _baseScreen);
            _nav.RegisterScreen(ScreenId.Army, _armyScreen);
        }

        [Test]
        public void NavigateTo_SetsCurrentScreenAndShowsIt()
        {
            _nav.NavigateTo(ScreenId.World);

            Assert.AreEqual(ScreenId.World, _nav.CurrentScreen);
            Assert.AreEqual(1, _nav.StackDepth);
            Assert.IsFalse(_nav.CanGoBack);
            Assert.IsTrue(_worldScreen.IsVisible);
        }

        [Test]
        public void Push_AddsToStackAndHidesPrevious()
        {
            _nav.NavigateTo(ScreenId.World);
            _nav.Push(ScreenId.Base);

            Assert.AreEqual(ScreenId.Base, _nav.CurrentScreen);
            Assert.AreEqual(2, _nav.StackDepth);
            Assert.IsTrue(_nav.CanGoBack);
            Assert.IsFalse(_worldScreen.IsVisible);
            Assert.IsTrue(_baseScreen.IsVisible);
        }

        [Test]
        public void GoBack_PopsTopAndRestoresPrevious()
        {
            _nav.NavigateTo(ScreenId.World);
            _nav.Push(ScreenId.Base);
            _nav.Push(ScreenId.Army);

            Assert.AreEqual(3, _nav.StackDepth);
            Assert.IsTrue(_armyScreen.IsVisible);

            bool backResult = _nav.GoBack();

            Assert.IsTrue(backResult);
            Assert.AreEqual(ScreenId.Base, _nav.CurrentScreen);
            Assert.AreEqual(2, _nav.StackDepth);
            Assert.IsFalse(_armyScreen.IsVisible);
            Assert.IsTrue(_baseScreen.IsVisible);
        }

        [Test]
        public void GoBack_AtRoot_ReturnsFalse()
        {
            _nav.NavigateTo(ScreenId.World);
            Assert.IsFalse(_nav.CanGoBack);

            bool result = _nav.GoBack();
            Assert.IsFalse(result);
            Assert.AreEqual(ScreenId.World, _nav.CurrentScreen);
        }
    }
}
