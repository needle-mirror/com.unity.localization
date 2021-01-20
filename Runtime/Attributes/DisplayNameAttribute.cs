using System;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Specify a custom name to be used when displayed in the Editor.
    /// </summary>
    /// <example>
    /// This example shows how Metadata can be given a custom name.
    /// <code source="../DocCodeSamples.Tests/DisplayNameAttributeSample.cs"/>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        /// <summary>
        /// The custom name to use when displayed in the Editor.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specify a custom name to be used when displayed in the Editor.
        /// </summary>
        public DisplayNameAttribute(string name) => Name = name;
    }
}
