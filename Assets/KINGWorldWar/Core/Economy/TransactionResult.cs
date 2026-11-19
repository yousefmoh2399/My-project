using System;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Immutable result of a server-authoritative economic mutation transaction.
    /// </summary>
    public sealed class TransactionResult
    {
        public bool Success { get; }
        public GameError Error { get; }
        public EconomySnapshot AuthoritativeSnapshot { get; }
        public string TransactionId { get; }
        public string RequestId { get; }
        public DateTime ServerTimeUtc { get; }

        private TransactionResult(
            bool success,
            GameError error,
            EconomySnapshot authoritativeSnapshot,
            string transactionId,
            string requestId,
            DateTime serverTimeUtc)
        {
            Success = success;
            Error = error;
            AuthoritativeSnapshot = authoritativeSnapshot;
            TransactionId = transactionId ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            ServerTimeUtc = serverTimeUtc.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(serverTimeUtc, DateTimeKind.Utc)
                : serverTimeUtc.ToUniversalTime();
        }

        public static TransactionResult Ok(
            EconomySnapshot authoritativeSnapshot,
            string transactionId,
            string requestId,
            DateTime serverTimeUtc)
        {
            return new TransactionResult(
                true,
                default,
                authoritativeSnapshot,
                transactionId,
                requestId,
                serverTimeUtc);
        }

        public static TransactionResult Fail(
            GameError error,
            string requestId,
            EconomySnapshot currentSnapshot = null)
        {
            return new TransactionResult(
                false,
                error,
                currentSnapshot,
                string.Empty,
                requestId,
                DateTime.UtcNow);
        }
    }
}
