namespace KingWorldWar.Core.UI
{
    /// <summary>
    /// Contract for a managed UI view screen with lifecycle callbacks.
    /// </summary>
    public interface IScreen
    {
        ScreenId ScreenId { get; }
        bool IsVisible { get; }

        void Show();
        void Hide();
    }
}
