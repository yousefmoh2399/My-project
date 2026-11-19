using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;
using KingWorldWar.Infrastructure.UI;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ModalServiceTests
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

        private class MockModalView : IModalView
        {
            public ModalRequest LastRequest { get; private set; }
            public bool IsOpen { get; private set; }

            public void DisplayModal(ModalRequest request)
            {
                LastRequest = request;
                IsOpen = true;
            }

            public void CloseModal()
            {
                LastRequest = null;
                IsOpen = false;
            }
        }

        private ModalService _service;
        private MockModalView _view;

        [SetUp]
        public void SetUp()
        {
            _service = new ModalService(new DummyLog());
            _view = new MockModalView();
            _service.BindView(_view);
        }

        [Test]
        public void ShowAlert_OpensModalAndSetsProperties()
        {
            _service.ShowAlert("Test Alert", "Alert message content");

            Assert.IsTrue(_service.IsModalOpen);
            Assert.IsTrue(_view.IsOpen);
            Assert.AreEqual("Test Alert", _view.LastRequest.Title);
            Assert.AreEqual("Alert message content", _view.LastRequest.Message);
        }

        [Test]
        public void ConfirmCurrent_ExecutesCallbackAndCloses()
        {
            bool confirmed = false;
            _service.ShowConfirm("Confirm", "Are you sure?", onConfirm: () => confirmed = true);

            _service.ConfirmCurrent();

            Assert.IsTrue(confirmed);
            Assert.IsFalse(_service.IsModalOpen);
            Assert.IsFalse(_view.IsOpen);
        }

        [Test]
        public void ModalQueue_SequentiallyPresentsDialogs()
        {
            _service.ShowAlert("First", "Message 1");
            _service.ShowAlert("Second", "Message 2");

            Assert.AreEqual("First", _view.LastRequest.Title);

            _service.DismissCurrent();

            // Next in queue should show
            Assert.IsTrue(_service.IsModalOpen);
            Assert.AreEqual("Second", _view.LastRequest.Title);

            _service.DismissCurrent();
            Assert.IsFalse(_service.IsModalOpen);
            Assert.IsFalse(_view.IsOpen);
        }
    }
}
