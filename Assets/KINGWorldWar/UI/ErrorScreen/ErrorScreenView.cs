using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.ErrorScreen
{
    /// <summary>
    /// Reusable view for displaying application errors, maintenance notifications, and fatal failures.
    /// </summary>
    public class ErrorScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private TMP_Text _codeText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _quitButton;

        private Action _onRetry;
        private Action _onQuit;

        public bool IsVisible => gameObject.activeSelf;
        public string DisplayedTitle => _titleText != null ? _titleText.text : string.Empty;
        public string DisplayedMessage => _messageText != null ? _messageText.text : string.Empty;
        public string DisplayedCode => _codeText != null ? _codeText.text : string.Empty;

        private void Awake()
        {
            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(HandleRetry);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuit);
            }
        }

        private void OnDestroy()
        {
            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(HandleRetry);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(HandleQuit);
            }
        }

        public void Show(string title, string message, string errorCode, Action onRetry = null, Action onQuit = null)
        {
            _onRetry = onRetry;
            _onQuit = onQuit;

            if (_titleText != null)
            {
                _titleText.text = title ?? "ERROR";
            }

            if (_messageText != null)
            {
                _messageText.text = message ?? "An unexpected error occurred.";
            }

            if (_codeText != null)
            {
                _codeText.text = string.IsNullOrWhiteSpace(errorCode) ? string.Empty : $"CODE: {errorCode}";
            }

            if (_retryButton != null)
            {
                _retryButton.gameObject.SetActive(onRetry != null);
            }

            if (_quitButton != null)
            {
                _quitButton.gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onRetry = null;
            _onQuit = null;
        }

        public void TriggerRetry()
        {
            HandleRetry();
        }

        private void HandleRetry()
        {
            _onRetry?.Invoke();
        }

        private void HandleQuit()
        {
            if (_onQuit != null)
            {
                _onQuit.Invoke();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
