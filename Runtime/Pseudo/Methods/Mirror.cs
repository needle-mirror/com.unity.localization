namespace UnityEngine.Localization.Pseudo
{
    /// <summary>
    /// Reverses all strings, to simulate right-to-left locales.
    /// </summary>
    public class Mirror : IPseudoLocalizationMethod
    {
        // There is a bug with SerializeField that causes empty instances to not deserialize. This is a workaround while we wait for the fix (case 1183543)
        [SerializeField, HideInInspector]
        int dummyObject;

        /// <summary>
        /// Reverse all strings, to simulate right-to-left locales.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Transform(string input)
        {
            var mirrorBuffer = new char[input.Length];

            int readPos = input.Length - 1;
            int writePos;

            // We search for a new line char in reverse,
            // when we find one we then copy that line into the buffer in reverse.
            for (int i = input.Length - 1; i >= 0; --i)
            {
                // Look for a new line
                if (input[i] == '\n')
                {
                    // Add the new line char
                    mirrorBuffer[i] = '\n';

                    // Mirror the line after the new line char.
                    writePos = i + 1;
                    while (readPos > i)
                    {
                        mirrorBuffer[writePos++] = input[readPos--];
                    }
                    readPos = i - 1;
                }
            }

            // Copy the remainder
            writePos = 0;
            while (readPos >= 0)
            {
                mirrorBuffer[writePos++] = input[readPos--];
            }

            return new string(mirrorBuffer);
        }
    }
}
