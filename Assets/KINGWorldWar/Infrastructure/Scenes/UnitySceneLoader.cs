using System;
using System.Threading;
using System.Threading.Tasks;
using KingWorldWar.Core.Common;
using KingWorldWar.Core.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingWorldWar.Infrastructure.Scenes
{
    /// <summary>
    /// Production scene loading service with asynchronous progress tracking and validation.
    /// </summary>
    public class UnitySceneLoader : ISceneLoader
    {
        private readonly ILog _log;

        public string CurrentSceneName => SceneManager.GetActiveScene().name;
        public bool IsLoading { get; private set; }

        public UnitySceneLoader(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<Result<GameError>> LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                var err = GameError.SceneLoadFailed(sceneName ?? "null", "Scene name cannot be empty.");
                _log.Error("SceneLoader", err.Message);
                return Result<GameError>.Fail(err);
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                var err = GameError.SceneNotFound(sceneName);
                _log.Error("SceneLoader", $"Scene '{sceneName}' is not registered in Build Settings or cannot be loaded.", null);
                return Result<GameError>.Fail(err);
            }

            if (IsLoading)
            {
                var err = GameError.SceneLoadFailed(sceneName, "Another scene load operation is already in progress.");
                _log.Warning("SceneLoader", err.Message);
                return Result<GameError>.Fail(err);
            }

            IsLoading = true;
            _log.Info("SceneLoader", $"Starting load of scene '{sceneName}' (Mode: {mode})");

            try
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
                if (asyncOp == null)
                {
                    var err = GameError.SceneLoadFailed(sceneName, "SceneManager returned null operation.");
                    _log.Error("SceneLoader", err.Message);
                    return Result<GameError>.Fail(err);
                }

                while (!asyncOp.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        var err = new GameError("SCENE_LOAD_CANCELLED", $"Loading of scene '{sceneName}' was cancelled.", sceneName);
                        _log.Warning("SceneLoader", err.Message);
                        return Result<GameError>.Fail(err);
                    }

                    progress?.Report(asyncOp.progress);
                    await Task.Yield();
                }

                progress?.Report(1.0f);
                _log.Info("SceneLoader", $"Completed load of scene '{sceneName}' successfully.");
                return Result<GameError>.Ok();
            }
            catch (Exception ex)
            {
                var err = GameError.SceneLoadFailed(sceneName, ex.Message);
                _log.Error("SceneLoader", $"Unhandled exception while loading scene '{sceneName}'", ex);
                return Result<GameError>.Fail(err);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
