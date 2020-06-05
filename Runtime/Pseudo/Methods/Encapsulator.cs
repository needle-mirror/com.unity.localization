using System;

namespace UnityEngine.Localization.Pseudo
{
    /// <summary>
    /// Encapsulates a string between 2 other strings.
    /// The default will add brackets around each input, to show where messages have
    /// been concatenated together and to indicate if text has been clipped in the UI.
    /// Concatenating text is a particular issue as some languages may need to reorder
    /// phrases or the translation may change depending on what is around it.
    /// </summary>
    [Serializable]
    public class Encapsulator : IPseudoLocalizationMethod
    {
        [SerializeField]
        string m_Start = "[";

        [SerializeField]
        string m_End = "]";

        /// <summary>
        /// String that will be added to the start of the input string.
        /// </summary>
        public string Start
        {
            get => m_Start;
            set => m_Start = value;
        }

        /// <summary>
        /// String that will be added to the end of the input string.
        /// </summary>
        public string End
        {
            get => m_End;
            set => m_End = value;
        }

        /// <summary>
        /// Encapsulates the input between the <see cref="Start"/> and
        /// <see cref="End"/> strings.
        /// </summary>
        /// <param name="input">The string that will be encapsulated.</param>
        /// <returns>New encapsulated string.</returns>
        public string Transform(string input)
        {
            return $"{Start}{input}{End}";
        }
    }
}
