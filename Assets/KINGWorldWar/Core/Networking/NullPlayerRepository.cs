using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Networking
{
    /// <summary>
    /// Offline / Null implementation of IPlayerRepository for Phase 1 local development and testing.
    /// </summary>
    public class NullPlayerRepository : IPlayerRepository
    {
        public Task<Result<PlayerProfile, GameError>> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            var profile = new PlayerProfile
            {
                Id = "player_offline_01",
                Username = "Commander",
                Level = 1,
                Power = 1000
            };

            return Task.FromResult(Result<PlayerProfile, GameError>.Ok(profile));
        }
    }
}
