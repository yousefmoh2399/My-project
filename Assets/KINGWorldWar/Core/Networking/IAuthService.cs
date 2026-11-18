using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Networking
{
    /// <summary>
    /// Placeholder authentication service contract (to be implemented with Nakama in Phase 2).
    /// </summary>
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        string UserId { get; }
        string SessionToken { get; }

        Task<Result<string, GameError>> AuthenticateDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
        Task<Result<string, GameError>> AuthenticateEmailAsync(string email, string password, CancellationToken cancellationToken = default);
        void Logout();
    }
}
