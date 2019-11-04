using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.Tests
{
    public class StringDatabaseTests
    {
        LocalizedStringDatabase m_StringDatabase = new LocalizedStringDatabase();

        [Test]
        public void StringDatabaseTestsSimplePasses()
        {
            // Uses fallback. Return string from fallback table

            //m_StringDatabase.GetTableAsync()
            // Use the Assert class to test conditions
        }
    }
}
