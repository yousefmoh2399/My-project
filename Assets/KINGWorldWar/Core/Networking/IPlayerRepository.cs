using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Networking
{
    /// <summary>
    /// Placeholder player data entity for client state.
    /// </summary>
    public class PlayerProfile
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public long Power { get; set; }
    }

    /// <summary>
    /// Placeholder repository for reading server-authoritative player state.
    /// </summary>
    public interface IPlayerRepository
    {
        Task<Result<PlayerProfile, GameError>> GetProfileAsync(CancellationToken cancellationToken = default);
    }
}
