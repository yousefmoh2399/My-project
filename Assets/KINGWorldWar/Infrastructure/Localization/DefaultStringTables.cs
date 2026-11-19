using System.Collections.Generic;
using KingWorldWar.Core.Localization;

namespace KingWorldWar.Infrastructure.Localization
{
    /// <summary>
    /// Static string tables for English and Arabic.
    /// Provides 100% key-parity baseline for KING WORLD WAR.
    /// </summary>
    public static class DefaultStringTables
    {
        public static Dictionary<string, string> GetTable(Language language)
        {
            return language == Language.Arabic ? ArabicTable : EnglishTable;
        }

        public static readonly Dictionary<string, string> EnglishTable = new Dictionary<string, string>
        {
            // Navigation
            { "nav.world", "World" },
            { "nav.base", "Base" },
            { "nav.army", "Army" },
            { "nav.alliance", "Alliance" },
            { "nav.rankings", "Rankings" },
            { "nav.profile", "Profile" },
            { "nav.back", "Back" },

            // Placeholders
            { "placeholder.world.title", "World Operations" },
            { "placeholder.world.desc", "PLACEHOLDER – World (Phase 4)" },
            { "placeholder.base.title", "Command Base" },
            { "placeholder.base.desc", "PLACEHOLDER – Base (Phase 5)" },
            { "placeholder.army.title", "Military Units" },
            { "placeholder.army.desc", "PLACEHOLDER – Army (Phase 6)" },
            { "placeholder.alliance.title", "Coalition Command" },
            { "placeholder.alliance.desc", "PLACEHOLDER – Alliance (Phase 7)" },
            { "placeholder.rankings.title", "Global Rankings" },
            { "placeholder.rankings.desc", "PLACEHOLDER – Rankings (Phase 8)" },
            { "placeholder.profile.title", "Commander Dossier" },
            { "placeholder.profile.desc", "PLACEHOLDER – Profile (Phase 9)" },

            // Common UI
            { "common.confirm", "Confirm" },
            { "common.cancel", "Cancel" },
            { "common.retry", "Retry" },
            { "common.close", "Close" },
            { "common.loading", "Loading..." },
            { "common.language", "Language" },
            { "common.gold", "Gold" },
            { "common.oil", "Oil" },
            { "common.iron", "Iron" },
            { "common.level_format", "Level {0}" },

            // Modals & Dialogs
            { "dialog.confirm_title", "Confirm Action" },
            { "dialog.confirm_exit_msg", "Are you sure you want to proceed?" },
            { "dialog.switch_lang_title", "Language Changed" },
            { "dialog.switch_lang_msg", "Language has been updated successfully." },
            { "dialog.error_title", "Command Error" },
            { "dialog.error_generic", "An unexpected tactical error occurred. Please retry." },

            // Regions (5)
            { "region.north.name", "Al-Shamal Reach" },
            { "region.north.desc", "Glacial highlands, strategic chokepoints, and rich iron deposits." },
            { "region.coast.name", "Iron Coast Basin" },
            { "region.coast.desc", "Industrial maritime delta, shipyards, and major petroleum depots." },
            { "region.oasis.name", "Al-Waha Front" },
            { "region.oasis.desc", "Arid desert plateau with vital petroleum fields and canyon passes." },
            { "region.valley.name", "Verdant Valley" },
            { "region.valley.desc", "Fertile central riverlands, agricultural granaries, and command nexus." },
            { "region.ridge.name", "Obsidian Ridge" },
            { "region.ridge.desc", "Volcanic mountain redoubts, basalt canyons, and heavy ordnance foundries." },

            // North Territories (5)
            { "territory.nrd_001.name", "Frostpeak Redoubt" },
            { "territory.nrd_001.desc", "Ancient mountaintop fortress serving as the northern command citadel." },
            { "territory.nrd_002.name", "Borealis Pass" },
            { "territory.nrd_002.desc", "Narrow glacial passage through the northern mountain range." },
            { "territory.nrd_003.name", "Ironcrag Valley" },
            { "territory.nrd_003.desc", "Heavy mining gorge producing high-grade iron ore for weapons manufacturing." },
            { "territory.nrd_004.name", "Glacial Rim Outpost" },
            { "territory.nrd_004.desc", "Radar surveillance station monitoring the northern sea approaches." },
            { "territory.nrd_005.name", "Valken Tundra" },
            { "territory.nrd_005.desc", "Frozen wind-swept plains ideal for rapid mechanized armored maneuvers." },

            // Coast Territories (5)
            { "territory.cst_001.name", "Steel Bay Anchorage" },
            { "territory.cst_001.desc", "Deepwater harbor and regional naval fortress commanding the western seas." },
            { "territory.cst_002.name", "Crimson Docks" },
            { "territory.cst_002.desc", "Vast drydocks, cargo cranes, and automated assembly yards." },
            { "territory.cst_003.name", "Trident Delta" },
            { "territory.cst_003.desc", "Triple-river mouth marshlands bridging the coast with the central valley." },
            { "territory.cst_004.name", "Saltmarsh Depot" },
            { "territory.cst_004.desc", "Concealed petroleum storage bunkers built among the coastal salt flats." },
            { "territory.cst_005.name", "Beacon Headland" },
            { "territory.cst_005.desc", "Fortified promontory with coastal artillery batteries overlooking maritime routes." },

            // Oasis Territories (5)
            { "territory.oas_001.name", "Sunfire Citadel" },
            { "territory.oas_001.desc", "Massive sandstone fortress standing defiant against desert storms." },
            { "territory.oas_002.name", "Mirage Dune Fields" },
            { "territory.oas_002.desc", "Shifting sand ocean concealing valuable subsurface petroleum deposits." },
            { "territory.oas_003.name", "Black Gold Wells" },
            { "territory.oas_003.desc", "Industrial oil refinery complex pumping fuel to frontier war machines." },
            { "territory.oas_004.name", "Qalat Al-Rih Pass" },
            { "territory.oas_004.desc", "Windy canyon gorge guarding the entrance to the desert heartland." },
            { "territory.oas_005.name", "Nomad Crossing" },
            { "territory.oas_005.desc", "Historic desert caravan intersection transformed into an armed logistical hub." },

            // Valley Territories (5)
            { "territory.val_001.name", "Emerald Nexus" },
            { "territory.val_001.desc", "Central command capital and strategic junction connecting all five regions." },
            { "territory.val_002.name", "Silver River Bridge" },
            { "territory.val_002.desc", "Reinforced strategic bridge spanning the mighty Silver River." },
            { "territory.val_003.name", "Golden Plains Granary" },
            { "territory.val_003.desc", "Endless wheat plateaus generating crucial gold trade and troop sustenance." },
            { "territory.val_004.name", "Whispering Woods" },
            { "territory.val_004.desc", "Dense ancient forest providing natural ambush cover and defense." },
            { "territory.val_005.name", "Amber Ridge" },
            { "territory.val_005.desc", "Gentle hillside heights offering wide panoramic view over central crossroads." },

            // Ridge Territories (5)
            { "territory.rdg_001.name", "Ashforge Bastion" },
            { "territory.rdg_001.desc", "Subterranean military stronghold fueled by geothermal volcanic energy." },
            { "territory.rdg_002.name", "Basalt Canyon" },
            { "territory.rdg_002.desc", "Towering dark stone gorge forming an impenetrable defensive barrier." },
            { "territory.rdg_003.name", "Brimstone Mines" },
            { "territory.rdg_003.desc", "Volcanic sulfur pits providing essential components for heavy ordnance." },
            { "territory.rdg_004.name", "Cinder Plateau" },
            { "territory.rdg_004.desc", "Scorched volcanic tableland utilized for heavy armor testing." },
            { "territory.rdg_005.name", "Magma Gate" },
            { "territory.rdg_005.desc", "Heavily fortified redoubt guarding the southern frontier pass." },

            // Factions (4)
            { "faction.allied", "Allied Coalition" },
            { "faction.neutral", "Neutral Territory" },
            { "faction.hostile_red", "Crimson Pact" },
            { "faction.hostile_blue", "Iron Syndicate" },

            // Territory Detail Drawer & Legend UI
            { "world.drawer.title", "Territory Dossier" },
            { "world.drawer.yield", "Hourly Yield" },
            { "world.drawer.defense", "Garrison Power" },
            { "world.drawer.owner", "Controlling Faction" },
            { "world.drawer.adjacent", "Connected Frontiers" },
            { "world.drawer.action_attack", "Attack" },
            { "world.drawer.action_scout", "Scout" },
            { "world.drawer.phase9_note", "Combat operations unlock in Phase 9" },
            { "world.legend.title", "Theater Factions" },

            // Base & Building Names & Descriptions
            { "building.hq.name", "Command Center" },
            { "building.hq.desc", "Headquarters of base operations and strategic command." },
            { "building.hq.desc_perk", "Max Building Level: {0}" },
            { "building.barracks.name", "Barracks" },
            { "building.barracks.desc", "Infantry quarters, tactical squad training, and base garrison." },
            { "building.barracks.desc_perk", "Troop Capacity: {0}" },
            { "building.depot.name", "Supply Depot" },
            { "building.depot.desc", "Reinforced logistics warehouses securing base resource stockpiles." },
            { "building.depot.desc_perk", "Storage Capacity: {0}" },
            { "building.radar.name", "Radar Station" },
            { "building.radar.desc", "Early warning scanning array and battlefield intelligence center." },
            { "building.radar.desc_perk", "Recon Range: {0} Regions" },
            { "building.factory.name", "Vehicle Factory" },
            { "building.factory.desc", "Heavy industrial plant assembling tanks and mechanized combat armor." },
            { "building.factory.desc_perk", "Armor Staging: {0}" },

            // Base Building Detail Drawer UI
            { "base.drawer.title", "Structure Dossier" },
            { "base.drawer.level", "Level {0}" },
            { "base.drawer.max_level", "MAX LEVEL" },
            { "base.drawer.upgrade_cost", "Upgrade Cost" },
            { "base.drawer.upgrade_time", "Construction Time" },
            { "base.drawer.req_hq", "Requires HQ Level {0}" },
            { "base.drawer.req_hq_met", "HQ Requirement Satisfied" },
            { "base.drawer.action_upgrade", "Upgrade" },
            { "base.drawer.action_cancel", "Cancel" },
            { "base.drawer.upgrading", "Upgrading..." },
            { "base.drawer.time_remaining", "Time Remaining: {0}" },
            { "base.drawer.current_perk", "Current Performance" },
            { "base.drawer.next_perk", "Next Level Upgrade" },
            { "base.cancel_dialog.title", "Cancel Construction" },
            { "base.cancel_dialog.msg", "Are you sure you want to abort the upgrade in progress?" }
        };

        public static readonly Dictionary<string, string> ArabicTable = new Dictionary<string, string>
        {
            // Navigation
            { "nav.world", "العالم" },
            { "nav.base", "القاعدة" },
            { "nav.army", "الجيش" },
            { "nav.alliance", "التحالف" },
            { "nav.rankings", "التصنيف" },
            { "nav.profile", "الملف" },
            { "nav.back", "رجوع" },

            // Placeholders
            { "placeholder.world.title", "العمليات العالمية" },
            { "placeholder.world.desc", "نموذج تجريبي – خريطة العالم (المرحلة 4)" },
            { "placeholder.base.title", "قاعدة العمليات" },
            { "placeholder.base.desc", "نموذج تجريبي – إدارة القاعدة (المرحلة 5)" },
            { "placeholder.army.title", "القوات العسكرية" },
            { "placeholder.army.desc", "نموذج تجريبي – تشكيلات الجيش (المرحلة 6)" },
            { "placeholder.alliance.title", "قيادة التحالف" },
            { "placeholder.alliance.desc", "نموذج تجريبي – قوات التحالف (المرحلة 7)" },
            { "placeholder.rankings.title", "التصنيف العالمي" },
            { "placeholder.rankings.desc", "نموذج تجريبي – لوحة الصدارة (المرحلة 8)" },
            { "placeholder.profile.title", "ملف القائد" },
            { "placeholder.profile.desc", "نموذج تجريبي – السجل العسكري (المرحلة 9)" },

            // Common UI
            { "common.confirm", "تأكيد" },
            { "common.cancel", "إلغاء" },
            { "common.retry", "إعادة المحاولة" },
            { "common.close", "إغلاق" },
            { "common.loading", "جارٍ التحميل..." },
            { "common.language", "اللغة" },
            { "common.gold", "الذهب" },
            { "common.oil", "النفط" },
            { "common.iron", "الحديد" },
            { "common.level_format", "المستوى {0}" },

            // Modals & Dialogs
            { "dialog.confirm_title", "تأكيد الإجراء" },
            { "dialog.confirm_exit_msg", "هل أنت متأكد من رغبتك في المتابعة؟" },
            { "dialog.switch_lang_title", "تم تغيير اللغة" },
            { "dialog.switch_lang_msg", "تم تحديث لغة الواجهة بنجاح." },
            { "dialog.error_title", "خطأ تكتيكي" },
            { "dialog.error_generic", "حدث خطأ غير متوقع في العمليات. يرجى إعادة المحاولة." },

            // Regions (5)
            { "region.north.name", "مرتفعات الشمال" },
            { "region.north.desc", "هضاب جليدية وتندرا جبلية وممرات وعرة ومناجم حديد وفيرة." },
            { "region.coast.name", "حوض الساحل الفولاذي" },
            { "region.coast.desc", "مرافئ صناعية، قنوات بحرية، ومستودعات وقود ونفط استراتيجية." },
            { "region.oasis.name", "جبهة الواحة الكبرى" },
            { "region.oasis.desc", "هضبة صحراوية قاحلة، كثبان حارقة، وآبار طاقة ومعابر تكتيكية." },
            { "region.valley.name", "الوادي الخصيب" },
            { "region.valley.desc", "سهول زراعية مروية، معابر نهرية، ومقر القيادة المركزي." },
            { "region.ridge.name", "حافة الأوبسيديان" },
            { "region.ridge.desc", "قمم بركانية سوداء، قلاع جبلية، ومصاهر أسلحة وذخائر ثقيلة." },

            // North Territories (5)
            { "territory.nrd_001.name", "معقل قمة الجليد" },
            { "territory.nrd_001.desc", "قلعة جبلية حصينة على قمة المرتفعات تمثل المقر القيادي لشمال البلاد." },
            { "territory.nrd_002.name", "ممر الشفق الشمالي" },
            { "territory.nrd_002.desc", "ممر جليدي ضيق يخترق السلاسل الجبلية الشمالية الوعرة." },
            { "territory.nrd_003.name", "وادي الصخر الحديدي" },
            { "territory.nrd_003.desc", "أخدود تعدين عميق ينتج خامات الحديد عالية الجودة لتصنيع العتاد الحربي." },
            { "territory.nrd_004.name", "مرصد الحافة الجليدية" },
            { "territory.nrd_004.desc", "محطة رصد ومراقبة متقدمة تطل على المسارات البحرية الشمالية." },
            { "territory.nrd_005.name", "تندرا فالكن" },
            { "territory.nrd_005.desc", "سهول متجمدة مفتوحة تلائم المناورات السريعة لقوات المدرعات." },

            // Coast Territories (5)
            { "territory.cst_001.name", "مرسى الخليج الفولاذي" },
            { "territory.cst_001.desc", "ميناء مياه عميقة وقاعدة بحرية كبرى تسيطر على الممرات المائية الغربية." },
            { "territory.cst_002.name", "أرصفة القرمز الصناعية" },
            { "territory.cst_002.desc", "أحواض بناء وسفن شحن ورافعات تفريغ ومستودعات صيانة ساحلية ضخمة." },
            { "territory.cst_003.name", "دلتا الرمح الثلاثي" },
            { "territory.cst_003.desc", "أراضي دلتا نهرية ثلاثية تشكل جسر العبور التكتيكي نحو الوادي المركزي." },
            { "territory.cst_004.name", "مستودع سبخات الملح" },
            { "territory.cst_004.desc", "خزانات وقود ومستودعات محروقات مموهة بين السبخات الملحية الساحلية." },
            { "territory.cst_005.name", "رأس الفنار العسكري" },
            { "territory.cst_005.desc", "رأس ساحلي حصين مجهز ببطاريات مدفعية ساحلية لمراقبة حركة الملاحة." },

            // Oasis Territories (5)
            { "territory.oas_001.name", "قلعة شمس الصحراء" },
            { "territory.oas_001.desc", "قلعة حجرية مهيبة تقف بشموخ في وجه العواصف الرملية الصحراوية." },
            { "territory.oas_002.name", "كثبان السراب الكبرى" },
            { "territory.oas_002.desc", "بحر رمال متحرك يخفي في باطنه مكامن نفطية استراتيجية بالغة الأهمية." },
            { "territory.oas_003.name", "آبار الذهب الأسود" },
            { "territory.oas_003.desc", "مجمع تكرير وإنتاج محروقات يمد الآليات العسكرية المتقدمة بالوقود." },
            { "territory.oas_004.name", "ممر قلعة الرياح" },
            { "territory.oas_004.desc", "خانق جبلي شديد الرياح يحرس المدخل الشرقي للعمق الصحراوي." },
            { "territory.oas_005.name", "معبر القوافل التكتيكي" },
            { "territory.oas_005.desc", "ملتقى قوافل تاريخي تحول إلى مركز إمداد ولوجستيات مسلحة في عمق الصحراء." },

            // Valley Territories (5)
            { "territory.val_001.name", "ملتقى الزمرد المركزي" },
            { "territory.val_001.desc", "عاصمة القيادة المركزية ومفترق الطرق الاستراتيجي الرابط بين كافة الأقاليم." },
            { "territory.val_002.name", "جسر النهر الفضي" },
            { "territory.val_002.desc", "جسر مدرع استراتيجي يعبر فوق نهر الفضة الأكبر لربط الوادي بالساحل." },
            { "territory.val_003.name", "مخازن السهول الذهبية" },
            { "territory.val_003.desc", "سهول زراعية ممتدة تؤمن إيرادات تجارية هامة ومؤونة حيوية للجيوش." },
            { "territory.val_004.name", "غابات الهمس الدفاعية" },
            { "territory.val_004.desc", "غابات كثيفة قديمة توفر غطاءً دفاعياً طبيعياً وفرصاً مثالية للكمائن." },
            { "territory.val_005.name", "مرتفعات الكهرمان" },
            { "territory.val_005.desc", "مرتفعات تلال هادئة تتيح رصداً بانورامياً واسعاً لمفترق الطرق المركزي." },

            // Ridge Territories (5)
            { "territory.rdg_001.name", "معقل مصهر الرماد" },
            { "territory.rdg_001.desc", "معقل عسكري تحت الأرض يستمد طاقته من الحرارة الجوفية البركانية." },
            { "territory.rdg_002.name", "أخدود البازلت الأسود" },
            { "territory.rdg_002.desc", "أخدود صخري أسود شاهق الارتفاع يشكل حاجزاً دفاعياً طبيعياً منيعاً." },
            { "territory.rdg_003.name", "مناجم الكبريت والبارود" },
            { "territory.rdg_003.desc", "مناجم كبريتية بركانية توفر المواد الأساسية لصناعة المتفجرات والذخيرة." },
            { "territory.rdg_004.name", "هضبة الجمر الملتهب" },
            { "territory.rdg_004.desc", "هضبة بركانية مستوية تستخدم لاختبارات وتدريبات الدبابات والمدرعات الثقيلة." },
            { "territory.rdg_005.name", "بوابة الحمم الحصينة" },
            { "territory.rdg_005.desc", "حصن دفاعي متقدم وشديد التحصين يحرس الممر الحدودي الجنوبي للخريطة." },

            // Factions (4)
            { "faction.allied", "التحالف الحليف" },
            { "faction.neutral", "أراضٍ محايدة" },
            { "faction.hostile_red", "حلف القرمز المعادي" },
            { "faction.hostile_blue", "نقابة الحديد المعادية" },

            // Territory Detail Drawer & Legend UI
            { "world.drawer.title", "ملف المقاطعة التكتيكي" },
            { "world.drawer.yield", "الإنتاج في الساعة" },
            { "world.drawer.defense", "قوة الحامية العسكرية" },
            { "world.drawer.owner", "الجهة المسيطرة" },
            { "world.drawer.adjacent", "الحدود المتصلة" },
            { "world.drawer.action_attack", "هجوم" },
            { "world.drawer.action_scout", "استطلاع" },
            { "world.drawer.phase9_note", "ستتاح العمليات القتالية في المرحلة 9" },
            { "world.legend.title", "دليل القوى في مسرح العمليات" },

            // Base & Building Names & Descriptions
            { "building.hq.name", "مقر القيادة" },
            { "building.hq.desc", "مقر العمليات والقيادة الاستراتيجية للقاعدة." },
            { "building.hq.desc_perk", "الحد الأقصى لمستوى المباني: {0}" },
            { "building.barracks.name", "الثكنات العسكرية" },
            { "building.barracks.desc", "تمركز وتدريب فرق المشاة التكتيكية وحراسة القاعدة." },
            { "building.barracks.desc_perk", "سعة القوات: {0}" },
            { "building.depot.name", "مستودع الإمداد" },
            { "building.depot.desc", "مستودعات لوجستية محصنة لتخزين وتأمين مخزون الموارد." },
            { "building.depot.desc_perk", "سعة التخزين: {0}" },
            { "building.radar.name", "محطة الرادار" },
            { "building.radar.desc", "منظومة مسح وإنذار مبكر ومركز استخبارات مسرح العمليات." },
            { "building.radar.desc_perk", "مدى الاستطلاع: {0} أقاليم" },
            { "building.factory.name", "مصنع الآليات" },
            { "building.factory.desc", "مجمع صناعي ثقيل لتجميع وصيانة الدبابات والمدرعات القتالية." },
            { "building.factory.desc_perk", "سعة الدروع: {0}" },

            // Base Building Detail Drawer UI
            { "base.drawer.title", "بيانات المنشأة" },
            { "base.drawer.level", "المستوى {0}" },
            { "base.drawer.max_level", "المستوى الأقصى" },
            { "base.drawer.upgrade_cost", "تكلفة الترقية" },
            { "base.drawer.upgrade_time", "وقت البناء" },
            { "base.drawer.req_hq", "يتطلب مقر القيادة مستوى {0}" },
            { "base.drawer.req_hq_met", "تم استيفاء شرط مقر القيادة" },
            { "base.drawer.action_upgrade", "ترقية" },
            { "base.drawer.action_cancel", "إلغاء" },
            { "base.drawer.upgrading", "جاري الترقية..." },
            { "base.drawer.time_remaining", "الوقت المتبقي: {0}" },
            { "base.drawer.current_perk", "القدرات الحالية" },
            { "base.drawer.next_perk", "مزايا المستوى التالي" },
            { "base.cancel_dialog.title", "إلغاء أعمال الترقية" },
            { "base.cancel_dialog.msg", "هل أنت متأكد من إلغاء عملية الترقية الجارية؟" }
        };
    }
}
