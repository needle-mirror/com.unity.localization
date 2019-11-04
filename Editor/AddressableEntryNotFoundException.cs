using System;

namespace UnityEditor.Localization
{
    /// <summary>
    /// This exception is thrown when an asset is expected to be part of the Addressables system.
    /// The Localization system uses Addressables for loading Locales, Tables and Table Assets.
    /// </summary>
    internal class AddressableEntryNotFoundException : Exception
    {
        public AddressableEntryNotFoundException(UnityEngine.Object target) :
            base($"{target.name} could not an Addressable asset.")
        {
        }
    }
}