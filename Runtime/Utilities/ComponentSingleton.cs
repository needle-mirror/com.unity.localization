using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Localization
{
    [ExecuteInEditMode]
    abstract class ComponentSingleton<T> : MonoBehaviour where T : ComponentSingleton<T>
    {
        static T s_Instance;

        public static bool Exists => s_Instance != null;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = FindInstance() ?? CreateNewSingleton();

                return s_Instance;
            }
        }

        static T FindInstance()
        {
#if UNITY_EDITOR
            foreach (T cb in Resources.FindObjectsOfTypeAll(typeof(T)).Cast<T>())
            {
                var go = cb.gameObject;
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    return cb;
            }

            return null;
#elif UNITY_2022_2_OR_NEWER
            return FindFirstObjectByType<T>();
#else
            return FindObjectOfType<T>();
#endif
        }

        protected virtual string GetGameObjectName() => typeof(T).Name;

        static T CreateNewSingleton()
        {
            var go = new GameObject();

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.DontSave;
            }
            else
            {
                go.hideFlags = HideFlags.HideAndDontSave;
            }

            var instance = go.AddComponent<T>();
            go.name = instance.GetGameObjectName();
            return instance;
        }

        void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            s_Instance = this as T;
        }

        public static void DestroySingleton()
        {
            if (Exists)
            {
                DestroyImmediate(Instance.gameObject);
                s_Instance = null;
            }
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeChanged;
        }

        void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode && Exists)
            {
                DestroyImmediate(Instance.gameObject);
                s_Instance = null;
            }
        }
#endif
    }
}
