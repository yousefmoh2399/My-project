using System;
using System.Collections.Generic;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.Logging;
using UnityEngine;

namespace KingWorldWar.Infrastructure.Localization
{
    /// <summary>
    /// Production implementation of ILocalizationService.
    /// Supports dynamic language switching, fallback tables, Arabic RTL shaping, and persistent preference.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        public const string LanguagePrefKey = "kww_language_preference";

        private readonly ILog _log;
        private Language _currentLanguage;
        private readonly Dictionary<string, string> _arabicTable;
        private readonly Dictionary<string, string> _englishTable;

        public Language CurrentLanguage => _currentLanguage;
        public bool IsRightToLeft => _currentLanguage == Language.Arabic;

        public event Action<Language> OnLanguageChanged;

        public LocalizationService(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _arabicTable = new Dictionary<string, string>(DefaultStringTables.ArabicTable);
            _englishTable = new Dictionary<string, string>(DefaultStringTables.EnglishTable);
        }

        public void Initialize()
        {
            if (PlayerPrefs.HasKey(LanguagePrefKey))
            {
                int saved = PlayerPrefs.GetInt(LanguagePrefKey);
                _currentLanguage = (saved == (int)Language.English) ? Language.English : Language.Arabic;
                _log.Info("Localization", $"Loaded persisted language: {_currentLanguage}");
            }
            else
            {
                // Default based on device language
                _currentLanguage = (Application.systemLanguage == SystemLanguage.Arabic) ? Language.Arabic : Language.English;
                _log.Info("Localization", $"Detected device language default: {_currentLanguage} (System: {Application.systemLanguage})");
            }

            OnLanguageChanged?.Invoke(_currentLanguage);
        }

        public void SetLanguage(Language language, bool persist = true)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            if (persist)
            {
                PlayerPrefs.SetInt(LanguagePrefKey, (int)language);
                PlayerPrefs.Save();
                _log.Info("Localization", $"Persisted language selection: {language}");
            }

            _log.Info("Localization", $"Language switched to: {language}");
            OnLanguageChanged?.Invoke(language);
        }

        public string GetString(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            var primaryTable = (_currentLanguage == Language.Arabic) ? _arabicTable : _englishTable;
            var fallbackTable = (_currentLanguage == Language.Arabic) ? _englishTable : _arabicTable;

            string value;
            if (!primaryTable.TryGetValue(key, out value))
            {
                if (!fallbackTable.TryGetValue(key, out value))
                {
                    _log.Warning("Localization", $"Missing localization key: '{key}' for language {_currentLanguage}");
                    return $"[missing:{key}]";
                }
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    value = string.Format(value, args);
                }
                catch (FormatException ex)
                {
                    _log.Error("Localization", $"Format error for key '{key}': {ex.Message}");
                }
            }

            if (_currentLanguage == Language.Arabic)
            {
                return ArabicTextShaper.Shape(value);
            }

            return value;
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return _arabicTable.ContainsKey(key) || _englishTable.ContainsKey(key);
        }

        public string ShapeText(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return string.Empty;
            if (_currentLanguage == Language.Arabic)
            {
                return ArabicTextShaper.Shape(rawText);
            }
            return rawText;
        }

        public IReadOnlyList<string> GetAllKeys()
        {
            var keys = new HashSet<string>(_englishTable.Keys);
            keys.UnionWith(_arabicTable.Keys);
            return new List<string>(keys);
        }
    }
}
