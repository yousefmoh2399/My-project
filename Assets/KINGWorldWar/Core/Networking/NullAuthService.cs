using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;

namespace KingWorldWar.Core.Networking
{
    /// <summary>
    /// Offline / Null implementation of IAuthService for Phase 1 local development and testing.
    /// </summary>
    public class NullAuthService : IAuthService
    {
        public bool IsAuthenticated { get; private set; }
        public string UserId { get; private set; }
        public string SessionToken { get; private set; }

        public Task<Result<string, GameError>> AuthenticateDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
        {
            UserId = string.IsNullOrWhiteSpace(deviceId) ? "offline_guest_user" : $"guest_{deviceId}";
            SessionToken = "null_session_token";
            IsAuthenticated = true;
            return Task.FromResult(Result<string, GameError>.Ok(UserId));
        }

        public Task<Result<string, GameError>> AuthenticateEmailAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            UserId = $"user_{email}";
            SessionToken = "null_session_token";
            IsAuthenticated = true;
            return Task.FromResult(Result<string, GameError>.Ok(UserId));
        }

        public void Logout()
        {
            IsAuthenticated = false;
            UserId = null;
            SessionToken = null;
        }
    }
}
