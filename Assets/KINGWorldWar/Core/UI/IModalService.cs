using System;

namespace KingWorldWar.Core.UI
{
    public enum ModalType
    {
        Alert,
        Confirm,
        Error
    }

    public class ModalRequest
    {
        public ModalType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ConfirmText { get; set; }
        public string CancelText { get; set; }
        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }
    }

    public interface IModalView
    {
        void DisplayModal(ModalRequest request);
        void CloseModal();
    }

    /// <summary>
    /// Service contract for queueing, displaying, and dismissing modal dialogs.
    /// </summary>
    public interface IModalService
    {
        bool IsModalOpen { get; }

        event Action<bool> OnModalVisibilityChanged;

        void BindView(IModalView view);
        void ShowAlert(string title, string message, string buttonText = null, Action onConfirm = null);
        void ShowConfirm(string title, string message, string confirmText = null, string cancelText = null, Action onConfirm = null, Action onCancel = null);
        void ShowError(string title, string message, string retryText = null, Action onRetry = null, Action onDismiss = null);
        void DismissCurrent();
    }
}
