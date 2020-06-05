using System;

namespace UnityEngine.Localization.Metadata
{
    /// <summary>
    /// Flags to indicate where the Metadata can be added.
    /// </summary>
    [Flags]
    public enum MetadataType
    {
        /// <summary>
        /// Metadata can be added to a <see cref="Locale"/>.
        /// </summary>
        Locale = 1,

        /// <summary>
        /// Metadata can be added to a <see cref="SharedTableData"/>.
        /// </summary>
        SharedTableData = 2,

        /// <summary>
        /// Metadata can be added to a <see cref="StringTable"/>.
        /// </summary>
        StringTable = 4,

        /// <summary>
        /// Metadata can be added to a <see cref="AssetTable"/>.
        /// </summary>
        AssetTable = 8,

        /// <summary>
        /// Metadata can be added to a <see cref="StringTable"/> entry.
        /// </summary>
        StringTableEntry = 16,

        /// <summary>
        /// Metadata can be added to a <see cref="AssetTable"/> entry.
        /// </summary>
        AssetTableEntry = 32,

        /// <summary>
        /// Metadata can be added to any table type.
        /// </summary>
        AllTables = StringTable | AssetTable,

        /// <summary>
        /// Metadata can be added to any table entry.
        /// </summary>
        AllTableEntries = StringTableEntry | AssetTableEntry,

        /// <summary>
        /// Metadata can be added to anything.
        /// </summary>
        All = AllTables | AllTableEntries | Locale | SharedTableData
    }


    /// <summary>
    /// Used to indicate the type of the Metadata that can be added.
    /// <seealso cref="MetadataAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    class MetadataTypeAttribute : PropertyAttribute
    {
        /// <summary>
        /// <inheritdoc cref="MetadataType"/>
        /// </summary>
        public MetadataType Type { get; set; }

        /// <summary>
        /// <inheritdoc cref="MetadataTypeAttribute"/>
        /// </summary>
        /// <param name="type"></param>
        public MetadataTypeAttribute(MetadataType type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Provides a hint to the editor on where this metadata can be used.
    /// This is only used in the editor and ignored when adding Metadata through script.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MetadataAttribute : Attribute
    {
        /// <summary>
        /// Name to use in the add Metadata menu. If empty then the class name will be used.
        /// </summary>
        public string MenuItem { get; set; }

        /// <summary>
        /// Can multiple instances be added?
        /// </summary>
        public bool AllowMultiple { get; set; } = true;

        /// <summary>
        /// The types that the Metadata is meant for. This is used when generating the add Metadata menu.
        /// </summary>
        public MetadataType AllowedTypes { get; set; } = MetadataType.All;
    }
}
