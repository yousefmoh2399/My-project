using System;
using System.Collections.Generic;
using System.Text;

namespace KingWorldWar.Core.Localization
{
    /// <summary>
    /// Pure C# bidirectional Arabic text shaper and ligature processor.
    /// Converts standard Unicode Arabic into contextual Presentation Forms with correct BiDi runs.
    /// Zero external packages, zero platform-specific dependencies.
    /// </summary>
    public static class ArabicTextShaper
    {
        private struct GlyphForm
        {
            public char Isolated;
            public char Initial;
            public char Medial;
            public char Final;

            public GlyphForm(char isolated, char initial, char medial, char final)
            {
                Isolated = isolated;
                Initial = initial;
                Medial = medial;
                Final = final;
            }
        }

        private static readonly Dictionary<char, GlyphForm> GlyphMap = new Dictionary<char, GlyphForm>
        {
            { '\u0621', new GlyphForm('\uFE80', '\uFE80', '\uFE80', '\uFE80') }, // Hamza
            { '\u0622', new GlyphForm('\uFE81', '\uFE81', '\uFE82', '\uFE82') }, // Alef Madda
            { '\u0623', new GlyphForm('\uFE83', '\uFE83', '\uFE84', '\uFE84') }, // Alef Hamza Above
            { '\u0624', new GlyphForm('\uFE85', '\uFE85', '\uFE86', '\uFE86') }, // Waw Hamza
            { '\u0625', new GlyphForm('\uFE87', '\uFE87', '\uFE88', '\uFE88') }, // Alef Hamza Below
            { '\u0626', new GlyphForm('\uFE89', '\uFE8B', '\uFE8C', '\uFE8A') }, // Yeh Hamza
            { '\u0627', new GlyphForm('\uFE8D', '\uFE8D', '\uFE8E', '\uFE8E') }, // Alef
            { '\u0628', new GlyphForm('\uFE8F', '\uFE91', '\uFE92', '\uFE90') }, // Beh
            { '\u0629', new GlyphForm('\uFE93', '\uFE93', '\uFE94', '\uFE94') }, // Teh Marbuta
            { '\u062A', new GlyphForm('\uFE95', '\uFE97', '\uFE98', '\uFE96') }, // Teh
            { '\u062B', new GlyphForm('\uFE99', '\uFE9B', '\uFE9C', '\uFE9A') }, // Theh
            { '\u062C', new GlyphForm('\uFE9D', '\uFE9F', '\uFEA0', '\uFE9E') }, // Jeem
            { '\u062D', new GlyphForm('\uFEA1', '\uFEA3', '\uFEA4', '\uFEA2') }, // Hah
            { '\u062E', new GlyphForm('\uFEA5', '\uFEA7', '\uFEA8', '\uFEA6') }, // Khah
            { '\u062F', new GlyphForm('\uFEA9', '\uFEA9', '\uFEAA', '\uFEAA') }, // Dal
            { '\u0630', new GlyphForm('\uFEAB', '\uFEAB', '\uFEAC', '\uFEAC') }, // Thal
            { '\u0631', new GlyphForm('\uFEAD', '\uFEAD', '\uFEAE', '\uFEAE') }, // Reh
            { '\u0632', new GlyphForm('\uFEAF', '\uFEAF', '\uFEB0', '\uFEB0') }, // Zain
            { '\u0633', new GlyphForm('\uFEB1', '\uFEB3', '\uFEB4', '\uFEB2') }, // Seen
            { '\u0634', new GlyphForm('\uFEB5', '\uFEB7', '\uFEB8', '\uFEB6') }, // Sheen
            { '\u0635', new GlyphForm('\uFEB9', '\uFEBB', '\uFEBC', '\uFEBA') }, // Sad
            { '\u0636', new GlyphForm('\uFEBD', '\uFEBF', '\uFEC0', '\uFEBE') }, // Dad
            { '\u0637', new GlyphForm('\uFEC1', '\uFEC3', '\uFEC4', '\uFEC2') }, // Tah
            { '\u0638', new GlyphForm('\uFEC5', '\uFEC7', '\uFEC8', '\uFEC6') }, // Zah
            { '\u0639', new GlyphForm('\uFEC9', '\uFECB', '\uFECC', '\uFECA') }, // Ain
            { '\u063A', new GlyphForm('\uFECD', '\uFECF', '\uFED0', '\uFECE') }, // Ghain
            { '\u0641', new GlyphForm('\uFED1', '\uFED3', '\uFED4', '\uFED2') }, // Feh
            { '\u0642', new GlyphForm('\uFED5', '\uFED7', '\uFED8', '\uFED6') }, // Qaf
            { '\u0643', new GlyphForm('\uFED9', '\uFEDB', '\uFEDC', '\uFEDA') }, // Kaf
            { '\u0644', new GlyphForm('\uFEDD', '\uFEDF', '\uFEE0', '\uFEDE') }, // Lam
            { '\u0645', new GlyphForm('\uFEE1', '\uFEE3', '\uFEE4', '\uFEE2') }, // Meem
            { '\u0646', new GlyphForm('\uFEE5', '\uFEE7', '\uFEE8', '\uFEE6') }, // Noon
            { '\u0647', new GlyphForm('\uFEE9', '\uFEEB', '\uFEEC', '\uFEEA') }, // Heh
            { '\u0648', new GlyphForm('\uFEED', '\uFEED', '\uFEEE', '\uFEEE') }, // Waw
            { '\u0649', new GlyphForm('\uFEEF', '\uFEEF', '\uFEF0', '\uFEF0') }, // Alef Maksura
            { '\u064A', new GlyphForm('\uFEF1', '\uFEF3', '\uFEF4', '\uFEF2') }  // Yeh
        };

        private static readonly HashSet<char> NonConnectingAfter = new HashSet<char>
        {
            '\u0621', '\u0622', '\u0623', '\u0624', '\u0625', '\u0627',
            '\u062F', '\u0630', '\u0631', '\u0632', '\u0648', '\u0649'
        };

        public static bool ContainsArabic(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= '\u0600' && c <= '\u06FF') return true;
                if (c >= '\uFB50' && c <= '\uFDFF') return true;
                if (c >= '\uFE70' && c <= '\uFEFC') return true;
            }
            return false;
        }

        /// <summary>
        /// Shapes an Arabic text string into connected glyph presentation forms
        /// and applies BiDi run reordering for LTR rendering engines (e.g. TextMeshPro).
        /// </summary>
        public static string Shape(string input)
        {
            if (string.IsNullOrEmpty(input) || !ContainsArabic(input))
            {
                return input;
            }

            // 1. Process Lam-Alif ligatures
            string withLigatures = ProcessLamAlif(input);

            // 2. Shape contextual characters (Isolated, Initial, Medial, Final)
            char[] shaped = new char[withLigatures.Length];
            int len = withLigatures.Length;

            for (int i = 0; i < len; i++)
            {
                char current = withLigatures[i];

                if (!GlyphMap.TryGetValue(current, out var forms))
                {
                    shaped[i] = current;
                    continue;
                }

                bool canConnectBefore = (i > 0) &&
                                        GlyphMap.ContainsKey(withLigatures[i - 1]) &&
                                        !NonConnectingAfter.Contains(withLigatures[i - 1]);

                bool canConnectAfter = (i < len - 1) &&
                                       GlyphMap.ContainsKey(withLigatures[i + 1]) &&
                                       !NonConnectingAfter.Contains(current);

                if (canConnectBefore && canConnectAfter)
                {
                    shaped[i] = forms.Medial;
                }
                else if (canConnectBefore)
                {
                    shaped[i] = forms.Final;
                }
                else if (canConnectAfter)
                {
                    shaped[i] = forms.Initial;
                }
                else
                {
                    shaped[i] = forms.Isolated;
                }
            }

            // 3. Reorder text into visual BiDi runs for TextMeshPro
            return ReorderBiDiRuns(new string(shaped));
        }

        private static string ProcessLamAlif(string text)
        {
            var sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\u0644' && i + 1 < text.Length)
                {
                    char next = text[i + 1];
                    char lamAlif = '\0';

                    switch (next)
                    {
                        case '\u0622': lamAlif = '\uFEF5'; break; // Madda
                        case '\u0623': lamAlif = '\uFEF7'; break; // Hamza above
                        case '\u0625': lamAlif = '\uFEF9'; break; // Hamza below
                        case '\u0627': lamAlif = '\uFEFB'; break; // Plain Alef
                    }

                    if (lamAlif != '\0')
                    {
                        // Check if preceded by a connecting letter
                        bool connectBefore = (i > 0) && GlyphMap.ContainsKey(text[i - 1]) && !NonConnectingAfter.Contains(text[i - 1]);
                        sb.Append(connectBefore ? (char)(lamAlif + 1) : lamAlif);
                        i++; // skip next alef
                        continue;
                    }
                }

                sb.Append(text[i]);
            }
            return sb.ToString();
        }

        private static string ReorderBiDiRuns(string text)
        {
            // Segment string into Arabic runs and LTR runs (Latin, digits, punctuation)
            var segments = new List<TextSegment>();
            var currentRun = new StringBuilder();
            bool isArabicRun = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isArabicChar = (c >= '\u0600' && c <= '\u06FF') ||
                                    (c >= '\uFB50' && c <= '\uFDFF') ||
                                    (c >= '\uFE70' && c <= '\uFEFC');

                if (i == 0)
                {
                    isArabicRun = isArabicChar;
                    currentRun.Append(c);
                }
                else
                {
                    // Spaces and brackets attach to current run unless followed by different script
                    if (isArabicChar == isArabicRun)
                    {
                        currentRun.Append(c);
                    }
                    else
                    {
                        segments.Add(new TextSegment(currentRun.ToString(), isArabicRun));
                        currentRun.Clear();
                        currentRun.Append(c);
                        isArabicRun = isArabicChar;
                    }
                }
            }

            if (currentRun.Length > 0)
            {
                segments.Add(new TextSegment(currentRun.ToString(), isArabicRun));
            }

            // For full Arabic paragraphs/strings, reverse Arabic words while preserving LTR words/numbers
            var result = new StringBuilder(text.Length);

            // Reverse the order of segments from right to left
            for (int s = segments.Count - 1; s >= 0; s--)
            {
                var seg = segments[s];
                if (seg.IsArabic)
                {
                    // Reverse the characters of the Arabic segment because it is read RTL
                    for (int c = seg.Text.Length - 1; c >= 0; c--)
                    {
                        result.Append(MirrorBracket(seg.Text[c]));
                    }
                }
                else
                {
                    // LTR segment: numbers, Latin words (keep internal order intact!)
                    result.Append(seg.Text);
                }
            }

            return result.ToString();
        }

        private static char MirrorBracket(char c)
        {
            switch (c)
            {
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
                case '{': return '}';
                case '}': return '{';
                case '<': return '>';
                case '>': return '<';
                default: return c;
            }
        }

        private struct TextSegment
        {
            public string Text;
            public bool IsArabic;

            public TextSegment(string text, bool isArabic)
            {
                Text = text;
                IsArabic = isArabic;
            }
        }
    }
}
