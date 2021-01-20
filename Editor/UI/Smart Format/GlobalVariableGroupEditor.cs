using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(GlobalVariablesGroup), true)]
    class GlobalVariableGroupEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() => new GlobalVariableGroupList(serializedObject.FindProperty("m_Variables"), typeof(IGlobalVariable));
    }
}
