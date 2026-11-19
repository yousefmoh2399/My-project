using KingWorldWar.Core.Localization;
using NUnit.Framework;

namespace KingWorldWar.Tests.EditMode
{
    [TestFixture]
    public class ArabicTextShaperTests
    {
        [Test]
        public void ContainsArabic_WithArabicText_ReturnsTrue()
        {
            Assert.IsTrue(ArabicTextShaper.ContainsArabic("مرحبا"));
            Assert.IsTrue(ArabicTextShaper.ContainsArabic("King World War - حرب الملوك"));
        }

        [Test]
        public void ContainsArabic_WithEnglishOnly_ReturnsFalse()
        {
            Assert.IsFalse(ArabicTextShaper.ContainsArabic("Hello World 123"));
            Assert.IsFalse(ArabicTextShaper.ContainsArabic(""));
            Assert.IsFalse(ArabicTextShaper.ContainsArabic(null));
        }

        [Test]
        public void Shape_NullOrEmpty_ReturnsSame()
        {
            Assert.IsNull(ArabicTextShaper.Shape(null));
            Assert.AreEqual("", ArabicTextShaper.Shape(""));
        }

        [Test]
        public void Shape_EnglishText_ReturnsUnchanged()
        {
            const string english = "KING WORLD WAR - Tactical Command 2026";
            Assert.AreEqual(english, ArabicTextShaper.Shape(english));
        }

        [Test]
        public void Shape_SimpleArabicWord_ConnectsGleamForms()
        {
            // "حرب" (Hah, Reh, Beh)
            // In initial Hah: \uFEA3, isolated Reh: \uFEAD, isolated Beh: \uFE8F
            string raw = "حرب";
            string shaped = ArabicTextShaper.Shape(raw);

            Assert.IsNotNull(shaped);
            Assert.AreNotEqual(raw, shaped, "Shaped Arabic should convert base chars into presentation forms");
            Assert.AreEqual(3, shaped.Length);
        }

        [Test]
        public void Shape_MixedArabicAndNumbers_PreservesNumberOrder()
        {
            string mixed = "المستوى 45";
            string shaped = ArabicTextShaper.Shape(mixed);

            Assert.IsNotNull(shaped);
            // Digits '4' and '5' must appear in consecutive order "45", not "54"
            Assert.IsTrue(shaped.Contains("45"), "Numbers in Arabic sentences must maintain internal LTR order");
        }

        [Test]
        public void Shape_LamAlifLigature_CombinedIntoSingleGlyph()
        {
            // "لا" (Lam + Alef) -> \uFEFB
            string shaped = ArabicTextShaper.Shape("لا");
            Assert.IsNotNull(shaped);
            Assert.AreEqual(1, shaped.Length, "Lam-Alef should be combined into a single ligature character");
            Assert.AreEqual('\uFEFB', shaped[0]);
        }
    }
}
