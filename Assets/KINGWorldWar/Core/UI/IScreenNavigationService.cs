using System;

namespace KingWorldWar.Core.UI
{
    /// <summary>
    /// Service contract for stack-based screen navigation and back button routing.
    /// </summary>
    public interface IScreenNavigationService
    {
        ScreenId CurrentScreen { get; }
        int StackDepth { get; }
        bool CanGoBack { get; }

        event Action<ScreenId> OnScreenChanged;

        void RegisterScreen(ScreenId screenId, IScreen screen);
        void UnregisterScreen(ScreenId screenId);

        void NavigateTo(ScreenId screenId);
        void Push(ScreenId screenId);
        bool GoBack();
    }
}
