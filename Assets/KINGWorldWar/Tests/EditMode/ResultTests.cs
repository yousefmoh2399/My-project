using KingWorldWar.Core.Common;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void Result_Ok_HasValueAndIsSuccess()
        {
            var res = Result<int, GameError>.Ok(42);
            Assert.IsTrue(res.IsSuccess);
            Assert.IsFalse(res.IsFailure);
            Assert.AreEqual(42, res.Value);
        }

        [Test]
        public void Result_Fail_HasErrorAndIsFailure()
        {
            var err = GameError.SceneNotFound("TestScene");
            var res = Result<int, GameError>.Fail(err);

            Assert.IsFalse(res.IsSuccess);
            Assert.IsTrue(res.IsFailure);
            Assert.AreEqual("SCENE_NOT_FOUND", res.Error.Code);
        }

        [Test]
        public void Result_Match_ExecutesCorrectBranch()
        {
            var success = Result<string, GameError>.Ok("Hello");
            string successMsg = success.Match(val => $"OK: {val}", err => $"ERR: {err.Code}");
            Assert.AreEqual("OK: Hello", successMsg);

            var failure = Result<string, GameError>.Fail(GameError.NetworkUnavailable("timeout"));
            string failureMsg = failure.Match(val => $"OK: {val}", err => $"ERR: {err.Code}");
            Assert.AreEqual("ERR: NETWORK_UNAVAILABLE", failureMsg);
        }

        [Test]
        public void VoidResult_OkAndFail_BehaveAsExpected()
        {
            var ok = Result<GameError>.Ok();
            Assert.IsTrue(ok.IsSuccess);

            var fail = Result<GameError>.Fail(new GameError("ERR", "msg"));
            Assert.IsTrue(fail.IsFailure);
            Assert.AreEqual("ERR", fail.Error.Code);
        }
    }
}
