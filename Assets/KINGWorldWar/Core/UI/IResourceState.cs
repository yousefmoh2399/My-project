using System;

namespace KingWorldWar.Core.UI
{
    /// <summary>
    /// Non-authoritative client presentation contract for core game resources (Gold, Oil, Iron).
    /// </summary>
    public interface IResourceState
    {
        long Gold { get; }
        long Oil { get; }
        long Iron { get; }

        event Action OnResourcesChanged;
    }
}
