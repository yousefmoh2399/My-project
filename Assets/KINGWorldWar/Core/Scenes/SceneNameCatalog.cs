using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KingWorldWar.Core.Scenes
{
    /// <summary>
    /// Central catalog of scene names to avoid magic strings across code.
    /// </summary>
    public static class SceneNameCatalog
    {
        public const string Bootstrap = "Bootstrap";
        public const string MainMenu = "MainMenu";
        public const string Base = "Base";
        public const string WorldMap = "WorldMap";
        public const string BattleReport = "BattleReport";

        /// <summary>
        /// Scenes strictly required for the core application lifecycle.
        /// </summary>
        public static readonly ReadOnlyCollection<string> RequiredScenes = new ReadOnlyCollection<string>(
            new[] { Bootstrap, MainMenu }
        );

        /// <summary>
        /// All scenes planned in the product architecture.
        /// </summary>
        public static readonly ReadOnlyCollection<string> AllPlannedScenes = new ReadOnlyCollection<string>(
            new[] { Bootstrap, MainMenu, Base, WorldMap, BattleReport }
        );

        public static bool IsValid(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName)) return false;
            foreach (var s in AllPlannedScenes)
            {
                if (string.Equals(s, sceneName, StringComparison.Ordinal)) return true;
            }
            return false;
        }
    }
}
