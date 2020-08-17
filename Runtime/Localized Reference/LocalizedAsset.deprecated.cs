#if UNITY_EDITOR // We only need the original type in editor.

using System;

namespace UnityEngine.Localization
{
    public partial class LocalizedAsset<TObject> : LocalizedReference where TObject : Object
    {
        [Obsolete("RegisterChangeHandler is obsolete. Please use the AssetChanged event instead.")]
        public void RegisterChangeHandler(ChangeHandler handler)
        {
            throw new NotSupportedException("RegisterChangeHandler is obsolete. Please use the AssetChanged event instead.");
        }

        [Obsolete("ClearChangeHandler is now obsolete. You should unsubscribe from the AssetChanged event instead.")]
        public void ClearChangeHandler()
        {
            throw new NotSupportedException("ClearChangeHandler is now obsolete. You should use the AssetChanged event instead.");
        }
    }
}
#endif
