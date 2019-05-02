using NUnit.Framework;
using System.Collections;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class PluralHandling
    {
        public static IEnumerable TestCases_HasExpectedNumberOfPlurals
        {
            get
            {
                // 1 Plural
                yield return new TestCaseData("ay").Returns(1);
                yield return new TestCaseData("bo").Returns(1);
                yield return new TestCaseData("cgg").Returns(1);
                yield return new TestCaseData("dz").Returns(1);
                yield return new TestCaseData("id").Returns(1);
                yield return new TestCaseData("ja").Returns(1);
                yield return new TestCaseData("jbo").Returns(1);
                yield return new TestCaseData("ka").Returns(1);
                yield return new TestCaseData("km").Returns(1);
                yield return new TestCaseData("ko").Returns(1);
                yield return new TestCaseData("lo").Returns(1);
                yield return new TestCaseData("ms").Returns(1);
                yield return new TestCaseData("my").Returns(1);
                yield return new TestCaseData("sah").Returns(1);
                yield return new TestCaseData("su").Returns(1);
                yield return new TestCaseData("th").Returns(1);
                yield return new TestCaseData("tt").Returns(1);
                yield return new TestCaseData("ug").Returns(1);
                yield return new TestCaseData("vi").Returns(1);
                yield return new TestCaseData("wo").Returns(1);
                yield return new TestCaseData("zh").Returns(1);
                yield return new TestCaseData("zh-CHS").Returns(1);

                // 2 Plurals
                yield return new TestCaseData("mk").Returns(2);
                yield return new TestCaseData("jv").Returns(2);
                yield return new TestCaseData("af").Returns(2);
                yield return new TestCaseData("an").Returns(2);
                yield return new TestCaseData("anp").Returns(2);
                yield return new TestCaseData("as").Returns(2);
                yield return new TestCaseData("ast").Returns(2);
                yield return new TestCaseData("az").Returns(2);
                yield return new TestCaseData("bg").Returns(2);
                yield return new TestCaseData("bn").Returns(2);
                yield return new TestCaseData("brx").Returns(2);
                yield return new TestCaseData("ca").Returns(2);
                yield return new TestCaseData("da").Returns(2);
                yield return new TestCaseData("de").Returns(2);
                yield return new TestCaseData("doi").Returns(2);
                yield return new TestCaseData("el").Returns(2);
                yield return new TestCaseData("en").Returns(2);
                yield return new TestCaseData("eo").Returns(2);
                yield return new TestCaseData("es").Returns(2);
                yield return new TestCaseData("es_AR").Returns(2);
                yield return new TestCaseData("et").Returns(2);
                yield return new TestCaseData("eu").Returns(2);
                yield return new TestCaseData("ff").Returns(2);
                yield return new TestCaseData("fi").Returns(2);
                yield return new TestCaseData("fo").Returns(2);
                yield return new TestCaseData("fur").Returns(2);
                yield return new TestCaseData("fy").Returns(2);
                yield return new TestCaseData("gl").Returns(2);
                yield return new TestCaseData("gu").Returns(2);
                yield return new TestCaseData("ha").Returns(2);
                yield return new TestCaseData("he").Returns(2);
                yield return new TestCaseData("hi").Returns(2);
                yield return new TestCaseData("hne").Returns(2);
                yield return new TestCaseData("hu").Returns(2);
                yield return new TestCaseData("hy").Returns(2);
                yield return new TestCaseData("ia").Returns(2);
                yield return new TestCaseData("it").Returns(2);
                yield return new TestCaseData("kk").Returns(2);
                yield return new TestCaseData("kl").Returns(2);
                yield return new TestCaseData("kn").Returns(2);
                yield return new TestCaseData("ku").Returns(2);
                yield return new TestCaseData("ky").Returns(2);
                yield return new TestCaseData("lb").Returns(2);
                yield return new TestCaseData("mai").Returns(2);
                yield return new TestCaseData("ml").Returns(2);
                yield return new TestCaseData("mn").Returns(2);
                yield return new TestCaseData("mni").Returns(2);
                yield return new TestCaseData("mr").Returns(2);
                yield return new TestCaseData("nah").Returns(2);
                yield return new TestCaseData("nap").Returns(2);
                yield return new TestCaseData("nb").Returns(2);
                yield return new TestCaseData("ne").Returns(2);
                yield return new TestCaseData("nl").Returns(2);
                yield return new TestCaseData("nn").Returns(2);
                yield return new TestCaseData("no").Returns(2);
                yield return new TestCaseData("nso").Returns(2);
                yield return new TestCaseData("or").Returns(2);
                yield return new TestCaseData("pa").Returns(2);
                yield return new TestCaseData("pap").Returns(2);
                yield return new TestCaseData("pms").Returns(2);
                yield return new TestCaseData("ps").Returns(2);
                yield return new TestCaseData("pt").Returns(2);
                yield return new TestCaseData("rm").Returns(2);
                yield return new TestCaseData("rw").Returns(2);
                yield return new TestCaseData("sat").Returns(2);
                yield return new TestCaseData("sco").Returns(2);
                yield return new TestCaseData("sd").Returns(2);
                yield return new TestCaseData("se").Returns(2);
                yield return new TestCaseData("si").Returns(2);
                yield return new TestCaseData("so").Returns(2);
                yield return new TestCaseData("son").Returns(2);
                yield return new TestCaseData("sq").Returns(2);
                yield return new TestCaseData("sv").Returns(2);
                yield return new TestCaseData("sw").Returns(2);
                yield return new TestCaseData("ta").Returns(2);
                yield return new TestCaseData("te").Returns(2);
                yield return new TestCaseData("tk").Returns(2);
                yield return new TestCaseData("ur").Returns(2);
                yield return new TestCaseData("yo").Returns(2);
                yield return new TestCaseData("ach").Returns(2);
                yield return new TestCaseData("ak").Returns(2);
                yield return new TestCaseData("am").Returns(2);
                yield return new TestCaseData("arn").Returns(2);
                yield return new TestCaseData("br").Returns(2);
                yield return new TestCaseData("fa").Returns(2);
                yield return new TestCaseData("fil").Returns(2);
                yield return new TestCaseData("fr").Returns(2);
                yield return new TestCaseData("gun").Returns(2);
                yield return new TestCaseData("ln").Returns(2);
                yield return new TestCaseData("mfe").Returns(2);
                yield return new TestCaseData("mg").Returns(2);
                yield return new TestCaseData("mi").Returns(2);
                yield return new TestCaseData("oc").Returns(2);
                yield return new TestCaseData("pt_BR").Returns(2);
                yield return new TestCaseData("tg").Returns(2);
                yield return new TestCaseData("ti").Returns(2);
                yield return new TestCaseData("tr").Returns(2);
                yield return new TestCaseData("uz").Returns(2);
                yield return new TestCaseData("wa").Returns(2);
                yield return new TestCaseData("is").Returns(2);

                // 3 Plurals
                yield return new TestCaseData("lv").Returns(3);
                yield return new TestCaseData("lt").Returns(3);
                yield return new TestCaseData("be").Returns(3);
                yield return new TestCaseData("bs").Returns(3);
                yield return new TestCaseData("hr").Returns(3);
                yield return new TestCaseData("ru").Returns(3);
                yield return new TestCaseData("sr").Returns(3);
                yield return new TestCaseData("uk").Returns(3);
                yield return new TestCaseData("mnk").Returns(3);
                yield return new TestCaseData("ro").Returns(3);
                yield return new TestCaseData("pl").Returns(3);
                yield return new TestCaseData("cs").Returns(3);
                yield return new TestCaseData("sk").Returns(3);
                yield return new TestCaseData("csb").Returns(3);
                yield return new TestCaseData("me").Returns(3);

                // 4 Plurals
                yield return new TestCaseData("sl").Returns(4);
                yield return new TestCaseData("mt").Returns(4);
                yield return new TestCaseData("gd").Returns(4);
                yield return new TestCaseData("cy").Returns(4);
                yield return new TestCaseData("kw").Returns(4);

                // 5 Plurals
                yield return new TestCaseData("ga").Returns(5);

                // 6 Plurals
                yield return new TestCaseData("ar").Returns(6);
            }
        }

        public static IEnumerable TestCases_ReturnsCorrectPluralIndex
        {
            get
            {
                // English
                yield return new TestCaseData("en", 0).Returns(1);
                yield return new TestCaseData("en", 1).Returns(0);
                yield return new TestCaseData("en", 2).Returns(1);
            }
        }

        [TestCaseSource("TestCases_HasExpectedNumberOfPlurals")]
        public int HasExpectedNumberOfPlurals(string code)
        {
            return PluralForm.CreatePluralForm(code).NumberOfPlurals;
        }

        [TestCaseSource("TestCases_ReturnsCorrectPluralIndex")]
        public int ReturnsCorrectPluralIndex(string code, int value)
        {
            return PluralForm.GetPluralForm(code).Evaluate(value);
        }

        [Test]
        public void UnknownCodeReturnsNullPluralForm()
        {
            Assert.IsNull(PluralForm.CreatePluralForm("fdsfdsfdsfds"), "Expected null when an unknown code was used");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EvaluateWithNoEvaluateFunctionReturns0(int values)
        {
            var handler = new PluralForm() { Evaluator = null };
            Assert.AreEqual(0, handler.Evaluate(values));
        }

        [Test]
        public void GetPluralForm_ReturnsCachedValue()
        {
            var item1 = PluralForm.GetPluralForm("en");
            var item2 = PluralForm.GetPluralForm("en");
            Assert.AreSame(item1, item2, "Expected the same object to be returned. Was the object cached correctly?");
        }
    }
}