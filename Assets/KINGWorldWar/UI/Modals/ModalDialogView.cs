using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Modals
{
    /// <summary>
    /// Presentation view for modal alerts, confirmations, and error dialogs.
    /// Intercepts all raycasts to prevent input leakage to underlying gameplay layers.
    /// </summary>
    public class ModalDialogView : MonoBehaviour, IModalView
    {
        [Header("Root & Backdrop")]
        [SerializeField] private GameObject _dialogRoot;
        [SerializeField] private Button _backdropButton;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        private ModalRequest _activeRequest;
        private IModalService _modalService;

        public bool IsVisible => _dialogRoot != null && _dialogRoot.activeSelf;

        public void BindService(IModalService modalService)
        {
            _modalService = modalService;
            _modalService?.BindView(this);
        }

        private void Awake()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);

            if (_backdropButton != null)
                _backdropButton.onClick.AddListener(OnBackdropClicked);

            if (_activeRequest == null)
            {
                CloseModal();
            }
        }

        public void DisplayModal(ModalRequest request)
        {
            _activeRequest = request;

            if (_titleText != null) _titleText.text = request.Title;
            if (_messageText != null) _messageText.text = request.Message;

            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(true);
                if (_confirmButtonText != null)
                    _confirmButtonText.text = request.ConfirmText ?? "OK";
            }

            if (_cancelButton != null)
            {
                bool hasCancel = !string.IsNullOrEmpty(request.CancelText);
                _cancelButton.gameObject.SetActive(hasCancel);
                if (hasCancel && _cancelButtonText != null)
                    _cancelButtonText.text = request.CancelText;
            }

            if (_dialogRoot != null)
            {
                _dialogRoot.SetActive(true);
            }
        }

        public void CloseModal()
        {
            _activeRequest = null;
            if (_dialogRoot != null)
            {
                _dialogRoot.SetActive(false);
            }
        }

        private void OnConfirmClicked()
        {
            var req = _activeRequest;
            _modalService?.DismissCurrent();
            req?.OnConfirm?.Invoke();
        }

        private void OnCancelClicked()
        {
            var req = _activeRequest;
            _modalService?.DismissCurrent();
            req?.OnCancel?.Invoke();
        }

        private void OnBackdropClicked()
        {
            // Dismiss if alert or cancelable
            if (_activeRequest != null && _activeRequest.Type == ModalType.Alert)
            {
                OnConfirmClicked();
            }
        }
    }
}
