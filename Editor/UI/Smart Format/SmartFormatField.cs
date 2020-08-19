using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class SmartFormatFieldInfoWindow : PopupWindowContent
    {
        Placeholder m_Contents;

        public SmartFormatFieldInfoWindow(Placeholder ph)
        {
            m_Contents = ph;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 250);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.HelpBox(rect, string.Empty, MessageType.None);

            rect.xMin += 3; // margin;
            rect.yMin += 3;

            EditorGUILayout.HelpBox("Placeholder", MessageType.Info);

            EditorGUILayout.LabelField("Nested Depth", m_Contents.NestedDepth.ToString());

            EditorGUILayout.LabelField("Alignment", m_Contents.Alignment.ToString());

            if (!string.IsNullOrEmpty(m_Contents.FormatterName))
            {
                EditorGUILayout.LabelField("Formatter Name", m_Contents.FormatterName);
                EditorGUILayout.LabelField("Formatter Options", m_Contents.FormatterOptions);
            }

            if (m_Contents.Selectors.Count > 0)
            {
                EditorGUILayout.LabelField("Selectors");
                foreach (var selector in m_Contents.Selectors)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("• " + selector.RawText);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }

    class SmartFormatField
    {
        /// <summary>
        /// Is the field plain text or using Smart Format?
        /// </summary>
        public bool IsSmart
        {
            get => m_IsSmart;
            set
            {
                m_Mode = Mode.Edit;
                m_IsSmart = value;
            }
        }

        /// <summary>
        /// The table that this field is part of.
        /// </summary>
        public StringTable Table { get; set; }

        /// <summary>
        /// Key id for the table entry.
        /// </summary>
        public long KeyId { get; set; }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// The value we store inside the table. This could be a SmartFormat value or just plain text.
        /// </summary>
        public string RawText
        {
            get => m_RawText;
            set
            {
                if (m_RawText == value)
                    return;

                m_RawText = value;
                ResetCache();
            }
        }

        /// <summary>
        /// Single line version of Raw text for showing in a label
        /// </summary>
        public string Label
        {
            get
            {
                if (m_Label == null)
                {
                    if (string.IsNullOrEmpty(RawText))
                    {
                        m_Label = "<empty>";
                    }
                    else
                    {
                        var idx = RawText.IndexOf('\n');
                        m_Label = idx != -1 ? RawText.Substring(0, idx) : RawText;
                    }
                }
                return m_Label;
            }
        }

        /// <summary>
        /// Show the metadata edit button
        /// </summary>
        public bool ShowMetadataButton { get; set; } = true;

        /// <summary>
        /// Show an additional tab for previewing the value using scene assets.
        /// </summary>
        public bool ShowPreviewTab { get; set; } = false;

        public float MinHeight { get; set; } = 100;

        /// <summary>
        /// Debug text is the RawText with hyper links inserted for Smart Format fields, we provide info when the links are clicked on.
        /// </summary>
        string DebugText
        {
            get
            {
                if (m_DebugText == null)
                {
                    m_Format = m_SmartFormatter.Parser.ParseFormat(RawText, m_SmartFormatter.GetNotEmptyFormatterExtensionNames());
                    m_FormatItemLookup.Clear();
                    int id = 0;
                    m_DebugText = FormatToDebugString(m_Format, ref id);
                }
                return m_DebugText;
            }
        }

        string PreviewText
        {
            get
            {
                if (m_PreviewText == null)
                    m_PreviewText = m_SmartFormatter?.Format(RawText, Arguments);
                return m_PreviewText;
            }
        }

        /// <summary>
        /// Optional arguments that will be passed through when using SmartFormat.
        /// </summary>
        public SmartObjects Arguments { get; set; } = new SmartObjects();
        public bool Selected { get; set; }

        static readonly string[] k_Modes = {"Edit", "Debug"};
        static readonly string[] k_ModesWithPreview = {"Edit", "Debug", "Preview"};
        static readonly GUIContent k_MetadataIcon = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.unity.localization/Editor/Icons/Localization_AssetTable.png"), "Edit Table Entry Metadata");

        const float k_ToolbarHeight = 20;

        enum Mode
        {
            Edit,
            Debug,
            Preview
        }

        Mode m_Mode = Mode.Edit;
        string m_DebugText = null;
        string m_PreviewText = null;
        string m_Label = null;
        string m_RawText;
        bool m_IsSmart;

        SmartFormatter m_SmartFormatter;
        Format m_Format;

        TableEntrySelected m_TableEntrySelected;

        Dictionary<string, FormatItem> m_FormatItemLookup = new Dictionary<string, FormatItem>();

        public SmartFormatField()
        {
            m_SmartFormatter = LocalizationEditorSettings.ActiveLocalizationSettings?.GetStringDatabase()?.SmartFormatter;
            // TODO: Only do this when in debug view.
            SubscribeToHyperlinkEvent();
        }

        /// <summary>
        /// Read the latest data for this key entry from the table. Called when the data we have may no longer be correct, such as after an Undo event.
        /// </summary>
        public void RefreshData()
        {
            ResetCache();
            var entry = Table.GetEntry(KeyId);
            if (entry != null)
            {
                m_IsSmart = entry.IsSmart;
                m_RawText = entry.Value;
            }
            else
            {
                m_IsSmart = false;
                RawText = string.Empty;
            }
            CalcHeight();
        }

        void CalcHeight()
        {
            var contentsHeight = EditorStyles.textArea.CalcSize(new GUIContent(RawText)).y + EditorGUIUtility.standardVerticalSpacing;
            contentsHeight += k_ToolbarHeight + EditorGUIUtility.standardVerticalSpacing;
            if (IsSmart)
                contentsHeight += EditorStyles.toolbarButton.lineHeight + EditorGUIUtility.standardVerticalSpacing;
            Height = Mathf.Max(MinHeight, contentsHeight);
        }

        public void ResetCache()
        {
            m_DebugText = null;
            m_PreviewText = null;
            m_Format = null;
            m_Label = null;
        }

        /// <summary>
        /// Subscribe to the hyperLinkClicked event, this lets us click on text elements and provide debug info for them.
        /// </summary>
        void SubscribeToHyperlinkEvent()
        {
            var hyperLinkClicked = typeof(EditorGUI).GetEvent("hyperLinkClicked", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            var callback = GetType().GetMethod("EditorGUI_HyperLinkClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var callbackDelegate = Delegate.CreateDelegate(hyperLinkClicked.EventHandlerType, this, callback);

            var addHandler = hyperLinkClicked.GetAddMethod(true);
            object[] args = { callbackDelegate };
            addHandler.Invoke(null, args);
        }

        /// <summary>
        /// Called when a hyper link is clicked in Debug mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditorGUI_HyperLinkClicked(object sender, EventArgs e)
        {
            var o = e.GetType().GetProperty("hyperlinkInfos").GetValue(e, null);
            if (o is Dictionary<string, string> dict)
            {
                if (dict.TryGetValue("href", out var formatId) && m_FormatItemLookup.TryGetValue(formatId, out var formatItem))
                {
                    if (formatItem is Placeholder ph)
                    {
                        var mousePos =  Event.current.mousePosition;
                        mousePos.y -= EditorWindow.mouseOverWindow.position.height * 0.5f;
                        mousePos.y += 80;

                        // relative to the window
                        var rect = new Rect(mousePos.x, mousePos.y, 200, 200);
                        PopupWindow.Show(rect, new SmartFormatFieldInfoWindow(ph));
                    }
                    else
                    {
                        Debug.Log("Cant show " + formatItem.GetType());
                    }
                }
            }
        }

        /// <summary>
        /// Draws the field including the toolbar.
        /// </summary>
        /// <param name="rect"></param>
        public bool Draw(Rect rect)
        {
            const float metaDataButtonWidth = 25;
            bool change = false;

            rect.yMin += EditorGUIUtility.standardVerticalSpacing;

            if (ShowMetadataButton)
            {
                var btnRect = new Rect(rect.x, rect.y, metaDataButtonWidth, k_ToolbarHeight);
                EditorGUI.BeginChangeCheck();
                GUI.Toggle(btnRect, Selected, k_MetadataIcon, GUI.skin.button);
                if (EditorGUI.EndChangeCheck())
                {
                    Selected = !Selected;
                }
            }

            EditorGUI.BeginChangeCheck();

            var smartFormatFieldWidth = rect.width - (ShowMetadataButton ? metaDataButtonWidth + 2 : 0);
            var smartFormatRect = new Rect(rect.x + (ShowMetadataButton ? metaDataButtonWidth + 2 : 0), rect.y, smartFormatFieldWidth, k_ToolbarHeight);
            smartFormatRect = EditorGUI.PrefixLabel(smartFormatRect, GUIContent.none);
            var newIsSmart = GUI.Toggle(smartFormatRect, IsSmart, "Smart");
            if (EditorGUI.EndChangeCheck())
            {
                SetIsSmart(newIsSmart);
                change = true;
            }

            rect.yMin += k_ToolbarHeight + EditorGUIUtility.standardVerticalSpacing;

            if (IsSmart)
            {
                var header = new Rect(rect.x, rect.y, rect.width, EditorStyles.toolbarButton.lineHeight);
                header = EditorGUI.PrefixLabel(header, GUIContent.none);
                var choices =  ShowPreviewTab ? k_ModesWithPreview : k_Modes;
                m_Mode = (Mode)GUI.SelectionGrid(header, (int)m_Mode, choices, choices.Length, EditorStyles.miniButtonMid);
                rect.yMin += header.height + EditorGUIUtility.standardVerticalSpacing;
            }

            switch (m_Mode)
            {
                case Mode.Edit:
                    EditorGUI.BeginChangeCheck();
                    var newText = EditorGUI.TextArea(rect, RawText);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetValue(newText);
                        change = true;
                    }
                    break;

                case Mode.Debug:
                    EditorStyles.textArea.richText = true;
                    EditorStyles.textArea.wordWrap = false;
                    EditorGUI.SelectableLabel(rect, DebugText, EditorStyles.textArea);
                    break;

                case Mode.Preview:
                    // TODO: List of references and the results.
                    EditorGUI.LabelField(rect, PreviewText, EditorStyles.textArea);
                    break;
            }
            return change;
        }

        StringTableEntry GetOrCreateEntry()
        {
            return Table.GetEntry(KeyId) ?? Table.AddEntry(KeyId, string.Empty);
        }

        internal void SetIsSmart(bool value)
        {
            Undo.RecordObject(Table, "Set smart format");

            // This is required as Undo does not make assets dirty
            EditorUtility.SetDirty(Table);

            var entry = GetOrCreateEntry();
            entry.IsSmart = value;
            IsSmart = value;
            CalcHeight();
        }

        internal void SetValue(string value)
        {
            Undo.RecordObject(Table, "Set smart format");

            // This is required as Undo does not make assets dirty
            EditorUtility.SetDirty(Table);

            var entry = GetOrCreateEntry();
            entry.Value = value;
            RawText = value;
            CalcHeight();
        }

        /// <summary>
        /// A debug string is a rich text string that uses hyper links to allow for clicking and inspecting smart format fields.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="nextId"></param>
        /// <returns></returns>
        string FormatToDebugString(Format format, ref int nextId)
        {
            var result = new StringBuilder();

            foreach (var item in format.Items)
            {
                if (item is Placeholder ph)
                {
                    var id = $"{Table.GetInstanceID()}-{KeyId}-{nextId++}";
                    m_FormatItemLookup.Add(id, ph);
                    result.Append($"<a href=\"{id}\">"); // TODO: Support Different selectors from Settings
                    result.Append("<b>");
                    result.Append('{');
                    result.Append("</b>");
                    foreach (var s in ph.Selectors)
                    {
                        result.Append("<b><color=#AE6503>");
                        result.Append(s.baseString, s.operatorStart, s.endIndex - s.operatorStart);
                        result.Append("</color></b>");
                    }
                    if (ph.Alignment != 0)
                    {
                        result.Append("<b>");
                        result.Append(',');
                        result.Append(ph.Alignment);
                        result.Append("</b>");
                    }

                    if (ph.FormatterName != "")
                    {
                        result.Append(':');
                        result.Append("<color=#937D39>");
                        result.Append(ph.FormatterName);
                        if (ph.FormatterOptions != "")
                        {
                            result.Append('(');
                            result.Append(ph.FormatterOptions);
                            result.Append(')');
                        }
                        result.Append("</color>");
                    }

                    if (ph.Format != null)
                    {
                        result.Append(':');
                        result.Append(FormatToDebugString(ph.Format, ref nextId));
                    }

                    result.Append("<b>");
                    result.Append('}');
                    result.Append("</b>");
                    result.Append("</a>");
                }
                else
                {
                    result.Append(item.RawText);
                }
            }

            return result.ToString();
        }

        public override string ToString() => $"SmartFormatField({RawText})";
    }
}
