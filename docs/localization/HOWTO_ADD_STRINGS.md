# Localization Guide — How to Add and Maintain Strings

This guide explains how to add, edit, and maintain localized strings in **KING WORLD WAR** across English and Arabic (RTL).

---

## 1. Architecture Overview

KING WORLD WAR uses a lightweight, zero-dependency localization architecture:
- **`ILocalizationService`** (`Assets/KINGWorldWar/Core/Localization/ILocalizationService.cs`): Contract for string resolution, language switching, and event notification.
- **`DefaultStringTables`** (`Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs`): Central dictionaries for English (`en`) and Arabic (`ar`).
- **`ArabicTextShaper`** (`Assets/KINGWorldWar/Core/Localization/ArabicTextShaper.cs`): Pure C# contextual shaper converting standard Unicode (`U+0621`–`U+064A`) to connected Presentation Forms-B (`U+FE80`–`U+FEFC`) and maintaining LTR order for numbers and English tokens in RTL contexts.
- **`LocalizedText`** (`Assets/KINGWorldWar/UI/Common/LocalizedText.cs`): MonoBehaviour that automatically refreshes its `TextMeshProUGUI` component when the active language changes, with automatic right-to-left alignment mirroring.

---

## 2. Adding a New String

### Step 1: Define Key Naming Convention
Follow dot-notation hierarchical keys:
- `nav.<screen>`: Navigation tab titles (e.g. `nav.world`, `nav.base`).
- `placeholder.<screen>.<field>`: Shell screen placeholders (e.g. `placeholder.world.title`, `placeholder.world.desc`).
- `dialog.<name>.<field>`: Modal and dialog strings (e.g. `dialog.confirm_title`, `dialog.confirm_exit_msg`).
- `common.<action>`: Reusable actions (e.g. `common.confirm`, `common.cancel`, `common.back`).
- `resource.<name>`: Resource names (e.g. `resource.gold`, `resource.oil`, `resource.iron`).

### Step 2: Add Keys to Both English and Arabic Tables
Open `Assets/KINGWorldWar/Infrastructure/Localization/DefaultStringTables.cs`:

1. Add the English string to `EnglishTable`:
   ```csharp
   ["army.deploy"] = "Deploy Army",
   ```

2. Add the exact same key to `ArabicTable`:
   ```csharp
   ["army.deploy"] = "نشر القوات",
   ```

> [!IMPORTANT]
> **Strict Key Parity Rule:** Every key that exists in English MUST exist in Arabic, and vice-versa. If a key is missing in either table, automated tests will fail immediately.

### Step 3: Flag Arabic Military Terminology
If the added Arabic string involves military ranks, equipment, units, tactics, or commands, you **must** add an entry to [`docs/localization/REVIEW_QUEUE.md`](file:///Users/yousef/Desktop/My%20project/docs/localization/REVIEW_QUEUE.md) flagged as `Needs Human Review`.

### Step 4: Run Parity Unit Tests
Run the EditMode test runner via CLI:
```bash
unity command test run --mode editor --filter LocalizationServiceTests
```
The test `StringTables_StrictKeyParityAcrossLanguages` verifies that both tables contain the exact same keys.

---

## 3. Formatting and Parametric Strings

Use standard C# format braces `{0}`, `{1}`:
```csharp
["profile.level"] = "Level {0}",
["profile.level"] = "المستوى {0}",
```

To resolve formatted strings in code:
```csharp
string text = localizationService.GetString("profile.level", 15);
// English: "Level 15"
// Arabic:  "15 ﻯﻮﺘﺴﻤﻟﺍ" (Shaped with LTR numbers preserved)
```

Or using `LocalizedText`:
```csharp
localizedTextComponent.SetArgs(15);
```

---

## 4. Text Alignment and RTL Mirroring

The `LocalizedText` component automatically mirrors text alignment when Arabic is selected:
- **English (LTR):** `TextAlignmentOptions.MidlineLeft`
- **Arabic (RTL):** `TextAlignmentOptions.MidlineRight`

You can disable auto-alignment on specific labels (such as centered titles) by unchecking `Auto Align RTL` in the Inspector.
