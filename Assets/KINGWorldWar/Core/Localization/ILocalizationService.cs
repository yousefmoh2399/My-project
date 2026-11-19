using System;
using System.Collections.Generic;

namespace KingWorldWar.Core.Localization
{
    /// <summary>
    /// Contract for localization string resolution, language switching, and Arabic RTL shaping.
    /// </summary>
    public interface ILocalizationService
    {
        Language CurrentLanguage { get; }
        bool IsRightToLeft { get; }

        event Action<Language> OnLanguageChanged;

        void Initialize();
        void SetLanguage(Language language, bool persist = true);
        string GetString(string key, params object[] args);
        bool HasKey(string key);
        string ShapeText(string rawText);
        IReadOnlyList<string> GetAllKeys();
    }
}
