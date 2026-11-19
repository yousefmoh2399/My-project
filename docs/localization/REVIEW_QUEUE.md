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
| `region.reg_north.name` | Northern Tundra | الجبهة الشمالية | World Map / Region | `Needs Human Review` | Fictional northern theater. |
| `region.reg_coast.name` | Coastline Dominion | ساحل الهيمنة | World Map / Region | `Needs Human Review` | Fictional coastal theater. |
| `region.reg_oasis.name` | Emerald Oasis | واحة الزمرد | World Map / Region | `Needs Human Review` | Fictional desert fuel hub. |
| `region.reg_valley.name` | Iron Valley | وادي الحديد | World Map / Region | `Needs Human Review` | Fictional central industrial basin. |
| `region.reg_ridge.name` | Obsidian Ridge | هضبة الأوبسيديان | World Map / Region | `Needs Human Review` | Fictional eastern mountain ridge. |
| `faction.neutral` | Neutral | محايد | World Map / Factions | `Needs Human Review` | Local militia/unclaimed forces. |
| `faction.player` | Player Faction | القيادة الوطنية | World Map / Factions | `Needs Human Review` | Player alliance / coalition forces. |
| `faction.enemy` | Enemy Alliance | قوات العدو | World Map / Factions | `Needs Human Review` | Hostile syndicate / rival alliance. |
| `faction.rebels` | Rebel Outpost | بؤرة متمردين | World Map / Factions | `Needs Human Review` | Rogue insurgent outposts. |
| `drawer.btn.attack` | Attack Territory | شن هجوم | World Map / UI | `Needs Human Review` | Tactical assault action. |
| `drawer.btn.scout` | Recon & Scout | استطلاع استخباراتي | World Map / UI | `Needs Human Review` | Military reconnaissance operation. |
| `drawer.btn.legend` | Map Legend | رموز الخريطة | World Map / Accessibility | `Needs Human Review` | Colorblind symbol accessibility guide. |
| `drawer.unlocked_in_phase9` | Tactical operations unlock in Phase 9 (Battle Engine) | العمليات القتالية ستتاح في المرحلة 9 | World Map / System | `Needs Human Review` | Disclaimer for disabled action buttons. |
| `territory.ter_nrd_001.name` | Frostpeak Citadel | قلعة قمة الجليد | Territory / Northern | `Needs Human Review` | Northern capital fortress. |
| `territory.ter_cst_001.name` | Harbor Prime | الميناء الرئيسي | Territory / Coastline | `Needs Human Review` | Coastline capital port. |
| `territory.ter_oas_001.name` | Palm Oasis | واحة النخيل | Territory / Oasis | `Needs Human Review` | Oasis capital oasis hub. |
| `territory.ter_val_001.name` | Iron Bastion | حصن الحديد | Territory / Valley | `Needs Human Review` | Valley capital fortress. |
| `territory.ter_rdg_001.name` | Ridge Spire | برج الهضبة | Territory / Ridge | `Needs Human Review` | Ridge capital mountain spire. |
| `building.hq.name` | Command Center | مقر القيادة | Base / Building | `Needs Human Review` | Central command structure. |
| `building.barracks.name` | Barracks | الثكنات العسكرية | Base / Building | `Needs Human Review` | Infantry quarters and training. |
| `building.depot.name` | Supply Depot | مستودع الإمداد | Base / Building | `Needs Human Review` | Resource storage and logistics. |
| `building.radar.name` | Radar Station | محطة الرادار | Base / Building | `Needs Human Review` | Early warning and recon. |
| `building.factory.name` | Vehicle Factory | مصنع الآليات | Base / Building | `Needs Human Review` | Armored vehicle production. |
| `base.drawer.action_upgrade` | Upgrade | ترقية | Base / UI | `Needs Human Review` | Structure upgrade button. |
| `base.cancel_dialog.title` | Cancel Construction | إلغاء أعمال الترقية | Base / UI | `Needs Human Review` | Upgrade cancellation prompt. |
| `unit.infantry.name` | Tactical Infantry | مشاة تكتيكية | Army / Units | `Needs Human Review` | Frontline combat infantry squad. |
| `unit.tank.name` | Main Battle Tank | دبابة قتال رئيسية | Army / Units | `Needs Human Review` | Heavy armored assault tank. |
| `unit.artillery.name` | Heavy Artillery | مدفعية ثقيلة | Army / Units | `Needs Human Review` | Long-range self-propelled bombardment. |
| `unit.recon.name` | Armored Recon Vehicle | مركبة استطلاع مدرعة | Army / Units | `Needs Human Review` | Fast light scout vehicle. |
| `unit.stat.power` | Combat Power | القوة القتالية | Army / Stats | `Needs Human Review` | Standard military combat power rating. |
| `army.screen.title` | Military Command | قيادة الجيش والقوات | Army / UI | `Needs Human Review` | Main army screen header. |
| `army.drawer.title` | Troop Mobilization | تعبئة الفصائل العسكرية | Army / UI | `Needs Human Review` | Recruitment drawer header. |
| `army.drawer.action_recruit` | Commence Mobilization | بدء التعبئة والتدريب | Army / UI | `Needs Human Review` | Recruitment order action button. |
| `army.cancel_dialog.title` | Abort Mobilization | إلغاء أمر التعبئة | Army / UI | `Needs Human Review` | Recruitment cancel dialog title. |

---

## Review Guidelines for Native Arabic Military Editor

When reviewing terms in this queue:
1. **Tone:** Modern tactical military terminology (used in modern Arab armed forces and military journalism), avoiding archaic medieval language while preserving authority and gravitas.
2. **Gender Neutrality & Command Style:** Use imperative verb forms for buttons (`تأكيد`, `إلغاء`, `عودة`) and formal operational language for descriptions.
3. **Numbers & Formatting:** Confirm that BiDi number runs render left-to-right (e.g. `المستوى 15` rather than inverted `15 المستوى`).
