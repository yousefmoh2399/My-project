using System;

namespace KingWorldWar.Core.Base
{
    /// <summary>
    /// Dynamic runtime state of a placed building in the player's base.
    /// 
    /// SECURITY NOTICE (Untrusted Client):
    /// All values (CurrentLevel, Upgrade timestamps, coordinates) are non-authoritative client displays.
    /// Upgrade requests, resource expenditures, and completions must be validated and granted by Nakama RPCs.
    /// </summary>
    public sealed class BuildingRuntimeState
    {
        public BuildingId Id { get; }
        public BuildingType Type { get; }
        public int CurrentLevel { get; private set; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }

        public bool IsUpgrading => UpgradeEndTimeUtc.HasValue;
        public DateTime? UpgradeStartTimeUtc { get; private set; }
        public DateTime? UpgradeEndTimeUtc { get; private set; }

        public BuildingRuntimeState(
            BuildingId id,
            BuildingType type,
            int currentLevel,
            int gridX,
            int gridY,
            DateTime? upgradeStartTimeUtc = null,
            DateTime? upgradeEndTimeUtc = null)
        {
            if (!id.IsValid)
                throw new ArgumentException("BuildingId must be valid and formatted with 'bld_'.", nameof(id));
            if (currentLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(currentLevel), "CurrentLevel must be >= 1.");

            Id = id;
            Type = type;
            CurrentLevel = currentLevel;
            GridX = gridX;
            GridY = gridY;
            UpgradeStartTimeUtc = upgradeStartTimeUtc;
            UpgradeEndTimeUtc = upgradeEndTimeUtc;
        }

        public TimeSpan GetRemainingUpgradeTime(DateTime currentUtc)
        {
            if (!UpgradeEndTimeUtc.HasValue) return TimeSpan.Zero;
            var remaining = UpgradeEndTimeUtc.Value - currentUtc;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public float GetUpgradeProgress(DateTime currentUtc)
        {
            if (!UpgradeStartTimeUtc.HasValue || !UpgradeEndTimeUtc.HasValue) return 0f;
            var total = (UpgradeEndTimeUtc.Value - UpgradeStartTimeUtc.Value).TotalSeconds;
            if (total <= 0) return 1f;

            var elapsed = (currentUtc - UpgradeStartTimeUtc.Value).TotalSeconds;
            return Math.Clamp((float)(elapsed / total), 0f, 1f);
        }

        public void StartUpgrade(DateTime startUtc, int durationSeconds)
        {
            if (durationSeconds < 0) durationSeconds = 0;
            UpgradeStartTimeUtc = startUtc;
            UpgradeEndTimeUtc = startUtc.AddSeconds(durationSeconds);
        }

        public void CancelUpgrade()
        {
            UpgradeStartTimeUtc = null;
            UpgradeEndTimeUtc = null;
        }

        public void CompleteUpgrade()
        {
            CurrentLevel++;
            UpgradeStartTimeUtc = null;
            UpgradeEndTimeUtc = null;
        }

        public void SetPosition(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
        }

        public override string ToString() =>
            $"{Id} ({Type} Lv.{CurrentLevel} at [{GridX},{GridY}])" + (IsUpgrading ? " [UPGRADING]" : "");
    }
}
