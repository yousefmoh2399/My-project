using System;

namespace KingWorldWar.Core.Localization
{
    /// <summary>
    /// Type-safe container for a localizable key and dynamic format parameters.
    /// </summary>
    [Serializable]
    public readonly struct LocalizedString : IEquatable<LocalizedString>
    {
        public string Key { get; }
        public object[] Args { get; }

        public LocalizedString(string key, params object[] args)
        {
            Key = key ?? string.Empty;
            Args = args ?? Array.Empty<object>();
        }

        public string Resolve(ILocalizationService service)
        {
            if (service == null || string.IsNullOrEmpty(Key)) return Key;
            return service.GetString(Key, Args);
        }

        public bool Equals(LocalizedString other) => string.Equals(Key, other.Key, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is LocalizedString other && Equals(other);
        public override int GetHashCode() => Key != null ? Key.GetHashCode() : 0;
        public override string ToString() => Key;
    }
}
