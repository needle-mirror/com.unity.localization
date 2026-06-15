#if MODULE_UITK && UNITY_2023_3_OR_NEWER

using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEngine.Localization
{
    [UxmlObject]
    public partial class LocalizedStringGroup : CustomBinding
    {
        int m_ActivatedCount;

        /// <summary>Creates a new <see cref="LocalizedStringGroup"/>.</summary>
        public LocalizedStringGroup()
        {
            updateTrigger = BindingUpdateTrigger.WhenDirty;
        }

        [UxmlObjectReference("strings")]
        internal List<LocalizedString> StringsUXML
        {
            get => Strings;
            set => Strings = value;
        }

        /// <inheritdoc/>
        protected override void OnActivated(in BindingActivationContext context)
        {
            base.OnActivated(context);
            if (++m_ActivatedCount == 1)
                ListChanged += OnListChangedForBinding;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated(in BindingActivationContext context)
        {
            base.OnDeactivated(context);
            if (--m_ActivatedCount == 0)
                ListChanged -= OnListChangedForBinding;
        }

        void OnListChangedForBinding(List<string> _) => MarkDirty();

        /// <inheritdoc/>
        protected override BindingResult Update(in BindingContext context)
        {
            if (Strings == null || Strings.Count == 0)
            {
                var emptyElement = context.targetElement;
                ConverterGroups.TrySetValueGlobal(ref emptyElement, context.bindingId, new List<string>(), out _);
                return new BindingResult(BindingStatus.Success);
            }

            #if UNITY_EDITOR
            foreach (var entry in Strings)
            {
                if (entry == null)
                    continue;
                if (!entry.IsEmpty && !PlaymodeState.IsPlaying && entry.LocaleOverride == null &&
                    LocalizationSettings.SelectedLocale == null)
                    entry.LocaleOverride = LocalizationSettings.ProjectLocale;
            }
            #endif

            foreach (var entry in Strings)
            {
                if (entry == null)
                    continue;
                if (!entry.IsEmpty && !entry.CurrentLoadingOperationHandle.IsDone)
                    return new BindingResult(BindingStatus.Pending);
            }

            var result = GetCurrentList();
            var element = context.targetElement;
            if (ConverterGroups.TrySetValueGlobal(ref element, context.bindingId, result, out var errorCode))
                return new BindingResult(BindingStatus.Success);
            return LocalizedReference.CreateErrorResult(context, errorCode, typeof(LocalizedStringGroup), typeof(List<string>));
        }
    }
}

#endif
