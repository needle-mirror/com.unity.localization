//namespace UnityEngine.Localization.Metadata
//{
//    /// <summary>
//    /// TODO: DOC
//    /// </summary>
//    /// <typeparam name="TComponent"></typeparam>
//    public abstract class JSonMetadata<T> : ScriptableObject
//    {
//        [SerializeField]
//        string m_Json;

//        /// <summary>
//        /// TODO: DOC
//        /// </summary>
//        /// <param name="obj"></param>
//        /// <param name="prettyPrint"></param>
//        public virtual void RecordData(T obj, bool prettyPrint = false)
//        {
//            m_Json = JsonUtility.ToJson(obj, prettyPrint);
//        }

//        /// <summary>
//        /// TODO: DOC
//        /// </summary>
//        /// <returns></returns>
//        public T FromJson()
//        {
//            return JsonUtility.FromJson<T>(m_Json);
//        }

//        /// <summary>
//        /// TODO: DOC
//        /// </summary>
//        /// <param name="target"></param>
//        public virtual void Overrwrite(T target)
//        {
//            if (string.IsNullOrEmpty(m_Json))
//            {
//                Debug.LogWarning($"{this} Could not overwrite target{target}, no Json data exists for type {typeof(T).Name}", this);
//            }
//            else
//            {
//                JsonUtility.FromJsonOverwrite(m_Json, target);
//            }
//        }
//    }
//}
