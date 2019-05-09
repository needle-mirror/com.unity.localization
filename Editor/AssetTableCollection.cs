using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Asset tables are collated by their type and table name
    /// </summary>
    public class AssetTableCollection : IEquatable<AssetTableCollection>
    {
        LocalizedTableEditor m_Editor;

        public Type TableType { get; set; }

        public Type AssetType
        {
            get
            {
                var assetTable = Tables[0] as LocalizedAssetTable;
                return assetTable != null ? assetTable.SupportedAssetType : null;
            }
        }

        public LocalizedTableEditor TableEditor
        {
            get
            {
                if (m_Editor == null)
                {
                    UnityEditor.Editor editor = null;
                    UnityEditor.Editor.CreateCachedEditor(Tables.ToArray(), null, ref editor);
                    Debug.Assert(editor != null);
                    m_Editor = editor as LocalizedTableEditor;
                }
                return m_Editor;
            }
        }

        public virtual string TableName => Tables[0].TableName;

        public List<LocalizedTable> Tables { get; set; } = new List<LocalizedTable>();

        public KeyDatabase Keys { get; set; }

        public override string ToString() => TableName + "("+ TableType.Name  + ")";

        public bool Equals(AssetTableCollection other)
        {
            if (other == null)
                return false;
            return TableType == other.TableType && TableName == other.TableName;
        }
    }
}
