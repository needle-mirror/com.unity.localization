#if MODULE_UITK && ENABLE_UITK_DATA_BINDING

using UnityEditor.UIElements;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    class TableReferenceConverter : UxmlAttributeConverter<TableReference>
    {
        public override TableReference FromString(string value) => TableReference.TableReferenceFromString(value);
        public override string ToString(TableReference value) => value.GetSerializedString();
    }
}

#endif
