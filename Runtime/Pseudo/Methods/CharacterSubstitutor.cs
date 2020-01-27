using System;
using System.Collections.Generic;

namespace UnityEngine.Localization.Pseudo
{
    /// <summary>
    /// Replaces characters in the input string.
    /// </summary>
    [Serializable]
    public class CharacterSubstitutor : IPseudoLocalizationMethod, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The substitution method to use when applying Pseudo-Localization.
        /// </summary>
        public enum SubstitutionMethod
        {
            /// <summary>
            /// Converts all characters to uppercase.
            /// </summary>
            ToUpper,

            /// <summary>
            /// Converts all characters to lowercase.
            /// </summary>
            ToLower,

            /// <summary>
            /// Replaces all characters with a single character from <see cref="ReplacementList"/>.
            /// This method is a good way to find any hard coded text or to test for missing characters in a font.
            /// </summary>
            List,

            /// <summary>
            /// Uses <see cref="Replacements"/> to replace each character, when it is present in the dictionary.
            /// </summary>
            Map
        }

        [Serializable]
        struct CharReplacement
        {
            public char original;
            public char replacement;
        }

        /// <summary>
        ///
        /// </summary>
        public enum ListSelectionMethod
        {
            /// <summary>
            /// Select characters at random from <see cref="ReplacementList"/>.
            /// </summary>
            Random,

            /// <summary>
            /// Starts at the position from the previous input string and use each character in <see cref="ReplacementList"/>,
            /// looping back to the start if the end of the list is reached.
            /// </summary>
            LoopFromPrevious,

            /// <summary>
            /// Starts at the first item in <see cref="ReplacementList"/> and uses each character in the list
            /// looping back to the start if the end of the list is reached.
            /// </summary>
            LoopFromStart
        }

        [SerializeField]
        SubstitutionMethod m_SubstitutionMethod;

        [SerializeField]
        ListSelectionMethod m_ListMode;

        [SerializeField]
        List<CharReplacement> m_ReplacementsMap;

        [SerializeField]
        List<char> m_ReplacementList = new List<char> { '_' };

        internal int m_ReplacementsPosition;

        /// <summary>
        /// The substitution method to use.
        /// </summary>
        public SubstitutionMethod Method
        {
            get => m_SubstitutionMethod;
            set => m_SubstitutionMethod = value;
        }

        /// <summary>
        /// Dictionary of characters that will be replaced when using <see cref="Method.ReplacementMap"/>. Any value that is not present will be preserved.
        /// </summary>
        public Dictionary<char, char> ReplacementMap { get; set; } = new Dictionary<char, char>();

        public ListSelectionMethod ListMode
        {
            get => m_ListMode;
            set => m_ListMode = value;
        }

        /// <summary>
        /// The characters to use when using <see cref="Method.SingleCharacter"/> mode.
        /// </summary>
        public List<char> ReplacementList
        {
            get => m_ReplacementList;
            set => m_ReplacementList = value;
        }

        /// <summary>
        /// Replaces each character in the input with a replacement character if one can be found.
        /// If a replacement character can not be found then the original is kept.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Transform(string input)
        {
            switch (Method)
            {
                case SubstitutionMethod.Map:
                    var converted = new char[input.Length];
                    for (int i = 0; i < converted.Length; ++i)
                    {
                        converted[i] = ReplaceCharFromMap(input[i]);
                    }
                    return new string(converted);

                case SubstitutionMethod.ToUpper:
                    return input.ToUpper();

                case SubstitutionMethod.ToLower:
                    return input.ToLower();

                case SubstitutionMethod.List:

                    if (m_ReplacementList == null || m_ReplacementList.Count == 0)
                        break;

                    if (m_ReplacementList.Count == 1)
                        return new string(m_ReplacementList[0], input.Length);

                    var newValues = new char[input.Length];

                    if (ListMode == ListSelectionMethod.Random)
                    {
                        Random.InitState(GetRandomSeed(input));
                        for (int i = 0; i < newValues.Length; ++i)
                        {
                            newValues[i] = m_ReplacementList[Random.Range(0, m_ReplacementList.Count)];
                        }
                    }
                    else
                    {
                        if (ListMode == ListSelectionMethod.LoopFromStart)
                            m_ReplacementsPosition = 0;

                        for (int i = 0; i < newValues.Length; ++i, ++m_ReplacementsPosition)
                        {
                            newValues[i] = m_ReplacementList[m_ReplacementsPosition % m_ReplacementList.Count];
                        }
                    }

                    return new string(newValues);
            }

            return input;
        }

        int GetRandomSeed(string input) => input.GetHashCode();

        /// <summary>
        /// Attempts to find a replacement character and return it.
        /// </summary>
        /// <param name="value">The character to replace.</param>
        /// <returns>The replacement character of the same if none could be found.</returns>
        internal char ReplaceCharFromMap(char value)
        {
            if (ReplacementMap != null && ReplacementMap.TryGetValue(value, out char replacement))
                return replacement;
            return value;
        }

        public void OnBeforeSerialize()
        {
            if (m_ReplacementsMap == null)
                m_ReplacementsMap = new List<CharReplacement>();
            m_ReplacementsMap.Clear();

            foreach (var pair in ReplacementMap)
            {
                m_ReplacementsMap.Add(new CharReplacement() { original = pair.Key, replacement = pair.Value });
            }
        }

        public void OnAfterDeserialize()
        {
            if (ReplacementMap == null)
                ReplacementMap = new Dictionary<char, char>();
            ReplacementMap.Clear();

            foreach (var d in m_ReplacementsMap)
            {
                ReplacementMap[d.original] = d.replacement;
            }
        }
    }
}