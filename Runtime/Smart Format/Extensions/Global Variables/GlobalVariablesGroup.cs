using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Localization.SmartFormat.GlobalVariables
{
    [DisplayName("Group")]
    public class NestedGlobalVariablesGroup : GlobalVariable<GlobalVariablesGroup>, IVariableGroup
    {
        public bool TryGetValue(string name, out IGlobalVariable value)
        {
            if (Value != null)
                return Value.TryGetValue(name, out value);
            value = default;
            return false;
        }
    }

    [CreateAssetMenu(menuName = "Localization/Global Variables Group")]
    public class GlobalVariablesGroup : ScriptableObject, IVariableGroup, IGlobalVariable, IDictionary<string, IGlobalVariable>, ISerializationCallbackReceiver
    {
        [Serializable]
        internal class NameValuePair
        {
            public string name;

            [SerializeReference]
            public IGlobalVariable variable;
        }

        [SerializeField]
        internal List<NameValuePair> m_Variables = new List<NameValuePair>();

        Dictionary<string, NameValuePair> m_VariableLookup = new Dictionary<string, NameValuePair>();

        public object SourceValue => this;

        public int Count => m_VariableLookup.Count;

        public ICollection<string> Keys => m_VariableLookup.Keys;

        public ICollection<IGlobalVariable> Values => m_VariableLookup.Values.Select(s => s.variable).ToList();

        public bool IsReadOnly => false;

        public IGlobalVariable this[string name]
        {
            get => m_VariableLookup[name].variable;
            set => Add(name, value);
        }

        public bool TryGetValue(string name, out IGlobalVariable value)
        {
            if (m_VariableLookup.TryGetValue(name, out var v))
            {
                value = v.variable;
                return true;
            }

            value = default;
            return false;
        }

        public void Add(string name, IGlobalVariable variable)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name), "Name must not be null or empty.");
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            name = name.ReplaceWhiteSpaces("-");
            var v = new NameValuePair { name = name, variable = variable };
            m_VariableLookup.Add(name, v);
            m_Variables.Add(v);
        }

        public void Add(KeyValuePair<string, IGlobalVariable> item) => Add(item.Key, item.Value);

        public bool Remove(string name)
        {
            if (m_VariableLookup.TryGetValue(name, out var v))
            {
                m_Variables.Remove(v);
                m_VariableLookup.Remove(name);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<string, IGlobalVariable> item) => Remove(item.Key);

        public bool ContainsKey(string name) => m_VariableLookup.ContainsKey(name);

        public bool Contains(KeyValuePair<string, IGlobalVariable> item) => TryGetValue(item.Key, out var v) && v == item.Value;

        public void CopyTo(KeyValuePair<string, IGlobalVariable>[] array, int arrayIndex)
        {
            foreach (var entry in m_VariableLookup)
            {
                array[arrayIndex++] = new KeyValuePair<string, IGlobalVariable>(entry.Key, entry.Value.variable);
            }
        }

        IEnumerator<KeyValuePair<string, IGlobalVariable>> IEnumerable<KeyValuePair<string, IGlobalVariable>>.GetEnumerator()
        {
            foreach (var v in m_VariableLookup)
            {
                yield return new KeyValuePair<string, IGlobalVariable>(v.Key, v.Value.variable);
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var v in m_VariableLookup)
            {
                yield return new KeyValuePair<string, IGlobalVariable>(v.Key, v.Value.variable);
            }
        }

        public bool ContainsName(string name) => m_VariableLookup.ContainsKey(name);

        public void Clear()
        {
            m_VariableLookup.Clear();
            m_Variables.Clear();
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            if (m_VariableLookup == null)
                m_VariableLookup = new Dictionary<string, NameValuePair>();

            m_VariableLookup.Clear();
            foreach (var v in m_Variables)
            {
                if (!string.IsNullOrEmpty(v.name))
                {
                    m_VariableLookup[v.name] = v;
                }
            }
        }
    }
}
