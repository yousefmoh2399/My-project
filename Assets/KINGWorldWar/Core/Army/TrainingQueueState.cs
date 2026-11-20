namespace KingWorldWar.Core.Army
{
    /// <summary>
    /// Explicit lifecycle states for a training queue operation.
    /// Supports asynchronous Nakama RPC status synchronization and UI state handling.
    /// </summary>
    public enum TrainingQueueState
    {
        /// <summary>
        /// Initial client synchronization or reloading snapshot from backend.
        /// </summary>
        Loading = 0,

        /// <summary>
        /// Request dispatched, waiting for server RPC authorization or queue slot availability.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Actively training with valid server start and end timestamps.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Training duration elapsed, troops transferred to standing roster snapshot.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Training request rejected or failed on server (insufficient resources, prerequisites unmet).
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Cancelled by commander with server-authorized refund applied to player wallet.
        /// </summary>
        Cancelled = 5
    }
}
