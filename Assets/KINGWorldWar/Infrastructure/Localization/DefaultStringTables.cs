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
            { "dialog.error_generic", "An unexpected tactical error occurred. Please retry." }
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
            { "dialog.error_generic", "حدث خطأ غير متوقع في العمليات. يرجى إعادة المحاولة." }
        };
    }
}
