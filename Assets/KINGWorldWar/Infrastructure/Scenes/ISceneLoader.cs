using System;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;
using UnityEngine.SceneManagement;

namespace KingWorldWar.Infrastructure.Scenes
{
    /// <summary>
    /// Service contract for asynchronous scene transitions with progress and cancellation.
    /// </summary>
    public interface ISceneLoader
    {
        string CurrentSceneName { get; }
        bool IsLoading { get; }

        Task<Result<GameError>> LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default);
    }
}
