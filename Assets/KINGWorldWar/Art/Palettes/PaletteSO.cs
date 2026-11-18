using UnityEngine;

namespace KingWorldWar.Art.Palettes
{
    /// <summary>
    /// Central definition of the official KING WORLD WAR color palette.
    /// Strictly original visual identity adhering to military-modern direction.
    /// </summary>
    [CreateAssetMenu(fileName = "KWW_OfficialPalette", menuName = "KingWorldWar/Art/Palette")]
    public class PaletteSO : ScriptableObject
    {
        [Header("Primary Military Modern Palette")]
        [Tooltip("Primary dark base for architecture and UI backing (#1F2421).")]
        [SerializeField] private Color _charcoal = new Color(0.12f, 0.14f, 0.13f, 1.0f);

        [Tooltip("Secondary metallic tone for structural elements and trims (#4A5568).")]
        [SerializeField] private Color _steelGray = new Color(0.29f, 0.33f, 0.41f, 1.0f);

        [Tooltip("Dominant tactical army tone for player units and camo panels (#3E4E3D).")]
        [SerializeField] private Color _mutedMilitaryGreen = new Color(0.24f, 0.31f, 0.24f, 1.0f);

        [Tooltip("Accent tone reserved strictly for rank, VIP emphasis, and victory crowns (#D4AF37).")]
        [SerializeField] private Color _restrainedGold = new Color(0.83f, 0.69f, 0.22f, 1.0f);

        [Header("Environment Tones")]
        [Tooltip("Ground terrain surface tone (#262C26).")]
        [SerializeField] private Color _terrainDark = new Color(0.15f, 0.17f, 0.15f, 1.0f);

        public Color Charcoal => _charcoal;
        public Color SteelGray => _steelGray;
        public Color MutedMilitaryGreen => _mutedMilitaryGreen;
        public Color RestrainedGold => _restrainedGold;
        public Color TerrainDark => _terrainDark;
    }
}
