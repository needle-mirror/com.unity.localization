using System;
using System.Collections.Generic;
using UnityEngine.Localization.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Use this component for populating localized dropdown choices.
    /// </summary>
    /// <remarks>
    /// This component triggers events for a localized list of strings.
    /// The <see cref="ListReference"/> can be either a <see cref="LocalizedStringList"/> (a single delimited entry split into items)
    /// or a <see cref="LocalizedStringGroup"/> (multiple individual <see cref="LocalizedString"/> entries aggregated into a list).
    /// Hook <see cref="OnUpdateList"/> up to <c>Dropdown.AddOptions</c> or <c>TMP_Dropdown.AddOptions</c> in the inspector.
    /// </remarks>
    [AddComponentMenu("Localization/Localize String List Event")]
    public class LocalizeStringListEvent : LocalizedMonoBehaviour
    {
        [SerializeReference]
        ILocalizedStringList m_ListReference = new LocalizedStringList();

        [SerializeField]
        UnityEventStringList m_UpdateList = new UnityEventStringList();

        LocalizedListChangeHandler m_ChangeHandler;

        /// <summary>
        /// The localized list source.
        /// </summary>
        /// <remarks>
        /// The type must be <see cref="LocalizedStringList"/> or <see cref="LocalizedStringGroup"/>.
        /// </remarks>
        public ILocalizedStringList ListReference
        {
            get => m_ListReference;
            set
            {
                ClearChangeHandler();
                m_ListReference = value;
                if (isActiveAndEnabled)
                    RegisterChangeHandler();
            }
        }

        /// <summary>
        /// Event invoked when the localized list is available or changes.
        /// </summary>
        public UnityEventStringList OnUpdateList
        {
            get => m_UpdateList;
            set => m_UpdateList = value;
        }

        void OnEnable()
        {
            #if UNITY_EDITOR
            if (PlaymodeState.IsChangingPlayMode)
                return;
            #endif
            RegisterChangeHandler();
        }

        void OnDisable() => ClearChangeHandler();

        void OnDestroy() => ClearChangeHandler();

        void UpdateList(List<string> value)
        {
            #if UNITY_EDITOR
            if (!PlaymodeState.IsPlayingOrWillChangePlaymode)
            {
                if (m_ListReference == null)
                {
                    Editor_UnregisterKnownDrivenProperties(OnUpdateList);
                    return;
                }

                Editor_RegisterKnownDrivenProperties(OnUpdateList);
                OnUpdateList.Invoke(value);
                Editor_RefreshEventObjects(OnUpdateList);
            }
            else
            #endif
            {
                OnUpdateList.Invoke(value);
            }
        }

        void RegisterChangeHandler()
        {
            if (m_ListReference == null)
                return;

            if (m_ChangeHandler == null)
                m_ChangeHandler = UpdateList;

            m_ListReference.ListChanged += m_ChangeHandler;
        }

        void ClearChangeHandler()
        {
            #if UNITY_EDITOR
            Editor_UnregisterKnownDrivenProperties(OnUpdateList);
            #endif

            if (m_ListReference != null && m_ChangeHandler != null)
                m_ListReference.ListChanged -= m_ChangeHandler;
        }
    }
}
