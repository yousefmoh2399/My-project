using System;
using System.Collections.Generic;
using KingWorldWar.Core.Logging;
using KingWorldWar.Core.UI;

namespace KingWorldWar.Infrastructure.UI
{
    /// <summary>
    /// Service managing modal dialog display, queueing, and dismissal.
    /// </summary>
    public class ModalService : IModalService
    {
        private readonly ILog _log;
        private readonly Queue<ModalRequest> _queue = new Queue<ModalRequest>();
        private ModalRequest _currentRequest;
        private IModalView _view;

        public bool IsModalOpen => _currentRequest != null;

        public event Action<bool> OnModalVisibilityChanged;

        public ModalService(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void BindView(IModalView view)
        {
            _view = view;
            if (_currentRequest != null && _view != null)
            {
                _view.DisplayModal(_currentRequest);
            }
        }

        public void ShowAlert(string title, string message, string buttonText = null, Action onConfirm = null)
        {
            Enqueue(new ModalRequest
            {
                Type = ModalType.Alert,
                Title = title,
                Message = message,
                ConfirmText = buttonText ?? "OK",
                OnConfirm = onConfirm
            });
        }

        public void ShowConfirm(string title, string message, string confirmText = null, string cancelText = null, Action onConfirm = null, Action onCancel = null)
        {
            Enqueue(new ModalRequest
            {
                Type = ModalType.Confirm,
                Title = title,
                Message = message,
                ConfirmText = confirmText ?? "Confirm",
                CancelText = cancelText ?? "Cancel",
                OnConfirm = onConfirm,
                OnCancel = onCancel
            });
        }

        public void ShowError(string title, string message, string retryText = null, Action onRetry = null, Action onDismiss = null)
        {
            Enqueue(new ModalRequest
            {
                Type = ModalType.Error,
                Title = title,
                Message = message,
                ConfirmText = retryText ?? "Retry",
                CancelText = "Dismiss",
                OnConfirm = onRetry,
                OnCancel = onDismiss
            });
        }

        private void Enqueue(ModalRequest request)
        {
            _queue.Enqueue(request);
            _log.Info("Modal", $"Enqueued modal: '{request.Title}' (Queue size: {_queue.Count})");

            if (_currentRequest == null)
            {
                ShowNext();
            }
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _currentRequest = null;
                _view?.CloseModal();
                OnModalVisibilityChanged?.Invoke(false);
                return;
            }

            _currentRequest = _queue.Dequeue();
            _view?.DisplayModal(_currentRequest);
            OnModalVisibilityChanged?.Invoke(true);
            _log.Info("Modal", $"Displaying modal: '{_currentRequest.Title}'");
        }

        public void DismissCurrent()
        {
            if (_currentRequest == null) return;

            _log.Info("Modal", $"Dismissed modal: '{_currentRequest.Title}'");
            ShowNext();
        }

        public void ConfirmCurrent()
        {
            if (_currentRequest == null) return;

            var action = _currentRequest.OnConfirm;
            DismissCurrent();
            action?.Invoke();
        }

        public void CancelCurrent()
        {
            if (_currentRequest == null) return;

            var action = _currentRequest.OnCancel;
            DismissCurrent();
            action?.Invoke();
        }
    }
}
