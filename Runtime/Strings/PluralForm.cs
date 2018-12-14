using System.Collections.Generic;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Used to determine what version of a translated string should be used when dealing with plurals.
    /// </summary>
    public class PluralForm
    {
        public int NumberOfPlurals { get; set; }

        public delegate int EvaluatePluralDelegate(int n);

        public EvaluatePluralDelegate Evaluator { get; set; }

        static List<KeyValuePair<string, PluralForm>> s_PluralFormCache = new List<KeyValuePair<string, PluralForm>>();

        /// <summary>
        /// Returns the plural index to use for the value.
        /// Plural index values are based on the GNU Gettext standard http://www.gnu.org/software/gettext/manual/gettext.html#Plural-forms
        /// </summary>
        /// <returns>Index of the plural form to use.</returns>
        public virtual int Evaluate(int value)
        {
            if (Evaluator != null)
            {
                int ret = Evaluator(value);
                Debug.Assert(ret < NumberOfPlurals);
                return ret;
            }

            return 0;
        }

        /// <summary>
        /// Returns a PluralForm from the local cache for the code.
        /// </summary>
        /// <param name="code">locale code.</param>
        /// <returns></returns>
        public static PluralForm GetPluralForm(string code)
        {
            // Just a simple linear search as we don't expect there to be many plurals in use.
            foreach (var pluralForm in s_PluralFormCache)
            {
                if (pluralForm.Key == code)
                    return pluralForm.Value;
            }

            var newPluralForm = CreatePluralForm(code);
            if (newPluralForm != null)
            {
                s_PluralFormCache.Add(new KeyValuePair<string, PluralForm>(code, newPluralForm));
                return newPluralForm;
            }

            return null;
        }

        /// <summary>
        /// Returns a new PluralForm for the locale Code.
        /// </summary>
        /// <returns>PluralForm or null if one could not be found.</returns>
        public static PluralForm CreatePluralForm(string code)
        {
            // TDOO: Add support for regional codes. e.g en-US. Maybe split string at '-'

            // Plural forms taken from http://docs.translatehouse.org/projects/localization-guide/en/latest/l10n/pluralforms.html
            switch (code)
            {
                case "ay":
                case "bo":
                case "cgg":
                case "dz":
                case "id":
                case "ja":
                case "jbo":
                case "ka":
                case "km":
                case "ko":
                case "lo":
                case "ms":
                case "my":
                case "sah":
                case "su":
                case "th":
                case "tt":
                case "ug":
                case "vi":
                case "wo":
                case "zh":
                case "zh-CHS":
                    return new PluralForm() { NumberOfPlurals = 1, Evaluator = n => 0 };

                case "mk":
                    return new PluralForm() { NumberOfPlurals = 2, Evaluator = n => n == 1 || n % 10 == 1 ? 0 : 1 };

                case "jv":
                case "af":
                case "an":
                case "anp":
                case "as":
                case "ast":
                case "az":
                case "bg":
                case "bn":
                case "brx":
                case "ca":
                case "da":
                case "de":
                case "doi":
                case "el":
                case "en":
                case "eo":
                case "es":
                case "es_AR":
                case "et":
                case "eu":
                case "ff":
                case "fi":
                case "fo":
                case "fur":
                case "fy":
                case "gl":
                case "gu":
                case "ha":
                case "he":
                case "hi":
                case "hne":
                case "hu":
                case "hy":
                case "ia":
                case "it":
                case "kk":
                case "kl":
                case "kn":
                case "ku":
                case "ky":
                case "lb":
                case "mai":
                case "ml":
                case "mn":
                case "mni":
                case "mr":
                case "nah":
                case "nap":
                case "nb":
                case "ne":
                case "nl":
                case "nn":
                case "no":
                case "nso":
                case "or":
                case "pa":
                case "pap":
                case "pms":
                case "ps":
                case "pt":
                case "rm":
                case "rw":
                case "sat":
                case "sco":
                case "sd":
                case "se":
                case "si":
                case "so":
                case "son":
                case "sq":
                case "sv":
                case "sw":
                case "ta":
                case "te":
                case "tk":
                case "ur":
                case "yo":
                case "ach":
                case "ak":
                case "am":
                case "arn":
                case "br":
                case "fa":
                case "fil":
                case "fr":
                case "gun":
                case "ln":
                case "mfe":
                case "mg":
                case "mi":
                case "oc":
                case "pt_BR":
                case "tg":
                case "ti":
                case "tr":
                case "uz":
                case "wa":
                    //case "zh": // In rare cases where plural form introduces difference in personal pronoun (such as her vs. they, we vs. I), the plural form is different.
                    return new PluralForm()
                    {
                        NumberOfPlurals = 2,
                        Evaluator = n => n != 1 ? 1 : 0
                    };

                case "is":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 2,
                        Evaluator = n => (n % 10 != 1 || n % 100 == 11) ? 1 : 0
                    };

                case "lv":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n % 10 == 1 && n % 100 != 11 ? 0 :
                            n != 0 ? 1 : 2)
                    };

                case "lt":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n % 10 == 1 && n % 100 != 11 ? 0 :
                            n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2)
                    };

                case "be":
                case "bs":
                case "hr":
                case "ru":
                case "sr":
                case "uk":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n % 10 == 1 && n % 100 != 11 ? 0 :
                            n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2)
                    };

                case "mnk":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n == 0 ? 0 :
                            n == 1 ? 1 : 2)
                    };

                case "ro":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n == 1 ? 0 :
                            (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2)
                    };

                case "pl":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n == 1 ? 0 :
                            n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2)
                    };

                case "cs":
                case "sk":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n == 1) ? 0 :
                            (n >= 2 && n <= 4) ? 1 : 2
                    };

                case "csb":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => (n == 1) ? 0 :
                            n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                    };

                case "me":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 3,
                        Evaluator = n => n % 10 == 1 && n % 100 != 11 ? 0 :
                            n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2
                    };

                case "sl":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 4,
                        Evaluator = n => (n % 100 == 1 ? 0 :
                            n % 100 == 2 ? 1 :
                            n % 100 == 3 || n % 100 == 4 ? 2 : 3)
                    };

                case "mt":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 4,
                        Evaluator = n => (n == 1 ? 0 :
                            n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 :
                            (n % 100 > 10 && n % 100 < 20) ? 2 : 3)
                    };

                case "gd":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 4,
                        Evaluator = n => (n == 1 || n == 11) ? 0 :
                            (n == 2 || n == 12) ? 1 :
                            (n > 2 && n < 20) ? 2 : 3
                    };

                case "cy":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 4,
                        Evaluator = n => (n == 1) ? 0 :
                            (n == 2) ? 1 :
                            (n != 8 && n != 11) ? 2 : 3
                    };

                case "kw":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 4,
                        Evaluator = n => (n == 1) ? 0 :
                            (n == 2) ? 1 :
                            (n == 3) ? 2 : 3
                    };

                case "ga":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 5,
                        Evaluator = n => n == 1 ? 0 :
                            n == 2 ? 1 :
                            (n > 2 && n < 7) ? 2 :
                            (n > 6 && n < 11) ? 3 : 4
                    };

                case "ar":
                    return new PluralForm()
                    {
                        NumberOfPlurals = 6,
                        Evaluator = n => (n == 0 ? 0 :
                            n == 1 ? 1 :
                            n == 2 ? 2 :
                            n % 100 >= 3 && n % 100 <= 10 ? 3 :
                            n % 100 >= 11 ? 4 : 5)
                    };
            }
            return null;
        }
    }
}

