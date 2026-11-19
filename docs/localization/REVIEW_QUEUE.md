# Arabic Localization Review Queue — Military Terminology

> **Policy:** All Arabic military terms, operational concepts, unit classifications, and command directives generated or drafted by AI must be logged here and flagged as **`Needs Human Review`** prior to production release.

---

## Review Queue Table

| Key | English Source | Arabic Draft | Category | Status | Reviewer Notes |
|---|---|---|---|---|---|
| `nav.world` | World | خريطة العالم | Navigation / Strategy | `Needs Human Review` | Could also be "العالم" or "المسرح العملياتي" depending on UI tone. |
| `nav.base` | Base | القاعدة العسكرية | Navigation / Base Building | `Needs Human Review` | Standard military term for military base / installation. |
| `nav.army` | Army | القوات المسلحة | Navigation / Military Units | `Needs Human Review` | "الجيش" vs "القوات المسلحة". Currently using "القوات المسلحة" for broader scope (ground + air). |
| `nav.alliance` | Alliance | التحالف العسكري | Navigation / Social | `Needs Human Review` | "التحالف" vs "التحالف العسكري". |
| `nav.rankings` | Rankings | لوحة الصدارة | Navigation / Social | `Needs Human Review` | Standard Arabic gaming localization for leaderboards/rankings. |
| `nav.profile` | Profile | الملف الشخصي | Navigation / Profile | `Needs Human Review` | Acceptable standard terminology for commander profile. |
| `placeholder.world.title` | World Theater | مسرح العمليات الدولي | Map / Operations | `Needs Human Review` | "مسرح العمليات" is formal doctrine for Theater of Operations. |
| `placeholder.world.desc` | Strategic world map and territory operations will unlock in Phase 4. | ستتاح خريطة العمليات الإستراتيجية والسيطرة على المناطق في المرحلة 4. | Map / Operations | `Needs Human Review` | Review military doctrine wording for territorial control ("السيطرة على المناطق"). |
| `placeholder.base.title` | Headquarters Base | القاعدة العسكرية الرئيسية | Base Building | `Needs Human Review` | Headquarters Base -> "قاعدة القيادة العامة" or "القاعدة العسكرية الرئيسية". |
| `placeholder.base.desc` | Base building, supply lines, and resource facilities will unlock in Phase 5. | سيتاح بناء المرافق والمنشآت وتأمين خطوط الإمداد في المرحلة 5. | Base Building / Logistics | `Needs Human Review` | "خطوط الإمداد" is accurate logistics doctrine for supply lines. |
| `placeholder.army.title` | Armed Forces & Garrisons | القوات المسلحة والحاميات | Military Units | `Needs Human Review` | "الحاميات" is classical/standard military term for garrisons. |
| `placeholder.army.desc` | Unit recruitment, barracks management, and tactical loadouts will unlock in Phase 6. | ستتاح تعبئة القوات وإدارة الثكنات والعتاد التكتيكي في المرحلة 6. | Recruitment / Tactics | `Needs Human Review` | "تعبئة القوات" (mobilization/recruitment), "الثكنات" (barracks), "العتاد التكتيكي" (tactical loadout). |
| `placeholder.alliance.title` | Military Coalition | التحالفات والائتلافات العسكرية | Alliance / Diplomacy | `Needs Human Review` | Check coalition nomenclature against modern Arabic defense conventions. |
| `placeholder.alliance.desc` | Joint military alliances, pacts, and territory wars will unlock in Phase 7. | ستتاح المعاهدات العسكرية والعمليات المشتركة والحروب الإقليمية في المرحلة 7. | Diplomacy / Warfare | `Needs Human Review` | "العمليات المشتركة" (joint operations), "الحروب الإقليمية" (territory/regional wars). |
| `placeholder.rankings.title` | High Command Leaderboard | لوحة الشرف والقيادة العامة | Rankings / Hierarchy | `Needs Human Review` | "القيادة العامة" represents High Command. |
| `placeholder.rankings.desc` | Global commander rankings and military power standings will unlock in Phase 8. | ستتاح تصنيفات القادة والقوة القتالية العسكرية الشاملة في المرحلة 8. | Rankings / Power Index | `Needs Human Review` | "القوة القتالية" is formal military index for combat power. |
| `placeholder.profile.title` | Commander Dossier | السجل العسكري للقائد | Commander Profile | `Needs Human Review` | "السجل العسكري" (military record/dossier). |
| `placeholder.profile.desc` | Commander progression, medals, and operational records will unlock in Phase 9. | ستتاح الرتب العسكرية والأوسمة والسجل العملياتي في المرحلة 9. | Progression / Honors | `Needs Human Review` | "الرتب العسكرية" (military ranks), "الأوسمة" (decorations/medals). |
| `dialog.confirm_title` | Confirm Action | تأكيد الأمر | Dialog / Orders | `Needs Human Review` | "تأكيد الأمر" (confirm order) gives appropriate military flavor. |
| `dialog.confirm_exit_msg` | Are you sure you want to exit King World War? | هل أنت متأكد من مغادرة مسرح العمليات؟ | Dialog / System | `Needs Human Review` | "مغادرة مسرح العمليات" gives immersive tactical flavor. |

---

## Review Guidelines for Native Arabic Military Editor

When reviewing terms in this queue:
1. **Tone:** Modern tactical military terminology (used in modern Arab armed forces and military journalism), avoiding archaic medieval language while preserving authority and gravitas.
2. **Gender Neutrality & Command Style:** Use imperative verb forms for buttons (`تأكيد`, `إلغاء`, `عودة`) and formal operational language for descriptions.
3. **Numbers & Formatting:** Confirm that BiDi number runs render left-to-right (e.g. `المستوى 15` rather than inverted `15 المستوى`).
