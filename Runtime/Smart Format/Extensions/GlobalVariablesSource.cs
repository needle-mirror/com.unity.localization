using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    /// <summary>
    /// Can be used to provide global values that do not need to be passed in as arguments when formatting a string.
    /// The smart string should take the format {groupName.variableName}. e.g {global.player-score}.
    /// Note: The group name and variable names must not contain any spaces.
    /// </summary>
    [Serializable]
    public class GlobalVariablesSource : ISource, IDictionary<string, GlobalVariablesGroup>, ISerializationCallbackReceiver
    {
        [Serializable]
        class NameValuePair
        {
            public string name;

            [SerializeReference]
            public GlobalVariablesGroup group;
        }

        /// <summary>
        /// Encapsulates a <see cref="BeginUpdating"/> and <see cref="EndUpdating"/> call.
        /// </summary>
        public struct GlobalVariablesScopedUpdate : IDisposable
        {
            public void Dispose() => EndUpdating();
        }

        [SerializeField]
        List<NameValuePair> m_Groups = new List<NameValuePair>();

        Dictionary<string, NameValuePair> m_GroupLookup = new Dictionary<string, NameValuePair>();

        internal static int s_IsUpdating;

        /// <summary>
        /// Has <see cref="BeginUpdating"/> been called?
        /// This can be used when updating the value of multiple <see cref="IGlobalVariable"/> in order to do
        /// a single update after the updates instead of 1 per change.
        /// </summary>
        public static bool IsUpdating => s_IsUpdating != 0;

        public int Count => m_Groups.Count;

        public bool IsReadOnly => false;

        public ICollection<string> Keys => m_GroupLookup.Keys;

        public ICollection<GlobalVariablesGroup> Values => m_GroupLookup.Values.Select(k => k.group).ToList();

        public GlobalVariablesGroup this[string name]
        {
            get => m_GroupLookup[name].group;
            set => Add(name, value);
        }

        /// <summary>
        /// Called after the final <see cref="EndUpdating"/> has been called.
        /// This can be used when you wish to respond to value change events but wish to do a
        /// single update at the end instead of 1 per change.
        /// For example, if you wanted to change the value of multiple global variables
        /// that a smart string was using then changing each value would result in a new string
        /// being generated, by using begin and end  the string generation can be deferred until the
        /// final change so that only 1 update is performed.
        /// </summary>
        public static event Action EndUpdate;

        /// <summary>
        /// Creates a new <see cref="GlobalVariablesGroup"/> instance and adds the "." operator to the parser.
        /// </summary>
        /// <param name="formatter"></param>
        public GlobalVariablesSource(SmartFormatter formatter)
        {
            formatter.Parser.AddOperators(".");
        }

        /// <summary>
        /// Indicates that multiple <see cref="IGlobalVariable"/> will be changed and <see cref="LocalizedString"/> should wait for <see cref="EndUpdate"/> before updating.
        /// See <seealso cref="EndUpdating"/> and <seealso cref="EndUpdate"/>.
        /// Note: <see cref="BeginUpdating"/> and <see cref="EndUpdating"/> can be nested, <see cref="EndUpdate"/> will only be called after the last <see cref="EndUpdate"/>.
        /// </summary>
        public static void BeginUpdating() => s_IsUpdating++;

        /// <summary>
        /// Indicates that updates to <see cref="IGlobalVariable"/> have finished and sends the <see cref="EndUpdate"/> event.
        /// Note: <see cref="BeginUpdating"/> and <see cref="EndUpdating"/> can be nested, <see cref="EndUpdate"/> will only be called after the last <see cref="EndUpdate"/>.
        /// </summary>
        public static void EndUpdating()
        {
            s_IsUpdating--;
            if (s_IsUpdating == 0)
            {
                EndUpdate?.Invoke();
            }
            else if (s_IsUpdating < 0)
            {
                Debug.LogWarning($"Incorrect number of Begin and End calls to {nameof(GlobalVariablesSource)}. {nameof(BeginUpdating)} must be called before {nameof(EndUpdating)}.");
                s_IsUpdating = 0;
            }
        }

        /// <summary>
        /// Can be used to create a <see cref="BeginUpdating"/> and <see cref="EndUpdating"/> scope.
        /// </summary>
        /// <returns></returns>
        public static IDisposable UpdateScope()
        {
            BeginUpdating();
            return new GlobalVariablesScopedUpdate();
        }

        public bool TryGetValue(string name, out GlobalVariablesGroup value)
        {
            if (m_GroupLookup.TryGetValue(name, out var v))
            {
                value = v.group;
                return true;
            }
            value = null;
            return false;
        }

        public void Add(string name, GlobalVariablesGroup group)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name), "Name must not be null or empty.");
            if (group == null)
                throw new ArgumentNullException(nameof(group));
            var pair = new NameValuePair { name = name, group = group };

            name = name.ReplaceWhiteSpaces("-");
            m_GroupLookup[name] = pair;
            m_Groups.Add(pair);
        }

        public void Add(KeyValuePair<string, GlobalVariablesGroup> item) => Add(item.Key, item.Value);

        public bool Remove(string name)
        {
            if (m_GroupLookup.TryGetValue(name, out var v))
            {
                m_Groups.Remove(v);
                m_GroupLookup.Remove(name);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<string, GlobalVariablesGroup> item) => Remove(item.Key);

        public void Clear()
        {
            m_GroupLookup.Clear();
            m_Groups.Clear();
        }

        public bool ContainsKey(string name) => m_GroupLookup.ContainsKey(name);

        public bool Contains(KeyValuePair<string, GlobalVariablesGroup> item) => TryGetValue(item.Key, out var v) && v == item.Value;

        public void CopyTo(KeyValuePair<string, GlobalVariablesGroup>[] array, int arrayIndex)
        {
            foreach (var entry in m_GroupLookup)
            {
                array[arrayIndex++] = new KeyValuePair<string, GlobalVariablesGroup>(entry.Key, entry.Value.group);
            }
        }

        IEnumerator<KeyValuePair<string, GlobalVariablesGroup>> IEnumerable<KeyValuePair<string, GlobalVariablesGroup>>.GetEnumerator()
        {
            foreach (var v in m_GroupLookup)
            {
                yield return new KeyValuePair<string, GlobalVariablesGroup>(v.Key, v.Value.group);
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var v in m_GroupLookup)
            {
                yield return new KeyValuePair<string, GlobalVariablesGroup>(v.Key, v.Value.group);
            }
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var selector = selectorInfo.SelectorText;

            // Are we dealing with nested groups?
            if (selectorInfo.CurrentValue is IVariableGroup g && selectorInfo.SelectorOperator == "." && g.TryGetValue(selector, out var gv))
            {
                // Add the variable to the cache
                var cache = selectorInfo.FormatDetails.FormatCache;
                if (cache != null && gv is IGlobalVariableValueChanged valueChanged)
                {
                    if (!cache.GlobalVariableTriggers.Contains(valueChanged))
                        cache.GlobalVariableTriggers.Add(valueChanged);
                }

                selectorInfo.Result = gv.SourceValue;
                return true;
            }

            if (TryGetValue(selector, out var group))
            {
                selectorInfo.Result = group;
                return true;
            }

            return false;
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            if (m_GroupLookup == null)
                m_GroupLookup = new Dictionary<string, NameValuePair>();

            m_GroupLookup.Clear();
            foreach (var v in m_Groups)
            {
                if (!string.IsNullOrEmpty(v.name))
                {
                    m_GroupLookup[v.name] = v;
                }
            }
        }
    }
}
