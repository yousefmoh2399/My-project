using System;
using System.Text.RegularExpressions;

namespace KingWorldWar.Core.Logging
{
    /// <summary>
    /// Utility for sanitizing sensitive credentials, tokens, and passwords from log strings.
    /// </summary>
    public static class LogRedactor
    {
        private const string Mask = "[REDACTED]";

        private static readonly Regex[] RedactionRules = new[]
        {
            // Key-value pairs: password="...", password: "...", password = "..."
            new Regex(@"(?i)(password|passwd|pwd|secret|apikey|api_key|access_token|refresh_token|auth_token|session_token)\s*[:=]\s*[""']?([^""'\s,;]+)[""']?", RegexOptions.Compiled),
            
            // Bearer tokens: Bearer eyJ...
            new Regex(@"(?i)(bearer\s+)([a-zA-Z0-9_\-\.]{10,})", RegexOptions.Compiled),

            // Connection strings: postgres://user:pass@host...
            new Regex(@"(?i)(postgres(?:ql)?:\/\/[^:]+:)([^@]+)(@)", RegexOptions.Compiled),

            // Private keys: -----BEGIN ... PRIVATE KEY-----
            new Regex(@"-----BEGIN[ A-Z0-9_-]+PRIVATE KEY-----[^-]+-----END[ A-Z0-9_-]+PRIVATE KEY-----", RegexOptions.Compiled)
        };

        /// <summary>
        /// Scrubs sensitive values from the input message, returning a safe-to-log string.
        /// </summary>
        public static string Redact(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string sanitized = input;

            // Rule 0: Key-value credentials
            sanitized = RedactionRules[0].Replace(sanitized, m => $"{m.Groups[1].Value}={Mask}");

            // Rule 1: Bearer tokens
            sanitized = RedactionRules[1].Replace(sanitized, m => $"{m.Groups[1].Value}{Mask}");

            // Rule 2: Connection strings (mask password only)
            sanitized = RedactionRules[2].Replace(sanitized, m => $"{m.Groups[1].Value}{Mask}{m.Groups[3].Value}");

            // Rule 3: Private key blocks
            sanitized = RedactionRules[3].Replace(sanitized, Mask);

            return sanitized;
        }
    }
}
