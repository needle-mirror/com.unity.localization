namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how the PluralForm can be used to select the correct plural to display.
    /// </summary>
    public class PluralFormExample : MonoBehaviour
    {
        public string localeCode = "en";

        public int numberOfApples = 1;

        public string[] pluralVersions =
        {
            "You have one apple",
            "You have {0} apples"
        };

        void OnGUI()
        {
            GUILayout.Label("How many apples do you have?");
            int.TryParse(GUILayout.TextField(numberOfApples.ToString(), GUILayout.Width(100)), out numberOfApples);

            var pluralForm = PluralForm.GetPluralForm(localeCode);
            if (pluralForm != null)
            {
                // NumberOfPlurals is the number of possible plural forms a language can contain. In English we would expect 2.
                if (pluralForm.NumberOfPlurals != pluralVersions.Length)
                {
                    GUILayout.Label(string.Format("Incorrect number of plurals in pluralVersions. Expected {0} but found {1}", pluralForm.NumberOfPlurals, pluralVersions.Length));
                    return;
                }

                // Evaluate the value to determine which plural we should use
                int pluralToUse = pluralForm.Evaluate(numberOfApples);

                // Now use the plural form and pass in the value as am argument so it can be added to the string.
                GUILayout.Label(string.Format(pluralVersions[pluralToUse], numberOfApples));
            }
            else
            {
                GUILayout.Label("Could not find a plural form for code: " + localeCode);
            }
        }
    }
}