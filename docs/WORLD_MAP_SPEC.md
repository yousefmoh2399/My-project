# WORLD MAP SPECIFICATION — KING WORLD WAR

**Status:** Phase 04 Approved Specification  
**Version:** 1.0.0  
**Author:** AI Lead Engineer & Backend Architect  

---

## 1. Executive Summary & Design Principles

The KING WORLD WAR world map represents the grand strategic layer of the game. It is designed to deliver tactical depth, clear territorial progression, and seamless cross-region conflict.

### Core Architectural Principles:
1. **100% Original Identity:** All fictionalized nations, geographical names, and lore are fully original ($0 cost, 0 copyright infringement).
2. **Data-Driven & Extensible:** Built on pure C# domain contracts (`KingWorldWar.Core.WorldMap`). New regions and territories are populated via data assets without code modifications.
3. **Graph Integrity:** Adjacency is strictly bidirectional and mathematically verified via `TerritoryGraph.ValidateSymmetry()` and `IsGraphConnected()`.
4. **Untrusted Client:** Territory ownership, garrison power, resource yields, and frontier statuses displayed by the client are strictly non-authoritative readouts. Mutations and auto-resolved battles will be executed exclusively via server-authoritative Nakama RPCs in Phase 8 and Phase 9.
5. **Universal Accessibility:** Every faction is differentiated not only by color tinting, but also by unambiguous geometric symbols (`★`, `◆`, `✖`, `▲`), ensuring full WCAG 2.1 AAA colorblind accessibility.

---

## 2. Strategic Regions (5 Fictional Regions)

| Region ID | English Name | Arabic Name (RTL) | Designated Capital | Strategic Specialty |
|---|---|---|---|---|
| `reg_north` | Northern Tundra | الجبهة الشمالية | `ter_nrd_001` | Heavy Iron extraction, defensive choke points |
| `reg_coast` | Coastline Dominion | ساحل الهيمنة | `ter_cst_001` | High Gold trade yields, maritime supply lines |
| `reg_oasis` | Emerald Oasis | واحة الزمرد | `ter_oas_001` | Abundant Oil reserves, vital motor fuel hub |
| `reg_valley` | Iron Valley | وادي الحديد | `ter_val_001` | Balanced industrial yields, central war theater |
| `reg_ridge` | Obsidian Ridge | هضبة الأوبسيديان | `ter_rdg_001` | Natural mountain redoubt, formidable fortresses |

---

## 3. Complete Territory Matrix (25 Territories)

### 3.1. Northern Tundra (`reg_north`)
| Territory ID | Type | English Name | Arabic Name | Base Yield (Gold/Oil/Iron) | World Pos (X, Z) | Connected Neighbors |
|---|---|---|---|---|---|---|
| `ter_nrd_001` | Capital | Frostpeak Citadel | قلعة قمة الجليد | 100 / 50 / 150 | (-20, 25) | `ter_nrd_002`, `ter_nrd_003` |
| `ter_nrd_002` | ResourceHub | Glacial Mine 01 | منجم جليدي 01 | 40 / 30 / 160 | (-30, 20) | `ter_nrd_001`, `ter_nrd_004` |
| `ter_nrd_003` | Fortress | Blizzard Fort | حصن العاصفة | 60 / 40 / 120 | (-15, 20) | `ter_nrd_001`, `ter_nrd_005` |
| `ter_nrd_004` | Standard | Permafrost Pass | ممر الجليد الدائم | 50 / 40 / 80 | (-25, 10) | `ter_nrd_002`, `ter_nrd_005`, `ter_cst_001` (Frontier) |
| `ter_nrd_005` | Frontier | Northern Redoubt | المعقل الشمالي | 70 / 50 / 90 | (-15, 10) | `ter_nrd_003`, `ter_nrd_004`, `ter_val_001` (Frontier) |

### 3.2. Coastline Dominion (`reg_coast`)
| Territory ID | Type | English Name | Arabic Name | Base Yield (Gold/Oil/Iron) | World Pos (X, Z) | Connected Neighbors |
|---|---|---|---|---|---|---|
| `ter_cst_001` | Capital | Harbor Prime | الميناء الرئيسي | 180 / 60 / 50 | (-35, 0) | `ter_cst_002`, `ter_cst_003`, `ter_nrd_004` (Frontier) |
| `ter_cst_002` | ResourceHub | Deepwater Oil Rig | منصة نفط المياه العميقة | 70 / 170 / 40 | (-45, -5) | `ter_cst_001`, `ter_cst_004` |
| `ter_cst_003` | Fortress | Cape Sentinel | حامية رأس الرجاء | 90 / 50 / 90 | (-30, -10) | `ter_cst_001`, `ter_cst_005` |
| `ter_cst_004` | Standard | Siren Anchorage | مرسى الحورية | 110 / 60 / 60 | (-40, -20) | `ter_cst_002`, `ter_cst_005` |
| `ter_cst_005` | Frontier | Seagull Peninsula | شبه جزيرة النورس | 100 / 60 / 70 | (-25, -20) | `ter_cst_003`, `ter_cst_004`, `ter_oas_001` (Frontier) |

### 3.3. Emerald Oasis (`reg_oasis`)
| Territory ID | Type | English Name | Arabic Name | Base Yield (Gold/Oil/Iron) | World Pos (X, Z) | Connected Neighbors |
|---|---|---|---|---|---|---|
| `ter_oas_001` | Capital | Palm Oasis | واحة النخيل | 120 / 160 / 40 | (-10, -35) | `ter_oas_002`, `ter_oas_003`, `ter_cst_005` (Frontier) |
| `ter_oas_002` | ResourceHub | Dunes Wellfield | حقل آبار الكثبان | 50 / 200 / 30 | (-20, -40) | `ter_oas_001`, `ter_oas_004` |
| `ter_oas_003` | Fortress | Sunken Mirage Fort | حصن السراب | 80 / 90 / 80 | (0, -30) | `ter_oas_001`, `ter_oas_005` |
| `ter_oas_004` | Standard | Golden Sands Post | نقطة الرمال الذهبية | 70 / 120 / 40 | (-10, -50) | `ter_oas_002`, `ter_oas_005` |
| `ter_oas_005` | Frontier | Oasis Verge | حافة الواحة | 80 / 110 / 50 | (5, -45) | `ter_oas_003`, `ter_oas_004`, `ter_rdg_005` (Frontier) |

### 3.4. Iron Valley (`reg_valley`)
| Territory ID | Type | English Name | Arabic Name | Base Yield (Gold/Oil/Iron) | World Pos (X, Z) | Connected Neighbors |
|---|---|---|---|---|---|---|
| `ter_val_001` | Capital | Iron Bastion | حصن الحديد | 100 / 70 / 140 | (0, 0) | `ter_val_002`, `ter_val_003`, `ter_nrd_005` (Frontier) |
| `ter_val_002` | ResourceHub | Smelting Works 04 | مصاهر الوادي 04 | 60 / 50 / 170 | (10, 10) | `ter_val_001`, `ter_val_004` |
| `ter_val_003` | Fortress | Red Gorge Watch | برج مراقبة الخانق الأحمر | 80 / 60 / 110 | (5, -10) | `ter_val_001`, `ter_val_005` |
| `ter_val_004` | Standard | Quarry Heights | مرتفعات المقالع | 70 / 60 / 120 | (20, 5) | `ter_val_002`, `ter_val_005`, `ter_rdg_001` (Frontier) |
| `ter_val_005` | Frontier | Valley Gateway | بوابة الوادي | 90 / 70 / 100 | (15, -15) | `ter_val_003`, `ter_val_004`, `ter_rdg_003` (Frontier) |

### 3.5. Obsidian Ridge (`reg_ridge`)
| Territory ID | Type | English Name | Arabic Name | Base Yield (Gold/Oil/Iron) | World Pos (X, Z) | Connected Neighbors |
|---|---|---|---|---|---|---|
| `ter_rdg_001` | Capital | Ridge Spire | برج الهضبة | 120 / 80 / 120 | (30, 20) | `ter_rdg_002`, `ter_rdg_003`, `ter_val_004` (Frontier) |
| `ter_rdg_002` | ResourceHub | Obsidian Quarry | مقلع الأوبسيديان | 80 / 40 / 160 | (40, 25) | `ter_rdg_001`, `ter_rdg_004` |
| `ter_rdg_003` | Fortress | Bastion of Crags | معقل الصخور | 90 / 60 / 110 | (35, 5) | `ter_rdg_001`, `ter_rdg_005`, `ter_val_005` (Frontier) |
| `ter_rdg_004` | Standard | High Peak Outpost | مخفر القمة العالية | 70 / 50 / 100 | (45, 10) | `ter_rdg_002`, `ter_rdg_005` |
| `ter_rdg_005` | Frontier | Dragon's Teeth Border | حدود أنياب التنين | 90 / 60 / 90 | (35, -25) | `ter_rdg_003`, `ter_rdg_004`, `ter_oas_005` (Frontier) |

---

## 4. Accessibility & Faction Symbols

To comply with mobile accessibility standards (color blindness: protanopia, deuteranopia, tritanopia, and monochromatic), all territorial banners, UI drawer badges, and map markers display dual cues (color tint + geometry symbol):

| Faction | Color Tint | Accessibility Symbol | Meaning |
|---|---|---|---|
| `Neutral` | Light Gray (`#CCCCCC`) | `◆` (Diamond) | Unclaimed territory / garrisoned by local militia |
| `PlayerFaction` | Gold / Green (`#4CAF50`) | `★` (Five-pointed Star) | Controlled by player's army / friendly forces |
| `EnemyAlliance` | Crimson / Red (`#F44336`) | `✖` (Cross / X) | Hostile territory controlled by enemy syndicate |
| `RebelOutpost` | Muted Orange (`#FF9800`) | `▲` (Triangle) | Rogue rebel stronghold with high bonus yield |

---

## 5. UI Drawer & Interaction Specification

When a player taps a territory tile on the 3D map:
1. **Camera Focus:** The `StrategyCameraRig` smoothly pans toward the territory's world-space coordinates with spherical easing.
2. **Detail Drawer (`TerritoryDetailDrawer`):**
   - Animates in from the screen edge.
   - Displays territory name, region badge, and owning faction symbol.
   - Displays estimated hourly yields for Gold, Oil, and Iron.
   - Displays non-authoritative garrison defense power rating.
   - Lists all adjacent frontier territories with quick-navigation capability.
   - Provides **Attack** and **Scout** action buttons, disabled with a clear tactical toast/disclaimer: `"Tactical operations unlock in Phase 9 (Battle Engine)"`.
   - Toggleable **Legend Panel** explaining colorblind accessibility symbols.
