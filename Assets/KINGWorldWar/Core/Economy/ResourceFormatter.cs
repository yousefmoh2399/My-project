using System;
using System.Globalization;

namespace KingWorldWar.Core.Economy
{
    /// <summary>
    /// Visual state classification for resource display.
    /// </summary>
    public enum ResourceVisualState
    {
        Normal = 0,
        Zero = 1,
        NearCap = 2,
        Capped = 3,
        Stale = 4
    }

    /// <summary>
    /// Culture-aware, overflow-safe military resource number formatting utility.
    /// Uses Western digits (0-9) as standard for modern military strategy UI precision.
    /// Uses floor rounding to ensure player never sees more than authoritative balance.
    /// </summary>
    public static class ResourceFormatter
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Formats an arbitrary resource amount into a tactical display string.
        /// Rejects negatives by clamping to 0. Safely handles long.MaxValue without overflow.
        /// </summary>
        public static string Format(long amount)
        {
            if (amount <= 0) return "0";

            if (amount < 1_000)
            {
                return amount.ToString("D", InvariantCulture);
            }

            if (amount < 1_000_000)
            {
                // Floor to 2 decimal places: e.g. 1259 -> 1.25K
                double val = Math.Floor((amount / 1_000.0) * 100.0) / 100.0;
                return val.ToString("0.##", InvariantCulture) + "K";
            }

            if (amount < 1_000_000_000)
            {
                // Floor to 2 decimal places: e.g. 3400000 -> 3.4M
                double val = Math.Floor((amount / 1_000_000.0) * 100.0) / 100.0;
                return val.ToString("0.##", InvariantCulture) + "M";
            }

            // Billions / max long
            double bVal = Math.Floor((amount / 1_000_000_000.0) * 100.0) / 100.0;
            return bVal.ToString("0.##", InvariantCulture) + "B";
        }

        /// <summary>
        /// Formats exact balance with standard thousands separator (e.g. 1,250,500).
        /// </summary>
        public static string FormatExact(long amount)
        {
            if (amount <= 0) return "0";
            return amount.ToString("N0", InvariantCulture);
        }

        /// <summary>
        /// Evaluates the visual alert status of a resource based on current balance vs storage cap.
        /// </summary>
        public static ResourceVisualState EvaluateVisualState(long amount, long capacity, bool isStale = false)
        {
            if (isStale) return ResourceVisualState.Stale;
            if (amount <= 0) return ResourceVisualState.Zero;
            if (capacity > 0 && amount >= capacity) return ResourceVisualState.Capped;
            if (capacity > 0 && (double)amount / capacity >= 0.90) return ResourceVisualState.NearCap;
            return ResourceVisualState.Normal;
        }

        /// <summary>
        /// Returns human-readable time-to-full string (e.g. "2h 45m" or "Full").
        /// </summary>
        public static string FormatTimeToFull(long amount, long capacity, long ratePerHour)
        {
            if (capacity <= 0 || amount >= capacity) return "Full";
            if (ratePerHour <= 0) return "—";

            long needed = capacity - amount;
            double hours = (double)needed / ratePerHour;
            if (hours > 720) return "> 30d"; // Capped sanity check

            TimeSpan span = TimeSpan.FromHours(hours);
            if (span.TotalDays >= 1)
            {
                return $"{(int)span.TotalDays}d {span.Hours}h";
            }
            if (span.TotalHours >= 1)
            {
                return $"{span.Hours}h {span.Minutes}m";
            }
            return $"{Math.Max(1, span.Minutes)}m";
        }
    }
}
