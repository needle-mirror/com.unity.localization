using System;
using System.Linq;
using System.Collections.ObjectModel;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    public class StringTableCollection : LocalizedTableCollection
    {
        readonly Type kTableType = typeof(StringTable);
        protected internal override Type TableType => kTableType;

        protected internal override Type RequiredExtensionAttribute => typeof(StringTableCollectionExtensionAttribute);

        public override string DefaultTableGroupName => "Localization-StringTables";

        /// <summary>
        /// A helper property which is the contents of <see cref="Tables"/> loaded and cast to <see cref="StringTable"/>.
        /// </summary>
        public virtual ReadOnlyCollection<StringTable> StringTables => new ReadOnlyCollection<StringTable>(Tables.Select(t => t.asset as StringTable).ToList().AsReadOnly());
    }
}
