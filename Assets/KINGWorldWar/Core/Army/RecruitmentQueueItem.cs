using System;

namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Represents an active, pending, or completed unit training queue operation.
    /// Uses UTC timestamps strictly to prevent client-side clock tampering.
    /// Classified as ServerAuthoritative: server dictates StartTime, EndTime, and State.
    /// </summary>
    [Serializable]
    public sealed class RecruitmentQueueItem
    {
        public string QueueId { get; }
        public UnitType Type { get; }
        public int Count { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public UnitCost TotalCost { get; }
        public TrainingQueueState State { get; }

        public double TotalDurationSeconds => (EndTimeUtc - StartTimeUtc).TotalSeconds;

        public RecruitmentQueueItem(
            string queueId,
            UnitType type,
            int count,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            UnitCost totalCost,
            TrainingQueueState state = TrainingQueueState.InProgress)
        {
            if (string.IsNullOrWhiteSpace(queueId)) throw new ArgumentException("QueueId cannot be null or whitespace.", nameof(queueId));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
            if (endTimeUtc < startTimeUtc) throw new ArgumentException("EndTimeUtc cannot be earlier than StartTimeUtc.");

            QueueId = queueId;
            Type = type;
            Count = count;
            StartTimeUtc = startTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(startTimeUtc, DateTimeKind.Utc)
                : startTimeUtc.ToUniversalTime();
            EndTimeUtc = endTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(endTimeUtc, DateTimeKind.Utc)
                : endTimeUtc.ToUniversalTime();
            TotalCost = totalCost;
            State = state;
        }

        public RecruitmentQueueItem WithState(TrainingQueueState newState)
        {
            return new RecruitmentQueueItem(QueueId, Type, Count, StartTimeUtc, EndTimeUtc, TotalCost, newState);
        }

        public double RemainingSeconds(DateTime utcNow)
        {
            if (State == TrainingQueueState.Completed || utcNow >= EndTimeUtc) return 0.0;
            return Math.Max(0.0, (EndTimeUtc - utcNow).TotalSeconds);
        }

        public float ProgressNormalized(DateTime utcNow)
        {
            if (State == TrainingQueueState.Completed) return 1.0f;
            if (State == TrainingQueueState.Pending || State == TrainingQueueState.Loading) return 0.0f;
            var total = TotalDurationSeconds;
            if (total <= 0.0) return 1.0f;
            if (utcNow <= StartTimeUtc) return 0.0f;
            if (utcNow >= EndTimeUtc) return 1.0f;

            var elapsed = (utcNow - StartTimeUtc).TotalSeconds;
            return Math.Clamp((float)(elapsed / total), 0.0f, 1.0f);
        }

        public float Progress(DateTime utcNow) => ProgressNormalized(utcNow);

        public bool IsCompleted(DateTime utcNow) =>
            State == TrainingQueueState.Completed || (State == TrainingQueueState.InProgress && utcNow >= EndTimeUtc);

        public override string ToString() => $"RecruitmentQueue [{QueueId}] {Type} x{Count} ({State}, Ends: {EndTimeUtc:O})";
    }
}
