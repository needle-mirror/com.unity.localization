#if MODULE_UITK && UNITY_2023_3_OR_NEWER

using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEngine.Localization
{
    [UxmlObject]
    public partial class LocalizedStringList
    {
        [UxmlAttribute("separator")]
        internal string SeparatorUXML
        {
            get => Separator;
            set => Separator = value;
        }

        protected override void Initialize() => ListChanged += OnListChangedForBinding;

        protected override void Cleanup() => ListChanged -= OnListChangedForBinding;

        void OnListChangedForBinding(List<string> _) => MarkDirty();

        /// <inheritdoc/>
        protected override BindingResult Update(in BindingContext context)
        {
            if (IsEmpty)
                return new BindingResult(BindingStatus.Success);

            #if UNITY_EDITOR
            if (!PlaymodeState.IsPlaying && LocaleOverride == null && LocalizationSettings.SelectedLocale == null)
                LocaleOverride = LocalizationSettings.ProjectLocale;
            #endif

            if (!CurrentLoadingOperationHandle.IsDone)
                return new BindingResult(BindingStatus.Pending);

            var items = CurrentList ?? new List<string>();

            var element = context.targetElement;
            if (ConverterGroups.TrySetValueGlobal(ref element, context.bindingId, items, out var errorCode))
                return new BindingResult(BindingStatus.Success);
            return CreateErrorResult(context, errorCode, typeof(List<string>));
        }
    }
}

#endif
