using System;
using System.Collections.Generic;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;

namespace KingWorldWar.Infrastructure.UI
{
    /// <summary>
    /// Manages the screen hierarchy, stack navigation, and back navigation.
    /// </summary>
    public class ScreenNavigationService : IScreenNavigationService
    {
        private readonly ILog _log;
        private readonly Dictionary<ScreenId, IScreen> _screens = new Dictionary<ScreenId, IScreen>();
        private readonly Stack<ScreenId> _historyStack = new Stack<ScreenId>();

        public ScreenId CurrentScreen => _historyStack.Count > 0 ? _historyStack.Peek() : ScreenId.World;
        public int StackDepth => _historyStack.Count;
        public bool CanGoBack => _historyStack.Count > 1;

        public event Action<ScreenId> OnScreenChanged;

        public ScreenNavigationService(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void RegisterScreen(ScreenId screenId, IScreen screen)
        {
            if (screen == null) return;
            _screens[screenId] = screen;
            _log.Info("Navigation", $"Registered screen: {screenId}");
        }

        public void UnregisterScreen(ScreenId screenId)
        {
            if (_screens.Remove(screenId))
            {
                _log.Info("Navigation", $"Unregistered screen: {screenId}");
            }
        }

        public void NavigateTo(ScreenId screenId)
        {
            if (_historyStack.Count > 0 && _historyStack.Peek() == screenId)
            {
                return; // already on screen
            }

            // Hide currently active screen
            if (_historyStack.Count > 0 && _screens.TryGetValue(_historyStack.Peek(), out var currentScreen))
            {
                currentScreen.Hide();
            }

            // Reset stack to new root
            _historyStack.Clear();
            _historyStack.Push(screenId);

            if (_screens.TryGetValue(screenId, out var targetScreen))
            {
                targetScreen.Show();
            }

            _log.Info("Navigation", $"Navigated root to: {screenId}");
            OnScreenChanged?.Invoke(screenId);
        }

        public void Push(ScreenId screenId)
        {
            if (_historyStack.Count > 0 && _historyStack.Peek() == screenId)
            {
                return;
            }

            if (_historyStack.Count > 0 && _screens.TryGetValue(_historyStack.Peek(), out var currentScreen))
            {
                currentScreen.Hide();
            }

            _historyStack.Push(screenId);

            if (_screens.TryGetValue(screenId, out var targetScreen))
            {
                targetScreen.Show();
            }

            _log.Info("Navigation", $"Pushed screen: {screenId} (Depth: {_historyStack.Count})");
            OnScreenChanged?.Invoke(screenId);
        }

        public bool GoBack()
        {
            if (!CanGoBack)
            {
                _log.Info("Navigation", "Cannot go back: At root of navigation stack.");
                return false;
            }

            // Pop top screen
            ScreenId popped = _historyStack.Pop();
            if (_screens.TryGetValue(popped, out var poppedScreen))
            {
                poppedScreen.Hide();
            }

            // Restore previous screen
            ScreenId top = _historyStack.Peek();
            if (_screens.TryGetValue(top, out var topScreen))
            {
                topScreen.Show();
            }

            _log.Info("Navigation", $"Went back from {popped} to {top} (Depth: {_historyStack.Count})");
            OnScreenChanged?.Invoke(top);
            return true;
        }
    }
}
