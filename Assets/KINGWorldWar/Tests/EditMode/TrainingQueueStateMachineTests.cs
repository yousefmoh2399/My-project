using System;
using KingWorldWar.Core.Army;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class TrainingQueueStateMachineTests
    {
        [Test]
        public void QueueItem_DefaultState_IsInProgress()
        {
            var now = DateTime.UtcNow;
            var item = new RecruitmentQueueItem(
                "q_test_01",
                UnitType.Tank,
                5,
                now,
                now.AddSeconds(120),
                new UnitCost(100, 50, 50, 120));

            Assert.That(item.State, Is.EqualTo(TrainingQueueState.InProgress));
        }

        [Test]
        public void QueueItem_WithState_CreatesNewInstanceWithExpectedState()
        {
            var now = DateTime.UtcNow;
            var item = new RecruitmentQueueItem(
                "q_test_01",
                UnitType.Tank,
                5,
                now,
                now.AddSeconds(120),
                new UnitCost(100, 50, 50, 120),
                TrainingQueueState.Pending);

            Assert.That(item.State, Is.EqualTo(TrainingQueueState.Pending));

            var inProgress = item.WithState(TrainingQueueState.InProgress);
            Assert.That(inProgress.State, Is.EqualTo(TrainingQueueState.InProgress));
            Assert.That(inProgress.QueueId, Is.EqualTo(item.QueueId));
            Assert.That(inProgress.Count, Is.EqualTo(item.Count));
            Assert.That(inProgress.StartTimeUtc, Is.EqualTo(item.StartTimeUtc));
            Assert.That(inProgress.EndTimeUtc, Is.EqualTo(item.EndTimeUtc));

            var completed = inProgress.WithState(TrainingQueueState.Completed);
            Assert.That(completed.State, Is.EqualTo(TrainingQueueState.Completed));

            var failed = item.WithState(TrainingQueueState.Failed);
            Assert.That(failed.State, Is.EqualTo(TrainingQueueState.Failed));

            var cancelled = item.WithState(TrainingQueueState.Cancelled);
            Assert.That(cancelled.State, Is.EqualTo(TrainingQueueState.Cancelled));
        }

        [Test]
        public void QueueItem_CountdownAndProgress_AdheresToUtcTimestamps()
        {
            var start = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
            var end = start.AddSeconds(100);

            var item = new RecruitmentQueueItem(
                "q_test_02",
                UnitType.ArmoredVehicle,
                2,
                start,
                end,
                new UnitCost(200, 100, 100, 100),
                TrainingQueueState.InProgress);

            // Halfway
            var mid = start.AddSeconds(50);
            Assert.That(item.Progress(mid), Is.EqualTo(0.5f));
            Assert.That(item.RemainingSeconds(mid), Is.EqualTo(50.0));
            Assert.That(item.IsCompleted(mid), Is.False);

            // Completed
            Assert.That(item.Progress(end), Is.EqualTo(1.0f));
            Assert.That(item.RemainingSeconds(end), Is.EqualTo(0.0));
            Assert.That(item.IsCompleted(end), Is.True);
        }
    }
}
