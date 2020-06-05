using System;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Indicates a custom name to be used when displayed in a list in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; set; }

        public DisplayNameAttribute(string name) => Name = name;
    }
}
